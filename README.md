# AprendeMasV2

Sistema de notificaciones para Windows que permite enviar mensajes a través de un servicio, controlado por una interfaz gráfica y mostrado en la bandeja del sistema. Desarrollado con .NET 8, usa named pipes para la comunicación y genera logs detallados para depuración.

## Características

- **Interfaz Gráfica**: Controla el servicio con botones "Iniciar" y "Detener" (`AprendeMas.UI`).
- **Servicio de Windows**: Procesa comandos y envía notificaciones (`AprendeMasWindowsService`).
- **Notificador**: Muestra un ícono en la bandeja del sistema y notificaciones (`AprendeMasNotificationService`).
- **Logging**: Registra eventos en archivos `.log` con niveles `INFO`, `WARNING`, `ERROR`.
- **Instalador**: Configura todo automáticamente con Inno Setup.

## Requisitos

- Windows 10 o superior
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (opcional, para desarrollo)
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (para generar el instalador)

## Instalación

1. **Clona el repositorio**:
   ```bash
   git clone https://github.com/Mariano2025/aprende-mas.git
   cd aprende-mas
   ```

2. **Compila la solución**:
   ```bash
   dotnet build --configuration Release
   ```

3. **Genera el instalador**:
   - Abre `Setup.iss` en Inno Setup.
   - Compila (Ctrl+F9) para crear `AprendeMasSetup.exe`.

4. **Ejecuta el instalador**:
   - Corre `AprendeMasSetup.exe` como administrador.
   - Instala en `C:\Program Files (x86)\Aprende Mas`.

## Uso

1. **Abrir la interfaz gráfica**:
   - Desde el Menú de Inicio o Escritorio, ejecuta `AprendeMas.UI.exe`.
   - O navega a `C:\Program Files (x86)\Aprende Mas\AprendeMas.UI` y corre el ejecutable.

2. **Controlar el servicio**:
   - **Iniciar Servicio**: Activa las notificaciones (envía comando `START`).
   - **Detener Servicio**: Pausa las notificaciones (envía comando `STOP`).

3. **Verificar el notificador**:
   - Un ícono aparece en la bandeja del sistema.
   - Muestra "Escuchando notificaciones..." cuando está activo o "Notificador activo, pero en pausa" cuando está detenido.
   - Las notificaciones aparecen como globos en la bandeja.

## Estructura del Proyecto

- **AprendeMas.UI**:
  - Interfaz gráfica en Windows Forms.
  - `MainForm.cs`: UI con botones de control.
  - `PipeClient.cs`: Envía comandos al servicio via named pipe (`AprendeMasPipe`).
- **AprendeMasWindowsService**:
  - Servicio de Windows que procesa comandos y envía notificaciones.
  - `AprendeMasService.cs`: Lógica principal del servicio.
  - `NotificationManager.cs`: Gestiona el envío de notificaciones.
  - `PipeServer.cs`: Escucha comandos en `AprendeMasPipe`.
  - `NotifierPipeClient.cs`: Envía notificaciones al notificador via `CanalNotificaciones`.
  - `Logger.cs`: Registra eventos en archivos de log.
- **AprendeMasNotificationService**:
  - Notificador que muestra mensajes en la bandeja del sistema.
  - `Program.cs`: Gestiona el ícono, procesa mensajes, y guarda el estado en `config.json`.

## Documentación

- [Guía de Desarrollo](DEVELOPMENT.md): Cómo configurar el entorno, compilar, y depurar.
- [Arquitectura](ARCHITECTURE.md): Diagrama de componentes y flujo de comunicación.
- [Flujo de Notificaciones](SEQUENCE.md): Diagrama de secuencia para comandos y notificaciones.
- [Diagrama de Clases](CLASS_DIAGRAM.md): Estructura interna de las clases.
- [Changelog](CHANGELOG.md): Historial de cambios y versiones.

## Logs

Los logs se generan automáticamente en:
- `C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService\Logs\Service.log`
- `C:\Program Files (x86)\Aprende Mas\AprendeMasNotificationService\Logs\NotificationService.log`
- `C:\Program Files (x86)\Aprende Mas\AprendeMas.UI\Logs\UI.log`

**Formato**: `[Timestamp] Nivel [Fuente] Mensaje`  
**Ejemplo**: `[2025-05-25 09:19:23.456] INFO [ExecuteAsync] Servicio iniciando...`

Para verificar:
```powershell
type "C:\Program Files (x86)\Aprende Mas\*\Logs\*.log"
```

## Contribución

¡Bienvenidos los aportes, cracks! 😎

1. Haz un fork del repositorio.
2. Crea una rama:
   ```bash
   git checkout -b feature/nueva-funcion
   ```
3. Commitea tus cambios:
   ```bash
   git commit -m "Añadir nueva función"
   ```
4. Sube la rama:
   ```bash
   git push origin feature/nueva-funcion
   ```
5. Abre un Pull Request en GitHub.

Sigue la [Guía de Desarrollo](DEVELOPMENT.md) para configurar tu entorno.

## Licencia

[MIT License](LICENSE) - Copyright (c) 2025 Mariano

## Contacto

Para dudas o sugerencias, abre un [issue](https://github.com/Mariano2025/aprende-mas/issues) en el repositorio. ¡A meterle flow! 🚀