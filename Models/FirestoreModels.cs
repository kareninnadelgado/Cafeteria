namespace Cafeteria.Models;

// Respuesta de Firestore cuando pedimos una LISTA de documentos
public class FirestoreListaResponse
{
    public List<FirestoreDocumento> Documents { get; set; } = new();
}

// Un documento individual de Firestore
public class FirestoreDocumento
{
    public string Name { get; set; } = "";
    public FirestoreCampos Fields { get; set; } = new();
}

// Los campos dentro de un documento
public class FirestoreCampos
{
    public FirestoreString nombre { get; set; } = new();
    public FirestoreString descripcion { get; set; } = new();
    public FirestoreDouble precio { get; set; } = new();
    public FirestoreString categoria { get; set; } = new();
    public FirestoreBool disponible { get; set; } = new();
    public FirestoreString imagen { get; set; } = new();
    public FirestoreArray personalizaciones { get; set; } = new();
    public List<string> Personalizaciones { get; set; } = new();
}

// Tipos de datos que Firestore entiende
public class FirestoreString 
{ 
    public string stringValue { get; set; } = ""; 
}

public class FirestoreDouble 
{ 
    public double doubleValue { get; set; } 
}

public class FirestoreBool 
{ 
    public bool booleanValue { get; set; } 
}

// Clase para arrays (personalizaciones)
public class FirestoreArray
{
    public FirestoreArrayValues arrayValue { get; set; } = new();
}

public class FirestoreArrayValues
{
    public List<object> values { get; set; } = new();
}

// Modelo de Producto
public class Producto
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public double Precio { get; set; }
    public string Categoria { get; set; } = "";
    public bool Disponible { get; set; }
    public string ImagenUrl { get; set; } = "";
    public List<string> Personalizaciones { get; set; } = new();
}

// Modelo de Usuario
public class Usuario
{
    public string Uid { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Rol { get; set; } = "alumno"; // "alumno" o "admin"
    public DateTime FechaRegistro { get; set; }
}