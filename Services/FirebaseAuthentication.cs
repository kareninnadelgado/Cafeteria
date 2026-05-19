namespace Cafeteria.Services;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class FirebaseAuthService
{
    private readonly HttpClient _http;
    private string apiKey = "AIzaSyAmJKvnkWt0lTKBiySguBhlOOitSBQD5aI";

    // 🚨 REEMPLAZA ESTO CON EL ID REAL DE TU PROYECTO DE FIREBASE
    private string projectId = "cafeteria-2e9db"; 

    public FirebaseAuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> Login(string email, string password)
    {
        var data = new { email = email, password = password, returnSecureToken = true };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
            content
        );

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> Register(string email, string password)
    {
        var data = new { email = email, password = password, returnSecureToken = true };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}",
            content
        );

        return await response.Content.ReadAsStringAsync();
    }

    // 🔥 EL NUEVO MÉTODO PARA EL INSERT EN LA BASE DE DATOS
    public async Task<bool> CrearPerfilUsuario(string uid, string nombre, string email)
    {
        // Endpoint oficial de la API REST de Firestore
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/usuarios/{uid}";

        // Firestore pide que le digamos explícitamente el tipo de dato ("stringValue")
        var datosUsuario = new
        {
            fields = new
            {
                nombre = new { stringValue = nombre },
                correo = new { stringValue = email },
                fechaRegistro = new { stringValue = DateTime.UtcNow.ToString("o") },
                rol = new { stringValue = "alumno" }
            }
        };

        var json = JsonSerializer.Serialize(datosUsuario);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Usamos PATCH porque si el documento no existe lo crea, y si existe lo actualiza
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);

// 🚨 Si sigue fallando, esto te dirá exactamente qué pasó en la terminal de VS Code:
    if (!response.IsSuccessStatusCode)
    {
        var errorFirestore = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔥 ERROR REAL DE FIRESTORE: {errorFirestore}");
    }

    return response.IsSuccessStatusCode;    }
}