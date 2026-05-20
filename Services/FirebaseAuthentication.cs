namespace Cafeteria.Services;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cafeteria.Models;
using System.Text.Json.Nodes;

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

    // EL NUEVO MÉTODO PARA EL INSERT EN LA BASE DE DATOS
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

        return response.IsSuccessStatusCode;   
    }

    // OBTENER EL ROL DE UN USUARIO POR SU UID
    public async Task<string> ObtenerRolUsuario(string uid)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/usuarios/{uid}";
        
        var response = await _http.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Error al obtener usuario: {response.StatusCode}");
            return "alumno";
        }
        
        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        var rol = root?["fields"]?["rol"]?["stringValue"]?.GetValue<string>();
        
        return rol ?? "alumno";
    }

    // MÉTODO PARA CREAR UN PRODUCTO
    public async Task<bool> CrearProducto(string nombre, string descripcion, double precio, string categoria, bool disponible, string imagenUrl = "", List<string> personalizaciones = null!)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{Guid.NewGuid()}";
        
        // Construir el array de personalizaciones para Firestore
        var personalizacionesArray = new
        {
            arrayValue = new
            {
                values = personalizaciones?.Select(p => new { stringValue = p }).ToArray() ?? new object[] { }
            }
        };
        
        var datosProducto = new
        {
            fields = new
            {
                nombre = new { stringValue = nombre },
                descripcion = new { stringValue = descripcion },
                precio = new { doubleValue = precio },
                categoria = new { stringValue = categoria },
                disponible = new { booleanValue = disponible },
                imagen = new { stringValue = imagenUrl },
                personalizaciones = personalizacionesArray
            }
        };
        
        var json = JsonSerializer.Serialize(datosProducto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorFirestore = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ ERROR: {errorFirestore}");
        }
        
        return response.IsSuccessStatusCode;
    }

    // MÉTODO PARA OBTENER TODOS LOS PRODUCTOS
    public async Task<List<Producto>> ObtenerProductos()
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos";
        
        var response = await _http.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        
        var productos = new List<Producto>();
        
        var root = JsonNode.Parse(json);
        var documents = root?["documents"]?.AsArray();
        
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                var fields = doc?["fields"];
                if (fields != null)
                {
                    var producto = new Producto();
                    
                    // Extraer ID
                    var name = doc?["name"]?.GetValue<string>() ?? "";
                    if (!string.IsNullOrEmpty(name))
                    {
                        var segments = name.Split('/');
                        producto.Id = segments.Last();
                    }
                    
                    // Campos básicos
                    producto.Nombre = fields["nombre"]?["stringValue"]?.GetValue<string>() ?? "";
                    producto.Descripcion = fields["descripcion"]?["stringValue"]?.GetValue<string>() ?? "";
                    producto.Precio = fields["precio"]?["doubleValue"]?.GetValue<double>() ?? 0;
                    producto.Categoria = fields["categoria"]?["stringValue"]?.GetValue<string>() ?? "";
                    producto.Disponible = fields["disponible"]?["booleanValue"]?.GetValue<bool>() ?? true;
                    producto.ImagenUrl = fields["imagen"]?["stringValue"]?.GetValue<string>() ?? "";
                    
                    // 🔥 LEER PERSONALIZACIONES (array)
                    var personalizacionesNode = fields["personalizaciones"];
                    if (personalizacionesNode != null)
                    {
                        var arrayValue = personalizacionesNode["arrayValue"];
                        if (arrayValue != null)
                        {
                            var values = arrayValue["values"]?.AsArray();
                            if (values != null)
                            {
                                foreach (var item in values)
                                {
                                    var stringValue = item?["stringValue"]?.GetValue<string>();
                                    if (!string.IsNullOrEmpty(stringValue))
                                    {
                                        producto.Personalizaciones.Add(stringValue);
                                    }
                                }
                            }
                        }
                    }
                    
                    productos.Add(producto);
                }
            }
        }
        
        return productos;
    }

    // MÉTODO PARA OBTENER LAS CATEGORÍAS
    public async Task<List<string>> ObtenerCategorias()
    {
        var productos = await ObtenerProductos();
        return productos.Select(p => p.Categoria).Distinct().ToList();
    }

    // ACTUALIZAR PRODUCTO
    public async Task<bool> ActualizarProducto(string id, string nombre, string descripcion, double precio, string categoria, bool disponible, string imagenUrl, List<string> personalizaciones)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}";
        
        var personalizacionesArray = new
        {
            arrayValue = new
            {
                values = personalizaciones?.Select(p => new { stringValue = p }).ToArray() ?? new object[] { }
            }
        };
        
        var datosProducto = new
        {
            fields = new
            {
                nombre = new { stringValue = nombre },
                descripcion = new { stringValue = descripcion ?? "" },
                precio = new { doubleValue = precio },
                categoria = new { stringValue = categoria },
                disponible = new { booleanValue = disponible },
                imagen = new { stringValue = imagenUrl ?? "" },
                personalizaciones = personalizacionesArray
            }
        };
        
        var json = JsonSerializer.Serialize(datosProducto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Error actualizando: {error}");
        }
        
        return response.IsSuccessStatusCode;
    }

    // ELIMINAR PRODUCTO
    public async Task<bool> EliminarProducto(string id)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _http.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Error eliminando: {error}");
        }
        
        return response.IsSuccessStatusCode;
    }

    // CAMBIAR DISPONIBILIDAD RÁPIDO
    public async Task<bool> ToggleDisponibilidad(string id, bool disponibleActual)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}";
        
        var datos = new
        {
            fields = new
            {
                disponible = new { booleanValue = !disponibleActual }
            }
        };
        
        var json = JsonSerializer.Serialize(datos);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        
        return response.IsSuccessStatusCode;
    }

    public async Task<Producto?> ObtenerProductoPorId(string id)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}";
        
        var response = await _http.GetAsync(url);
        
        if (!response.IsSuccessStatusCode) return null;
        
        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        var fields = root?["fields"];
        
        if (fields == null) return null;
        
        var producto = new Producto
        {
            Id = id,
            Nombre = fields["nombre"]?["stringValue"]?.GetValue<string>() ?? "",
            Descripcion = fields["descripcion"]?["stringValue"]?.GetValue<string>() ?? "",
            Precio = fields["precio"]?["doubleValue"]?.GetValue<double>() ?? 0,
            Categoria = fields["categoria"]?["stringValue"]?.GetValue<string>() ?? "",
            Disponible = fields["disponible"]?["booleanValue"]?.GetValue<bool>() ?? true,
            ImagenUrl = fields["imagen"]?["stringValue"]?.GetValue<string>() ?? "",
        };
        
        // Leer personalizaciones
        var personalizacionesNode = fields["personalizaciones"];
        if (personalizacionesNode != null)
        {
            var arrayValue = personalizacionesNode["arrayValue"];
            if (arrayValue != null)
            {
                var values = arrayValue["values"]?.AsArray();
                if (values != null)
                {
                    foreach (var item in values)
                    {
                        var stringValue = item?["stringValue"]?.GetValue<string>();
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            producto.Personalizaciones.Add(stringValue);
                        }
                    }
                }
            }
        }
        
        return producto;
    }
}