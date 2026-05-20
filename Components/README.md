# ☕ Sistema de Pedidos - Cafetería Escolar PWA

Este proyecto es una aplicación web progresiva (PWA) desarrollada en **Blazor** y conectada a **Google Firebase**. Su objetivo principal es optimizar el flujo de pedidos en la cafetería de la escuela, permitiendo a los alumnos ordenar desde el salón de clases y evitar filas.

## 🚀 Características Actuales

### Autenticación y Seguridad
- Autenticación segura de usuarios (Registro y Login) con Firebase Authentication.
- Validación estricta de credenciales en el frontend.
- Persistencia automática de perfiles en base de datos NoSQL con Cloud Firestore.
- Roles de usuario: **Admin** y **Cliente** con vistas diferenciadas.
- Protección de rutas administrativas con verificación de permisos.

### Interfaz de Usuario
- Implementación de **Material Design Guidelines** (Google).
- Uso de **MudBlazor** como biblioteca de componentes Material Design para Blazor.
- Pantallas implementadas:
  - `Home.razor` - Selección de categorías
  - `Menu.razor` - Visualización de productos con filtro por categoría
  - `ProductoDetalle.razor` - Detalle completo, personalizaciones y opciones admin
  - `AdminTools.razor` - Panel de administración con tarjetas de acceso
  - `AdminProductoNuevo.razor` - Formulario para agregar productos

### Funcionalidades de Productos
- Visualización de productos por categoría.
- Vista detallada de cada producto con imagen, descripción y precio.
- Sistema de **personalizaciones**.
- Indicadores visuales de disponibilidad.
- CRUD completo para administradores:
  - Crear nuevos productos
  - Editar productos existentes
  - Eliminar productos con confirmación

### Panel de Administración
- Acceso restringido solo para usuarios con rol `admin`.
- Tarjetas de navegación para:
  - Gestión del menú (CRUD completo)
  - Añadir nuevos productos
- Botón de cierre de sesión con diseño Material Design.

---

## 🤖 Uso de Inteligencia Artificial

En cumplimiento con los lineamientos de la materia, se declara el uso de herramientas de Inteligencia Artificial (IA Generativa) como copiloto de desarrollo durante el diseño e implementación de este módulo:

### Arquitectura de Software
- Se utilizó asistencia de IA para estructurar el servicio `FirebaseAuthService.cs` mediante peticiones HTTP limpias a la API REST de Google, optimizando el consumo de recursos sin dependencias pesadas.
- La IA ayudó en la transición de un esquema relacional tradicional (SQL) hacia un modelo NoSQL basado en colecciones y documentos optimizados para Cloud Firestore, previniendo costos de infraestructura.

### Seguridad y Validación
- Se generaron expresiones regulares (Regex) y anotaciones de datos (`DataAnnotationsValidator`) con soporte de IA para asegurar que las contraseñas cumplan con políticas de seguridad (mínimo 8 caracteres, 1 mayúscula y 1 símbolo) antes de interactuar con el servidor.

### Interfaz de Usuario y Material Design
- Se utilizó IA para adaptar componentes existentes a las **Material Design Guidelines**.
- La IA asistió en la implementación de principios clave de Material Design.
- Se corrigieron errores específicos del ecosistema Blazor/MudBlazor con ayuda de IA.

### Depuración y Optimización
- La IA colaboró en la identificación y resolución de errores de compilación.
- Se optimizaron los estilos.

> *El código fue revisado, depurado y adaptado manualmente por el equipo para integrarse al ecosistema de .NET 10. Se realizaron esfuerzos por seguir los principios de Material Design en la medida de lo posible.*