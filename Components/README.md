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

### 🎮 Ludificación y Experiencia del Alumno (Fun UX)
- **Gestión de la Espera Pasiva:** Implementé una estrategia de UX para mitigar la ansiedad del alumno durante la preparación de su comida mediante un mini-juego interactivo directamente relacionado con la nutrición.
- **Reto Nutricional (Memorama):** Desarrollé un juego de memoria basado en el "Plato del Bien Comer" mediante el componente reutilizable `JuegoNutricion.razor`. El objetivo es emparejar alimentos del mismo grupo (Frutas/Verduras, Cereales, Proteínas), fomentando hábitos saludables de forma divertida.
- **Integración en el Flujo de Pedidos:** Añadí lógica dinámica en `MisPedidos.razor` para mostrar un acceso al seguimiento y al juego únicamente cuando el alumno tiene órdenes activas (estados "Pendiente" o "En Preparación").
- **Navegación Centralizada:** Actualicé la `NavBar` con un menú de perfil de usuario (`MudMenu`) que permite al alumno navegar de forma fluida entre el Menú Principal, sus Pedidos y el Reto Nutricional.
- **Diseño Adaptativo y Semántico:** El juego utiliza colores semánticos (Verde, Amarillo, Rojo) alineados con las normas oficiales de salud y una interfaz responsiva de MudBlazor que garantiza la jugabilidad en dispositivos móviles.

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

##  Cambios realizados (implementación)

A continuación describo en primera persona y con detalle las modificaciones que realicé en este módulo para añadir soporte de carrito, autenticación mejorada, modelos y ajustes en la interfaz siguiendo Material Design 3:

- **Servicios y estado del carrito:** Implementé `Services/CarritoStateService.cs`, un servicio responsable de mantener el estado del carrito en memoria (añadir, eliminar, limpiar, calcular totales) y de notificar a los componentes mediante un evento `OnChange`. Esto permite que `NavBar`, `Home`, `ProductoDetalle` y la página `Carrito` se sincronicen sin necesidad de pasar parámetros por la ruta.

- **Registro en la aplicación:** Modifiqué `Program.cs` para registrar `CarritoStateService`, `Services/FirebaseAuthentication.cs` y `Services/AuthStateService.cs` en el contenedor de dependencias. De esta forma los componentes pueden resolverlos con `@inject` o `[Inject]` y consumir el estado y la autenticación fácilmente.

- **Autenticación:** Actualicé `Services/FirebaseAuthentication.cs` para centralizar la comunicación con Firebase (login, logout, verificación y refresco de tokens) y `Services/AuthStateService.cs` para mantener el estado del usuario (perfil, roles, claims). Separé responsabilidades: la comunicación con Firebase queda en `FirebaseAuthentication` y la lógica de sesión/observabilidad en `AuthStateService`.

- **Modelo del carrito:** Añadí `Models/Carritos.cs` que contiene la entidad `CarritoItem` (propiedades como `Id`, `ProductoId`, `Nombre`, `Cantidad`, `PrecioUnitario`, `ImagenUrl` y `Subtotal` calculado). Tipar el carrito facilita el enlazado en Razor y la futura persistencia.

- **Componentes y páginas:** Realicé cambios en varios componentes para integrar la experiencia de carrito y autenticación:
  - `Components/NavBar.razor`: añadí contador reactivo del carrito y controles de sesión (login/logout) basados en `AuthStateService`.
  - `Components/Layout/MainLayout.razor`: ajusté la disposición para mostrar elementos globales (nav, badge del carrito) y asegurar consistencia visual.
  - `Components/Pages/Home.razor`: añadí acciones rápidas para agregar productos al carrito desde la vista principal.
  - `Components/Pages/Carrito.razor`: página nueva que lista items, permite editar cantidades, eliminar y ver totales; consume `CarritoStateService` para todas las operaciones.
  - `Components/Pages/ProductoDetalle.razor`: añadí la opción de seleccionar cantidad y agregar al carrito desde la ficha del producto.
  - `Components/Pages/PanelAdminPedidos.razor`: implementé un panel básico para ver pedidos y gestionar estados, accesible solo a roles `admin`.

- **Interactividad y control de accesos:** Integré visibilidad condicional en los menús y accesos rápidos del `Menu` para respetar roles definidos por `AuthStateService`.

### Uso de componentes y decisiones visuales (MudBlazor / Material Design)

Durante la implementación procuré mantener una guía visual consistente basada en Material Design 3 usando MudBlazor. Un ejemplo recurrente fue el uso de tarjetas con elevación y bordes suaves:

```html
<MudCard Elevation="2" ElevationHover="6" Style="border-radius: 16px/20px">
  <!-- Contenido de producto, formulario o tarjeta -->
</MudCard>
```

- **Por qué esta elección:** La elevación (`Elevation="2"`) establece una jerarquía visual base; `ElevationHover="6"` aumenta la retroalimentación al interactuar (hover/tap). Los radios entre 12 y 20 dp (representados aquí con `16px/20px`) siguen las recomendaciones de M3 para ofrecer bordes suaves y modernos.
- **Aplicación práctica:** Usé esta configuración en listados de productos, `ProductoDetalle`, las tarjetas de admin y en los ítems del carrito para mantener coherencia y mejor señalización de elementos interactivos.

### Justificación de la guía de estilo: elección de Material Design

Para la interfaz decidí seguir Material Design por las siguientes razones:
- **Multiplataforma:** La PWA corre en cualquier navegador; Material Design es agnóstico a plataformas mientras que otras guías (ej. Apple HIG) se orientan a un ecosistema concreto.
- **Optimización web:** Material Design ofrece patrones extensos para web responsiva, grids adaptativos y soporte para interacciones táctiles.
- **Ecosistema:** MudBlazor provee componentes compatibles con Blazor que implementan Material Design, lo que acelera desarrollo y asegura coherencia visual.

Estas razones guiaron las decisiones de componentes, elevaciones, radios, tipografías y comportamiento interactivo en todo el módulo.

---
