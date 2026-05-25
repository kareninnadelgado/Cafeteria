using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace Cafeteria.Services;

public class AuthStateService
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly IJSRuntime _js;
    
    public string? CurrentUserId { get; private set; }
    public string? CurrentUserEmail { get; private set; }
    public string? CurrentUserRol { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId);
    public bool IsAdmin => CurrentUserRol == "admin";
    public bool IsLoading { get; private set; } = true;

    public event Action? OnChange;

    public AuthStateService(ProtectedLocalStorage protectedLocalStorage, IJSRuntime js)
    {
        _protectedLocalStorage = protectedLocalStorage;
        _js = js;
    }

    // Llamar este método después del renderizado (desde MainLayout o App)
    public async Task CargarSesionDesdeLocalStorage()
    {
        try
        {
            Console.WriteLine("Cargando sesión desde localStorage...");
            var userId = await _protectedLocalStorage.GetAsync<string>("userId");
            var userEmail = await _protectedLocalStorage.GetAsync<string>("userEmail");
            var userRol = await _protectedLocalStorage.GetAsync<string>("userRol");
            
            Console.WriteLine($"userId obtenido: {userId.Success}, valor: {userId.Value}");
            Console.WriteLine($"userRol obtenido: {userRol.Success}, valor: {userRol.Value}");
            
            if (userId.Success && !string.IsNullOrEmpty(userId.Value))
            {
                CurrentUserId = userId.Value;
                CurrentUserEmail = userEmail.Success ? userEmail.Value : null;
                CurrentUserRol = userRol.Success ? userRol.Value : null;
                Console.WriteLine($"Sesión cargada: {CurrentUserId} - Rol: {CurrentUserRol}");
            }
            else
            {
                // Fallback: intentar leer del localStorage del navegador (útil en desarrollo o cuando ProtectedLocalStorage no está disponible)
                try
                {
                    // Intento inicial de leer desde window.localStorage
                    var jsUserId = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userId" });
                    var jsUserEmail = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userEmail" });
                    var jsUserRol = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userRol" });

                    // En entornos donde el JS interop se registra un poco más tarde, reintentamos una vez
                    if (string.IsNullOrEmpty(jsUserId))
                    {
                        Console.WriteLine("Fallback: intento adicional para leer localStorage...");
                        await Task.Delay(200);
                        jsUserId = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userId" });
                        jsUserEmail = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userEmail" });
                        jsUserRol = await _js.InvokeAsync<string>("localStorage.getItem", new object?[] { "userRol" });
                    }

                    if (!string.IsNullOrEmpty(jsUserId))
                    {
                        // Persistir en ProtectedLocalStorage para futuras cargas
                        await SetUser(jsUserId, jsUserEmail ?? "", jsUserRol ?? "alumno");
                        Console.WriteLine($"Sesión (fallback) cargada desde window.localStorage: {jsUserId} - Rol: {jsUserRol}");
                    }
                    else
                    {
                        Console.WriteLine("No se encontró sesión en localStorage");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No se encontró sesión en localStorage (error): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando sesión: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task SetUser(string userId, string email, string rol)
    {
        CurrentUserId = userId;
        CurrentUserEmail = email;
        CurrentUserRol = rol;
        IsLoading = false;
        
        await _protectedLocalStorage.SetAsync("userId", userId);
        await _protectedLocalStorage.SetAsync("userEmail", email);
        await _protectedLocalStorage.SetAsync("userRol", rol);
        
        NotifyStateChanged();
    }

    public async Task ClearUser()
    {
        CurrentUserId = null;
        CurrentUserEmail = null;
        CurrentUserRol = null;
        
        await _protectedLocalStorage.DeleteAsync("userId");
        await _protectedLocalStorage.DeleteAsync("userEmail");
        await _protectedLocalStorage.DeleteAsync("userRol");
        
        // También limpiar el localStorage base para evitar que el fallback recupere la sesión
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _js.InvokeVoidAsync("localStorage.removeItem", "userEmail");
            await _js.InvokeVoidAsync("localStorage.removeItem", "userRol");
        }
        catch { }

        // Borrar todas las cookies del navegador para un cierre de sesión total (Requerimiento de limpieza)
        try
        {
            await _js.InvokeVoidAsync("eval", @"
                document.cookie.split(';').forEach(function(c) {
                    document.cookie = c.trim().split('=')[0] + '=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/';
                });");
        }
        catch { }

        NotifyStateChanged();
    }

    public UsuarioActual? ObtenerUsuarioActual()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return null;
            
        return new UsuarioActual
        {
            Uid = CurrentUserId,
            Email = CurrentUserEmail,
            Rol = CurrentUserRol
        };
    }

    // Clase auxiliar
    public class UsuarioActual
    {
        public string Uid { get; set; } = "";
        public string? Email { get; set; }
        public string? Rol { get; set; }
    }

    public void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }

}