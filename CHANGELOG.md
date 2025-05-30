Changelog - AprendeMasV2
[1.0.1] - 2025-05-29
Añadido

Clases placeholders para funcionalidades futuras: AudioConverter, AudioTranscriber, KeyboardSimulator, MdnsServer, y WebSocketServer en AprendeMasWindowsService.

[1.0.0] - 2025-05-24
Añadido

Interfaz gráfica (AprendeMas.UI) para controlar el servicio con MainForm y PipeClient.
Servicio de Windows (AprendeMasWindowsService) con comunicación por named pipes (AprendeMasPipe) usando PipeServer y gestión de notificaciones con NotificationManager.
Notificador (AprendeMasNotificationService) con ícono en la bandeja del sistema (NotifyIcon) y comunicación por named pipe (CanalNotificaciones) usando PipeServer.
Sistema de logging (Logger.cs) con soporte para INFO, WARNING, ERROR en todos los componentes.
Persistencia del estado de escucha en config.json para AprendeMasNotificationService.
Instalador con Inno Setup (Setup.iss).

Cambios

Implementación inicial del sistema de notificaciones con comunicación bidireccional entre componentes.
Configuración de logs en C:\Program Files (x86)\Aprende Mas\*\Logs.

Notas

La "tarea programada para iniciar el notificador al iniciar sesión" está configurada en el instalador, pero no se detalla en el código actual.
Clases placeholders (AudioConverter, AudioTranscriber, KeyboardSimulator, MdnsServer, WebSocketServer) están incluidas para futuras implementaciones.

[0.1.0] - 2025-05-22
Añadido

Esqueleto inicial del proyecto con tres componentes: UI, servicio, y notificador.
Configuración básica de GitHub (README.md, .gitignore).

