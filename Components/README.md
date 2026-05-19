# ☕ Sistema de Pedidos - Cafetería Escolar PWA

Este proyecto es una aplicación web progresiva (PWA) desarrollada en **Blazor** y conectada a **Google Firebase**. Su objetivo principal es optimizar el flujo de pedidos en la cafetería de la escuela, permitiendo a los alumnos ordenar desde el salón de clases y evitar filas.

## 🚀 Características Actuales
* Autenticación segura de usuarios (Registro y Login) con Firebase Authentication.
* Validación estricta de credenciales en el frontend.
* Persistencia automática de perfiles en base de datos NoSQL con Cloud Firestore.

---

## 🤖 Uso de Inteligencia Artificial

En cumplimiento con los lineamientos de la materia, se declara el uso de herramientas de Inteligencia Artificial (IA Generativa) como copiloto de desarrollo durante el diseño e implementación de este módulo:

* **Arquitectura de Software:** Se utilizó asistencia de IA para estructurar el servicio `FirebaseAuthService.cs` mediante peticiones HTTP limpias a la API REST de Google, optimizando el consumo de recursos sin dependencias pesadas.
* **Seguridad y Validación:** Se generaron expresiones regulares (Regex) y anotaciones de datos (`DataAnnotationsValidator`) con soporte de IA para asegurar que las contraseñas cumplan con políticas de seguridad (mínimo 8 caracteres, 1 mayúscula y 1 símbolo) antes de interactuar con el servidor.
* **Modelado de Datos:** Se colaboró con la IA en la transición de un esquema relacional tradicional (SQL) hacia un modelo NoSQL basado en colecciones y documentos optimizados para Cloud Firestore, previniendo costos de infraestructura.

*El código fue revisado, depurado y adaptado manualmente por el equipo para integrarse al ecosistema de .NET 10.*