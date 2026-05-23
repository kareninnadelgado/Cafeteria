namespace Cafeteria.Models;

public class CarritoItem
{
    public string ProductoId { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public double Precio { get; set; }
    public int Cantidad { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public List<string> Personalizaciones { get; set; } = new();
    // Propiedad calculada para el subtotal de este producto
    public double Subtotal => Precio * Cantidad;
}

// Modelo para la colección de Tickets (Pedidos finalizados)
public class TicketPedido
{
    public string Id { get; set; } = string.Empty; // ID autogenerado de Firestore
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public double Total { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Preparación, Listo, Entregado
    public List<CarritoItem> Productos { get; set; } = new();
}