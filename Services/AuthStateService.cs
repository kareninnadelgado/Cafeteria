using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Cafeteria.Services;

public class AuthStateService
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    
    public string? CurrentUserId { get; private set; }
    public string? CurrentUserEmail { get; private set; }
    public string? CurrentUserRol { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId);
    public bool IsAdmin => CurrentUserRol == "admin";
    public bool IsLoading { get; private set; } = true;

    public event Action? OnChange;

    public AuthStateService(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
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
                Console.WriteLine("No se encontró sesión en localStorage");
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

    private void NotifyStateChanged() => OnChange?.Invoke();
}