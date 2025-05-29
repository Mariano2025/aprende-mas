# Diagrama de Clases - AprendeMasV2

## Diagrama

```mermaid
classDiagram
    %% AprendeMas.UI
    class MainForm {
        -PipeClient pipeClient
        -Logger logger
        +btnStart_Click() void
        +btnStop_Click() void
    }
    class PipeClient {
        -Logger logger
        +SendCommandAsync(command) Task
    }

    %% AprendeMasWindowsService
    class AprendeMasService {
        -Logger logger
        -PipeServer pipeServer
        -NotificationManager notificationManager
        +ExecuteAsync(stoppingToken) Task
        +StartAsync(cancellationToken) Task
        +StopAsync(cancellationToken) Task
        -HandleCommand(command) void
    }
    class NotificationManager {
        -Logger logger
        -NotifierPipeClient notifierPipeClient
        -bool isRunning
        +StartAsync() Task
        +StopAsync() Task
        +SendNotificationAsync(message) Task
    }
    class PipeServer {
        -Logger logger
        -Action~string~ commandHandler
        +Start() void
        +Stop() void
    }
    class NotifierPipeClient {
        -Logger logger
        +SendMessageAsync(message) Task
    }
    class Logger {
        -string _logFilePath
        -object _lock
        +Logger(componentName, baseDirectory)
        +Info(message, source) void
        +Warning(message, source) void
        +Error(message, ex, source) void
    }

    %% AprendeMasNotificationService
    class NotificationService {
        -NotifyIcon notifyIcon
        -bool isListening
        -Logger logger
        -PipeServer pipeServer
        +Main(args) void
        -HandleMessage(mensaje) void
    }
    class PipeServer {
        -string _pipeName
        -Logger _logger
        -Action~string~ _messageHandler
        +PipeServer(pipeName, logger, messageHandler)
        +ListenAsync(cancellationToken) Task
    }
    class Config {
        +bool IsListening
    }

    %% Relaciones
    MainForm --> PipeClient : usa
    MainForm --> Logger : usa
    PipeClient --> Logger : usa
    AprendeMasService --> NotificationManager : contiene
    AprendeMasService --> PipeServer : contiene
    AprendeMasService --> Logger : usa
    NotificationManager --> NotifierPipeClient : usa
    NotificationManager --> Logger : usa
    PipeServer --> Logger : usa
    NotifierPipeClient --> Logger : usa
    NotificationService --> Config : usa
    NotificationService --> Logger : usa
    NotificationService --> PipeServer : contiene
```

## Explicación

- **AprendeMas.UI**:
  - `MainForm`: Interfaz gráfica que envía comandos (`START`, `STOP`) via `PipeClient`. Registra eventos con `Logger`.
  - `PipeClient`: Envía comandos al servicio a través del named pipe `AprendeMasPipe`.

- **AprendeMasWindowsService**:
  - `AprendeMasService`: Servicio de Windows que inicia `PipeServer` y `NotificationManager`. Procesa comandos y registra eventos.
  - `NotificationManager`: Gestiona notificaciones, enviándolas al notificador via `NotifierPipeClient`.
  - `PipeServer`: Escucha comandos en `AprendeMasPipe`.
  - `NotifierPipeClient`: Envía notificaciones al notificador via `CanalNotificaciones`.
  - `Logger`: Usado por todas las clases para registrar eventos en `[Componente].log`.

- **AprendeMasNotificationService**:
  - `NotificationService`: Clase estática que gestiona el ícono en la bandeja del sistema, escucha mensajes a través de `PipeServer` en el pipe `CanalNotificaciones`, y guarda el estado en `Config`.
  - `PipeServer`: Escucha mensajes asíncronamente en el pipe `CanalNotificaciones` y los procesa con un manejador de mensajes.
  - `Config`: Clase simple para serializar/deserializar el estado (`IsListening`) en `config.json`.

- **Relaciones**:
  - **Composición**: `AprendeMasService` contiene instancias de `NotificationManager` y `PipeServer`. `NotificationService` contiene una instancia de `PipeServer`.
  - **Dependencia**: `MainForm` depende de `PipeClient`, `NotificationService` depende de `PipeServer`, y casi todas las clases usan `Logger`.
  - **Uso**: `NotificationService` usa `Config` para persistir el estado.

## Notas

- Los logs se generan en `C:\Program Files (x86)\Aprende Mas\[Componente]\Logs`.
- Los pipes (`AprendeMasPipe`, `CanalNotificaciones`) son el núcleo de la comunicación.
- Clases no incluidas (por ejemplo, `WebSocketServer.cs`, `AudioConverter.cs`, `AudioTranscriber.cs`, `KeyboardSimulator.cs`, `MdnsServer.cs`) no están en el diagrama, ya que son placeholders sin implementación relevante.
