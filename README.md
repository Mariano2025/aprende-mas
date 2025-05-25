AprendeMasV2
Sistema de notificaciones para Windows que permite enviar mensajes a través de un servicio, con una interfaz gráfica y un notificador en la bandeja del sistema.
Requisitos

Windows 10 o superior
.NET 8.0 SDK
Visual Studio 2022 (opcional, para desarrollo)
Inno Setup 6 (para generar el instalador)

Instalación

Clona el repositorio:git clone https://github.com/Mariano2025/aprende-mas.git


Navega al directorio:cd aprende-mas


Compila la solución:dotnet build


Genera el instalador:
Abre Setup.iss en Inno Setup.
Compila (Ctrl+F9) para crear AprendeMasSetup.exe.


Ejecuta el instalador como administrador:.\AprendeMasSetup.exe



Uso

Abre la interfaz gráfica desde el Menú de Inicio o Escritorio (AprendeMas.UI.exe).
Usa los botones:
Iniciar Servicio: Activa las notificaciones (envía comando START).
Detener Servicio: Pausa las notificaciones (envía comando STOP).


Verifica el ícono en la bandeja del sistema:
Muestra notificaciones cuando el servicio está activo.
Permanece visible en modo pausa.



Estructura del Proyecto

AprendeMas.UI: Interfaz gráfica (Windows Forms) para controlar el servicio.
MainForm.cs: UI principal con botones de control.
PipeClient.cs: Envía comandos al servicio via named pipes.


AprendeMasWindowsService: Servicio de Windows que procesa comandos y envía notificaciones.
AprendeMasService.cs: Lógica del servicio.
NotificationManager.cs: Gestiona notificaciones.
PipeServer.cs, NotifierPipeClient.cs: Comunicación por pipes.
Logger.cs: Registro de eventos en archivos de log.


AprendeMasNotificationService: Notificador que muestra mensajes en la bandeja del sistema.
Program.cs: Gestiona el ícono y procesa mensajes del servicio.
Guarda el estado (START/STOP) en config.json.



Logs
Los logs se generan en:

C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService\Logs\Service.log
C:\Program Files (x86)\Aprende Mas\AprendeMasNotificationService\Logs\NotificationService.log
C:\Program Files (x86)\Aprende Mas\AprendeMas.UI\Logs\UI.log

Formato: [Timestamp] Nivel [Fuente] Mensaje (por ejemplo, [2025-05-24 20:41:23.456] INFO [ExecuteAsync] Servicio iniciando...).
Contribución

Haz un fork del repositorio.
Crea una rama (git checkout -b feature/nueva-funcion).
Commitea tus cambios (git commit -m "Añadir nueva función").
Sube la rama (git push origin feature/nueva-funcion).
Abre un Pull Request.

Licencia
MIT License - Ver LICENSE para detalles.
