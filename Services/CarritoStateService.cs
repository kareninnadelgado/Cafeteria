using Cafeteria.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Cafeteria.Services;

public class CarritoStateService
{
    private readonly IJSRuntime _js;
    private readonly FirebaseAuthService _firestoreService;
    private List<CarritoItem> _items = new();

    // Evento que notificará a la barra de navegación para actualizar el MudBadge
    public event Action? OnCarritoCambiado;

    public List<CarritoItem> Items => _items;

    // Cuenta cuántos productos individuales hay en total (conteo acumulado)
    public int TotalProductos => _items.Sum(i => i.Cantidad);
    public double TotalPagar => _items.Sum(i => i.Subtotal);

    public CarritoStateService(IJSRuntime js, FirebaseAuthService firestoreService)
    {
        _js = js;
        _firestoreService = firestoreService;
    }

    // Inicializa el carrito cargando desde LocalStorage o Firestore
    public async Task InicializarCarritoAsync(string uid)
    {
        try
        {
            // Si no hay uid, cargamos solo el carrito anónimo local (clave: carrito_)
            if (string.IsNullOrEmpty(uid))
            {
                string? anonData = null;
                try
                {
                    anonData = await _js.InvokeAsync<string>("localStorage.getItem", "carrito_");
                }
                catch
                {
                    // localStorage blocked (Tracking Prevention u otras políticas). Dejamos anonData null
                    anonData = null;
                }

                if (!string.IsNullOrEmpty(anonData))
                {
                    _items = JsonSerializer.Deserialize<List<CarritoItem>>(anonData) ?? new();
                }
                else
                {
                    _items = new();
                }
                NotifyStateChanged();
                return;
            }

            // 1. Intentar cargar desde LocalStorage local con uid
            string? localData = null;
            try
            {
                localData = await _js.InvokeAsync<string>("localStorage.getItem", $"carrito_{uid}");
            }
            catch
            {
                // acceso a localStorage falló
                localData = null;
            }

            if (!string.IsNullOrEmpty(localData))
            {
                _items = JsonSerializer.Deserialize<List<CarritoItem>>(localData) ?? new();
            }
            else
            {
                // 2. Si no hay nada local para este uid, comprobar si existe un carrito anónimo y migrarlo
                string? anonData = null;
                try
                {
                    anonData = await _js.InvokeAsync<string>("localStorage.getItem", "carrito_");
                }
                catch
                {
                    anonData = null;
                }

                if (!string.IsNullOrEmpty(anonData))
                {
                    _items = JsonSerializer.Deserialize<List<CarritoItem>>(anonData) ?? new();
                    // Guardar migrado bajo el uid y en Firestore
                    await GuardarCambiosLocalYCloudAsync(uid);
                    // eliminar la copia anónima
                    try { await _js.InvokeVoidAsync("localStorage.removeItem", "carrito_"); } catch { }
                }
                else
                {
                    // 3. Si no hay copia local (o acceso a localStorage está bloqueado), intentar recuperar desde Firestore
                    try
                    {
                        _items = await _firestoreService.ObtenerCarritoAsync(uid);
                        await GuardarCambiosLocalYCloudAsync(uid);
                    }
                    catch
                    {
                        // En caso de fallo, mantener el carrito vacío pero no romper la UI
                        _items = new();
                    }
                }
            }
        }
        catch
        {
            _items = new();
        }
        NotifyStateChanged();
    }

    public async Task AgregarProductoAsync(string uid, Producto producto)
    {
        var itemExistente = _items.FirstOrDefault(i => i.ProductoId == producto.Id);

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
                ImagenUrl = producto.ImagenUrl
            });
        }

        await GuardarCambiosLocalYCloudAsync(uid);
    }

    public async Task ModificarCantidadAsync(string uid, string productoId, int nuevaCantidad)
    {
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
        _items.Clear();
        await GuardarCambiosLocalYCloudAsync(uid);
    }

    private async Task GuardarCambiosLocalYCloudAsync(string uid)
    {
        var json = JsonSerializer.Serialize(_items);
        // Persistencia en LocalStorage (proteger contra bloqueos del navegador)
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", $"carrito_{uid}", json);
        }
        catch
        {
            // Ignorar fallos de acceso a localStorage (Tracking Prevention, etc.)
        }
        // Persistencia en Firestore solo si hay un uid válido
        if (!string.IsNullOrEmpty(uid))
        {
            try
            {
                await _firestoreService.GuardarCarritoAsync(uid, _items);
            }
            catch
            {
                // Ignorar errores de red/Firestore para no romper la experiencia local
            }
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnCarritoCambiado?.Invoke();
    
}
