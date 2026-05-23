namespace Cafeteria.Services;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Cafeteria.Models;
using System.Text.Json.Nodes;

public class FirebaseAuthService
{
    private readonly HttpClient _http;
    private string apiKey = "AIzaSyAmJKvnkWt0lTKBiySguBhlOOitSBQD5aI";
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

    public async Task<bool> CrearPerfilUsuario(string uid, string nombre, string email)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/usuarios/{uid}";

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

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    public async Task<string> ObtenerRolUsuario(string uid)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/usuarios/{uid}";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode) return "alumno";

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        return root?["fields"]?["rol"]?["stringValue"]?.GetValue<string>() ?? "alumno";
    }

    // OBTENER CARRITO DESDE FIRESTORE
    public async Task<List<CarritoItem>> ObtenerCarritoAsync(string uid)
    {
        var items = new List<CarritoItem>();
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/carritos/{uid}";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode) return items;

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        var arrayValues = root?["fields"]?["items"]?["arrayValue"]?["values"]?.AsArray();

        if (arrayValues != null)
        {
            foreach (var itemNode in arrayValues)
            {
                var f = itemNode?["mapValue"]?["fields"];
                if (f != null)
                {
                    items.Add(new CarritoItem
                    {
                        ProductoId = f["productoId"]?["stringValue"]?.GetValue<string>() ?? "",
                        Nombre = f["nombre"]?["stringValue"]?.GetValue<string>() ?? "",
                        Precio = f["precio"]?["doubleValue"]?.GetValue<double>() ?? 0,
                        Cantidad = int.Parse(f["cantidad"]?["integerValue"]?.GetValue<string>() ?? "1"),
                        ImagenUrl = f["imagen"]?["stringValue"]?.GetValue<string>() ?? f["imagenUrl"]?["stringValue"]?.GetValue<string>() ?? "",
                        Personalizaciones = f["personalizaciones"]?["arrayValue"]?["values"]?.AsArray()
                            .Select(v => v?["stringValue"]?.GetValue<string>() ?? "")
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList() ?? new List<string>()
                    });
                }
            }
        }
        return items;
    }

    // PERSISTIR CARRITO EN FIRESTORE
    public async Task<bool> GuardarCarritoAsync(string uid, List<CarritoItem> items)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/carritos/{uid}";

        var fieldsItems = items.Where(i => i != null).Select(i => new
        {
            mapValue = new
            {
                fields = new
                {
                    productoId = new { stringValue = i.ProductoId ?? "" },
                    nombre = new { stringValue = i.Nombre ?? "" },
                    precio = new { doubleValue = i.Precio },
                    cantidad = new { integerValue = i.Cantidad.ToString() },
                    imagen = new { stringValue = i.ImagenUrl ?? "" },
                    personalizaciones = new { arrayValue = new { values = (i.Personalizaciones ?? new()).Select(p => new { stringValue = p }).ToArray() } }
                }
            }
        }).ToArray();

        var datosCarrito = new
        {
            fields = new
            {
                fechaActualizacion = new { stringValue = DateTime.UtcNow.ToString("o") },
                items = new { arrayValue = new { values = fieldsItems } }
            }
        };

        var json = JsonSerializer.Serialize(datosCarrito);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    // CREAR TICKET DE PEDIDO FINALIZADO
    public async Task<bool> CrearTicketPedidoAsync(string uid, string nombreAlumno, List<CarritoItem> items, double total)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/tickets";

        var fieldsProductos = items.Select(i => new
        {
            mapValue = new
            {
                fields = new
                {
                    nombre = new { stringValue = i.Nombre },
                    cantidad = new { integerValue = i.Cantidad.ToString() },
                    precio = new { doubleValue = i.Precio }
                }
            }
        }).ToArray();

        var datosTicket = new
        {
            fields = new
            {
                usuarioId = new { stringValue = uid },
                nombreAlumno = new { stringValue = nombreAlumno },
                fechaPedido = new { stringValue = DateTime.UtcNow.ToString("o") },
                total = new { doubleValue = total },
                estado = new { stringValue = "Pendiente" },
                productos = new { arrayValue = new { values = fieldsProductos } }
            }
        };

        var json = JsonSerializer.Serialize(datosTicket);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    // OBTENER PEDIDOS DE UN ALUMNO ESPECÍFICO
    public async Task<List<TicketPedido>> ObtenerPedidosPorUsuarioAsync(string uid)
    {
        var todos = await ObtenerTodosLosPedidosAdminAsync();
        return todos.Where(t => t.UsuarioId == uid).ToList();
    }

    // OBTENER TODOS LOS TICKETS (ADMIN / COCINA)
    public async Task<List<TicketPedido>> ObtenerTodosLosPedidosAdminAsync()
    {
        var lista = new List<TicketPedido>();
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/tickets";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode) return lista;

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonNode.Parse(json);
        var documents = root?["documents"]?.AsArray();

        if (documents != null)
        {
            foreach (var doc in documents)
            {
                var fields = doc?["fields"];
                if (fields == null) continue;

                var ticket = new TicketPedido();
                var nameProperty = doc?["name"]?.GetValue<string>() ?? "";
                ticket.Id = !string.IsNullOrEmpty(nameProperty) ? nameProperty.Split('/').Last() : Guid.NewGuid().ToString();

                ticket.UsuarioId = fields["usuarioId"]?["stringValue"]?.GetValue<string>() ?? "";
                ticket.NombreAlumno = fields["nombreAlumno"]?["stringValue"]?.GetValue<string>() ?? "";
                ticket.Estado = fields["estado"]?["stringValue"]?.GetValue<string>() ?? "Pendiente";
                ticket.Total = fields["total"]?["doubleValue"]?.GetValue<double>() ?? 0;

                if (DateTime.TryParse(fields["fechaPedido"]?["stringValue"]?.GetValue<string>(), out var fecha))
                {
                    ticket.FechaPedido = fecha;
                }

                var prodArray = fields["productos"]?["arrayValue"]?["values"]?.AsArray();
                if (prodArray != null)
                {
                    foreach (var pNode in prodArray)
                    {
                        var pf = pNode?["mapValue"]?["fields"];
                        if (pf != null)
                        {
                            ticket.Productos.Add(new CarritoItem
                            {
                                Nombre = pf["nombre"]?["stringValue"]?.GetValue<string>() ?? "",
                                Cantidad = int.Parse(pf["cantidad"]?["integerValue"]?.GetValue<string>() ?? "1"),
                                Precio = pf["precio"]?["doubleValue"]?.GetValue<double>() ?? 0
                            });
                        }
                    }
                }
                lista.Add(ticket);
            }
        }
        return lista;
    }

    // ACTUALIZAR ESTADO DEL PEDIDO DESDE EL PANEL DE COCINA
    public async Task<bool> ActualizarEstadoTicketAsync(string ticketId, string nuevoEstado)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/tickets/{ticketId}?updateMask.fieldPaths=estado";

        var datos = new { fields = new { estado = new { stringValue = nuevoEstado } } };
        var json = JsonSerializer.Serialize(datos);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    // ----- PRODUCTOS CRUD PARA FIRESTORE -----
    public async Task<List<Producto>> ObtenerProductos()
    {
        var lista = new List<Producto>();
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode) return lista;

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        // Intentamos deserializar a la clase estructurada
        FirestoreListaResponse? resp = null;
        try { resp = JsonSerializer.Deserialize<FirestoreListaResponse>(json, options); } catch { resp = null; }

        // También parseamos con JsonNode para campos complejos (arrays)
        var root = JsonNode.Parse(json);
        var documents = root?["documents"]?.AsArray();

        // Preferir usar el JsonNode 'documents' (más robusto ante variaciones)
        if (documents != null)
        {
            foreach (var docNode in documents)
            {
                var prod = new Producto();
                var nameProp = docNode?["name"]?.GetValue<string>() ?? "";
                prod.Id = !string.IsNullOrEmpty(nameProp) ? nameProp.Split('/').Last() : "";

                var f = docNode?["fields"];
                if (f != null)
                {
                    prod.Nombre = f?["nombre"]?["stringValue"]?.GetValue<string>() ?? "";
                    prod.Descripcion = f?["descripcion"]?["stringValue"]?.GetValue<string>() ?? "";
                    prod.Precio = f?["precio"]?["doubleValue"]?.GetValue<double?>() ?? 0;
                    prod.Categoria = f?["categoria"]?["stringValue"]?.GetValue<string>() ?? "";
                    prod.Disponible = f?["disponible"]?["booleanValue"]?.GetValue<bool?>() ?? false;
                    prod.ImagenUrl = f?["imagen"]?["stringValue"]?.GetValue<string>() ?? "";

                    prod.Personalizaciones = new List<string>();
                    var pvals = f?["personalizaciones"]?["arrayValue"]?["values"]?.AsArray();
                    if (pvals != null)
                    {
                        foreach (var v in pvals)
                        {
                            var s = v?["stringValue"]?.GetValue<string?>();
                            if (!string.IsNullOrEmpty(s)) prod.Personalizaciones.Add(s);
                        }
                    }
                }

                lista.Add(prod);
            }
        }
        else if (resp != null && resp.Documents != null)
        {
            for (int i = 0; i < resp.Documents.Count; i++)
            {
                var doc = resp.Documents[i];
                var prod = new Producto();

                // Id desde el nombre del documento
                prod.Id = !string.IsNullOrEmpty(doc.Name) ? doc.Name.Split('/').Last() : "";
                prod.Nombre = doc.Fields?.nombre?.stringValue ?? "";
                prod.Descripcion = doc.Fields?.descripcion?.stringValue ?? "";
                prod.Precio = doc.Fields?.precio?.doubleValue ?? 0;
                prod.Categoria = doc.Fields?.categoria?.stringValue ?? "";
                prod.Disponible = doc.Fields?.disponible?.booleanValue ?? false;
                prod.ImagenUrl = doc.Fields?.imagen?.stringValue ?? "";

                prod.Personalizaciones = new List<string>();
                if (doc.Fields?.personalizaciones?.arrayValue?.values != null)
                {
                    foreach (var val in doc.Fields.personalizaciones.arrayValue.values)
                    {
                        if (val is JsonElement) continue;
                        // intentar extraer como string si hay texto
                        try
                        {
                            var s = val?.ToString();
                            if (!string.IsNullOrEmpty(s)) prod.Personalizaciones.Add(s);
                        }
                        catch { }
                    }
                }

                lista.Add(prod);
            }
        }

        return lista;
    }

    public async Task<List<string>> ObtenerCategorias()
    {
        var productos = await ObtenerProductos();
        return productos.Select(p => p.Categoria).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
    }

    public async Task<Dictionary<string, int>> ObtenerCategoriasConConteo()
    {
        var productos = await ObtenerProductos();
        return productos
            .Where(p => !string.IsNullOrEmpty(p.Categoria))
            .GroupBy(p => p.Categoria)
            .ToDictionary(g => g.Key, g => g.Count());
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
            Nombre = fields?["nombre"]?["stringValue"]?.GetValue<string>() ?? "",
            Descripcion = fields?["descripcion"]?["stringValue"]?.GetValue<string>() ?? "",
            Precio = fields?["precio"]?["doubleValue"]?.GetValue<double?>() ?? 0,
            Categoria = fields?["categoria"]?["stringValue"]?.GetValue<string>() ?? "",
            Disponible = fields?["disponible"]?["booleanValue"]?.GetValue<bool?>() ?? false,
            ImagenUrl = fields?["imagen"]?["stringValue"]?.GetValue<string>() ?? "",
            Personalizaciones = new List<string>()
        };

        var pnode = fields?["personalizaciones"]?["arrayValue"]?["values"]?.AsArray();
        if (pnode != null)
        {
            foreach (var v in pnode)
            {
                var s = v?["stringValue"]?.GetValue<string?>();
                if (!string.IsNullOrEmpty(s)) producto.Personalizaciones.Add(s);
            }
        }

        return producto;
    }

    public async Task<bool> CrearProducto(Producto producto)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos";

        var personalizaciones = producto.Personalizaciones?.Select(p => new { stringValue = p }).ToArray() ?? Array.Empty<object>();

        var datos = new
        {
            fields = new
            {
                nombre = new { stringValue = producto.Nombre },
                descripcion = new { stringValue = producto.Descripcion },
                precio = new { doubleValue = producto.Precio },
                categoria = new { stringValue = producto.Categoria },
                disponible = new { booleanValue = producto.Disponible },
                imagen = new { stringValue = producto.ImagenUrl },
                personalizaciones = new { arrayValue = new { values = personalizaciones } }
            }
        };

        var json = JsonSerializer.Serialize(datos);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    // Sobrecarga para llamadas desde componentes (parámetros nombrados)
    public async Task<bool> CrearProducto(string nombre, string descripcion, double precio, string categoria, bool disponible, string imagenUrl, List<string> personalizaciones)
    {
        var producto = new Producto
        {
            Nombre = nombre,
            Descripcion = descripcion,
            Precio = precio,
            Categoria = categoria,
            Disponible = disponible,
            ImagenUrl = imagenUrl,
            Personalizaciones = personalizaciones ?? new List<string>()
        };

        return await CrearProducto(producto);
    }

    public async Task<bool> ActualizarProducto(Producto producto)
    {
        if (string.IsNullOrEmpty(producto.Id)) return false;
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{producto.Id}";

        var personalizaciones = producto.Personalizaciones?.Select(p => new { stringValue = p }).ToArray() ?? Array.Empty<object>();

        var datos = new
        {
            fields = new
            {
                nombre = new { stringValue = producto.Nombre },
                descripcion = new { stringValue = producto.Descripcion },
                precio = new { doubleValue = producto.Precio },
                categoria = new { stringValue = producto.Categoria },
                disponible = new { booleanValue = producto.Disponible },
                imagen = new { stringValue = producto.ImagenUrl },
                personalizaciones = new { arrayValue = new { values = personalizaciones } }
            }
        };

        var json = JsonSerializer.Serialize(datos);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    // Sobrecarga para llamadas desde componentes (parámetros nombrados)
    public async Task<bool> ActualizarProducto(string id, string nombre, string descripcion, double precio, string categoria, bool disponible, string imagenUrl, List<string> personalizaciones)
    {
        var producto = new Producto
        {
            Id = id,
            Nombre = nombre,
            Descripcion = descripcion,
            Precio = precio,
            Categoria = categoria,
            Disponible = disponible,
            ImagenUrl = imagenUrl,
            Personalizaciones = personalizaciones ?? new List<string>()
        };

        return await ActualizarProducto(producto);
    }

    public async Task<bool> EliminarProducto(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}";
        var response = await _http.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleDisponibilidad(string id, bool actual)
    {
        if (string.IsNullOrEmpty(id)) return false;
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/productos/{id}?updateMask.fieldPaths=disponible";

        var datos = new { fields = new { disponible = new { booleanValue = !actual } } };
        var json = JsonSerializer.Serialize(datos);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}