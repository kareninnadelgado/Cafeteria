using Cafeteria.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Cafeteria.Services;

public class CarritoStateService : IDisposable
{
    private readonly IJSRuntime _js;
    private readonly FirebaseAuthService _firestoreService;
    private readonly AuthStateService _authState;
    private List<CarritoItem> _items = new();
    private bool _isInitialized = false;
    private string? _ultimoUidInicializado = null;

    // Evento que notificará a la barra de navegación para actualizar el MudBadge
    public event Action? OnCarritoCambiado;

    public List<CarritoItem> Items => _items;

    // Cuenta cuántos productos individuales hay en total (conteo acumulado)
    public int TotalProductos => _items.Sum(i => i.Cantidad);
    public double TotalPagar => _items.Sum(i => i.Subtotal);

    public CarritoStateService(IJSRuntime js, FirebaseAuthService firestoreService, AuthStateService authState)
    {
        _js = js;
        _firestoreService = firestoreService;
        _authState = authState;

        // Suscripción automática: cuando el estado de autenticación cambie (ej. Logout), limpiamos el carrito
        _authState.OnChange += HandleAuthStateChanged;
    }

    private void HandleAuthStateChanged()
    {
        if (!_authState.IsAuthenticated)
        {
            LimpiarEstadoLocal();
        }
    }

    // Inicializa el carrito cargando desde LocalStorage o Firestore
    public async Task InicializarCarritoAsync(string uid)
    {
        if (_isInitialized && _ultimoUidInicializado == uid) return;

        try
        {
            _isInitialized = false; // Bloqueamos temporalmente mientras cargamos
            _ultimoUidInicializado = uid;
            if (string.IsNullOrEmpty(uid))
            {
                var anonData = await TryGetLocalStorage("carrito_");
                if (!string.IsNullOrEmpty(anonData))
                {
                    try
                    {
                        _items = JsonSerializer.Deserialize<List<CarritoItem>>(anonData) ?? new();
                    }
                    catch
                    {
                        _items = new();
                    }
                }
                _isInitialized = true;
                NotifyStateChanged();
                return;
            }

            // Cargar desde Firestore (Fuente de verdad)
            try
            {
                var cloudItems = await _firestoreService.ObtenerCarritoAsync(uid);
                // Si hay items en la nube, los tomamos como fuente de verdad
                if (cloudItems != null && cloudItems.Any())
                {
                    _items = cloudItems;
                    _isInitialized = true;
                    // sincronizar local
                    await GuardarCambiosLocalYCloudAsync(uid);
                    NotifyStateChanged();
                    return;
                }
            }
            catch
            { }

            var localData = await TryGetLocalStorage($"carrito_{uid}");
            if (!string.IsNullOrEmpty(localData))
            {
                try
                {
                    _items = JsonSerializer.Deserialize<List<CarritoItem>>(localData) ?? new();
                }
                catch
                {
                    _items = new();
                }
            }
            else
            {
                var anonData = await TryGetLocalStorage("carrito_");
                if (!string.IsNullOrEmpty(anonData))
                {
                    try
                    {
                        _items = JsonSerializer.Deserialize<List<CarritoItem>>(anonData) ?? new();
                    }
                    catch
                    {
                        _items = new();
                    }
                    // Guardar migrado bajo el uid y en Firestore
                    await GuardarCambiosLocalYCloudAsync(uid);
                    // eliminar la copia anónima
                    try { await _js.InvokeVoidAsync("localStorage.removeItem", "carrito_"); } catch { }
                }
                else
                {
                    // 3. Si no hay copia local (o acceso a localStorage está bloqueado), mantener vacío
                    _items = new();
                }
            }
        }
        catch
        {
            _items = new();
        }
        _isInitialized = true;
        NotifyStateChanged();
    }

    public async Task AgregarProductoAsync(string uid, Producto producto, List<string>? personalizaciones = null)
    {
        if (!_isInitialized) await InicializarCarritoAsync(uid);
        if (string.IsNullOrEmpty(uid)) uid = _authState.CurrentUserId ?? string.Empty;
        var seleccionadas = personalizaciones ?? new List<string>();

        // Buscamos si ya existe el MISMO producto con las MISMAS personalizaciones
        var itemExistente = _items.FirstOrDefault(i => i.ProductoId == producto.Id 
            && i.Personalizaciones.SequenceEqual(seleccionadas));

        if (itemExistente != null)
        {
            itemExistente.Cantidad++;
        }
        else
        {
            _items.Add(new CarritoItem
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Cantidad = 1,
                ImagenUrl = producto.ImagenUrl,
                Personalizaciones = seleccionadas
            });
        }

        await GuardarCambiosLocalYCloudAsync(uid);
    }

    public async Task ModificarCantidadAsync(string uid, string productoId, int nuevaCantidad)
    {
        if (string.IsNullOrEmpty(uid)) uid = _authState.CurrentUserId ?? string.Empty;

        var item = _items.FirstOrDefault(i => i.ProductoId == productoId);
        if (item != null)
        {
            if (nuevaCantidad <= 0) _items.Remove(item);
            else item.Cantidad = nuevaCantidad;

            await GuardarCambiosLocalYCloudAsync(uid);
        }
    }

    public async Task VaciarCarritoAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid)) uid = _authState.CurrentUserId ?? string.Empty;

        _items.Clear();
        await GuardarCambiosLocalYCloudAsync(uid);
    }

    // Limpia el estado en memoria para cuando un usuario cierra sesión, 
    // sin afectar los datos guardados en Firestore.
    public void LimpiarEstadoLocal()
    {
        _items.Clear();
        _isInitialized = false;
        _ultimoUidInicializado = null;
        NotifyStateChanged();
    }

    private async Task GuardarCambiosLocalYCloudAsync(string uid)
    {
        // No guardar si no hemos terminado de inicializar para evitar sobrescribir con vacío
        if (!_isInitialized) return;

        var json = JsonSerializer.Serialize(_items);
        var key = string.IsNullOrEmpty(uid) ? "carrito_" : $"carrito_{uid}";
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
            Console.WriteLine($"LocalStorage: guardado bajo clave={key}, items={_items.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage: fallo guardando clave={key}: {ex.Message}");
        }

        // Persistencia en Firestore solo si hay un uid válido
        if (!string.IsNullOrEmpty(uid))
        {
            try
            {
                var ok = await _firestoreService.GuardarCarritoAsync(uid, _items);
                Console.WriteLine($"Firestore: intento guardar carrito uid={uid}, items={_items.Count}, ok={ok}");
                if (!ok)
                {
                    Console.WriteLine($"Warning: guardar carrito en Firestore devolvió false para uid={uid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception al guardar carrito en Firestore: {ex.Message}");
            }
        }

        NotifyStateChanged();
    }

    private async Task<string?> TryGetLocalStorage(string key)
    {
        try 
        { 
            // Solo intentar si JS está disponible (evita error en prerendering)
            return await _js.InvokeAsync<string>("localStorage.getItem", key); 
        }
        catch { return null; }
    }

    private void NotifyStateChanged() => OnCarritoCambiado?.Invoke();
    
    public void Dispose()
    {
        // Desvincular el evento para evitar fugas de memoria
        _authState.OnChange -= HandleAuthStateChanged;
    }
}
