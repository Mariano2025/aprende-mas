# Guía de Desarrollo - AprendeMasV2

Esta guía explica cómo configurar el entorno de desarrollo, compilar, depurar, y contribuir al proyecto AprendeMasV2.

## Configuración del Entorno

1. **Instalar .NET 8.0 SDK**:
   - Descarga desde [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0).
   - Verifica: `dotnet --version`.

2. **Instalar Visual Studio 2022** (opcional):
   - Incluye la carga de trabajo ".NET desktop development".
   - Community Edition es gratuita.

3. **Instalar Inno Setup 6**:
   - Descarga desde [jrsoftware.org](https://jrsoftware.org/isinfo.php).
   - Necesario para generar el instalador.

4. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/Mariano2025/aprende-mas.git
   cd aprende-mas
   ```

## Compilación

1. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

2. **Compilar la solución**:
   ```bash
   dotnet build --configuration Release
   ```

3. **Generar el instalador**:
   - Abre `Setup.iss` en Inno Setup.
   - Compila (Ctrl+F9) para crear `AprendeMasSetup.exe`.

## Depuración

1. **Abrir en Visual Studio**:
   - Carga `AprendeMas.sln`.
   - Configura `AprendeMas.UI` como proyecto de inicio para probar la UI.
   - Usa `AprendeMasWindowsService` con `dotnet run` para depurar el servicio.

2. **Revisar logs**:
   - Ubicación: `C:\Program Files (x86)\Aprende Mas\[Componente]\Logs`.
   - Ejemplo: `Service.log`, `NotificationService.log`, `UI.log`.

3. **Puntos de interrupción**:
   - En `MainForm.cs` (botones), `AprendeMasService.cs` (manejo de comandos), o `Program.cs` (notificador).

## Estructura de Código

- **AprendeMas.UI**:
  - Windows Forms para la interfaz gráfica.
  - `PipeClient.cs`: Envía comandos (`START`, `STOP`) al servicio via named pipe (`AprendeMasPipe`).
- **AprendeMasWindowsService**:
  - Servicio de Windows que escucha comandos y envía notificaciones.
  - `PipeServer.cs`: Recibe comandos.
  - `NotificationManager.cs`: Gestiona el envío de notificaciones al notificador via pipe (`CanalNotificaciones`).
  - `Logger.cs`: Registra eventos en archivos de log.
- **AprendeMasNotificationService**:
  - Proceso que muestra notificaciones en la bandeja del sistema.
  - Escucha mensajes del servicio y guarda el estado en `config.json`.

## Pruebas

1. **Instalar**:
   - Ejecuta `AprendeMasSetup.exe` como administrador.
   - Verifica que el servicio (`AprendeMasWindowsService`) y el notificador estén corriendo:
     ```bash
     tasklist | findstr "AprendeMas"
     ```

2. **Probar la UI**:
   - Abre `AprendeMas.UI.exe`.
   - Haz clic en "Iniciar" y "Detener".
   - Confirma que el ícono en la bandeja del sistema cambia de estado.

3. **Revisar logs**:
   - Abre los archivos `.log` en `C:\Program Files (x86)\Aprende Mas\*\Logs`.

## Notas

- Los logs son thread-safe y se generan automáticamente.
- El notificador inicia con una tarea programada (`AprendeMasNotificationService`) al iniciar sesión.
- Usa `net start/stop AprendeMasWindowsService` para controlar el servicio manualmente.