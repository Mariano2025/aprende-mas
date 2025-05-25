# Changelog - AprendeMasV2

## [1.0.0] - 2025-05-24

### Añadido
- Interfaz gráfica (`AprendeMas.UI`) para controlar el servicio.
- Servicio de Windows (`AprendeMasWindowsService`) con comunicación por named pipes.
- Notificador (`AprendeMasNotificationService`) con ícono en la bandeja del sistema.
- Sistema de logging (`Logger.cs`) con soporte para `INFO`, `WARNING`, `ERROR`.
- Tarea programada para iniciar el notificador al iniciar sesión.
- Instalador con Inno Setup (`Setup.iss`).

### Cambios
- Implementación inicial del sistema de notificaciones.
- Configuración de logs en `C:\Program Files (x86)\Aprende Mas\*\Logs`.

## [0.1.0] - 2025-05-22

### Añadido
- Esqueleto inicial del proyecto con tres componentes: UI, servicio, y notificador.
- Configuración básica de GitHub (`README.md`, `.gitignore`).