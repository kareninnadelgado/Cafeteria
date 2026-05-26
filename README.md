# Sistema de Pedidos - Cafetería Universitaria PWA

Aplicación Web Progresiva (PWA) interactiva diseñada para la comunidad de la Universidad de Colima, orientada a optimizar el flujo de pedidos en la cafetería escolar, eliminar filas y reducir los tiempos de espera de los estudiantes.

---

## Información General

* **Materia:** Diseño y Evaluación de Interfaces de Usuario (DEIU)
* **Profesor:** Santana Mancilla Pedro César
* **Equipo / Integrantes:** * Delgado García, Jiménez Sandoval, Lugo Cruz, Torres Barajas
 

---

## Enlace al Prototipo Funcional

El prototipo funcional se encuentra desplegado en producción y es accesible de forma pública a través del siguiente enlace:
**https://cafeteria-q208.onrender.com/**

---

## Tecnologías y Versiones Utilizadas

* **Framework Core:** Blazor Server (.NET 10.0)
* **Librería de Componentes UI:** MudBlazor (v9.4.0) basado en Material Design 3
* **Base de Datos y Autenticación:** Google Firebase (Cloud Firestore NoSQL & Firebase Auth REST API)
* **Contenedor de Despliegue:** Docker (Imagen base .NET SDK / ASP.NET 10.0)
* **Hosting:** Render.com (Cloud Web Services)

---

## Funcionalidades Implementadas

### Módulo de Clientes (Alumnos)
* **Autenticación Segura:** Registro e inicio de sesión con validación estricta de contraseñas mediante Firebase.
* **Menú Guiado (Card Sorting):** Visualización del catálogo de alimentos ordenado bajo la estructura mental real de los estudiantes, con filtros rápidos por categoría.
* **Ficha de Producto:** Vista detallada de alimentos con selector de cantidad e inclusión de personalizaciones (ej. sin azúcar, sin cebolla).
* **Gestión de Carrito Activo:** Control en memoria y persistencia automática híbrida (LocalStorage del navegador y sincronización en la nube con Firestore).
* **Módulo de Finalización (WhatsApp API):** El botón de confirmación procesa el carrito, genera un ticket en Firestore y dispara una comanda estructurada por WhatsApp directamente a la cocina de la cafetería.
* **Historial de Pedidos:** Consulta de compras anteriores mediante un diseño de acordeones táctiles que reduce la sobrecarga visual.

### Módulo de Administración (Personal de Cocina)
* **Protección de Rutas:** Panel restringido exclusivamente para usuarios con rol admin.
* **CRUD de Catálogo:** Panel completo para crear, editar, eliminar y cambiar la disponibilidad de cualquier producto en tiempo real.
* **Panel de Cocina Reactivo:** Visualización instantánea de los pedidos entrantes y actualización manual del estado del pedido ("Pendiente", "En Preparación", "Listo para Recoger", "Entregado").

### Infraestructura PWA & Fun UX
* **Soporte Offline y Caching:** Configuración de manifest.json y sw.js (Service Worker) para almacenamiento en caché local.
* **Instalación Nativa:** Diseño responsivo adaptable que permite "Agregar a la pantalla de inicio" en iOS y Android, eliminando las barras de navegación web.
* **Ludificación (Espera Activa):** Integración de un minijuego de Memorama Nutricional interactivo basado en el Plato del Bien Comer para mitigar la ansiedad de los alumnos durante el tiempo de espera.

---

## Instrucciones de Instalación y Ejecución Local

### Requisitos
* Tener instalado el SDK de .NET 10.

### Ejecución en Modo Desarrollo
1. Clona este repositorio en tu máquina local:
   ```bash
git clone [https://github.com/kareninnadelgado/Cafeteria.git](https://github.com/kareninnadelgado/Cafeteria.git)
