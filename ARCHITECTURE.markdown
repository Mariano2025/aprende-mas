# Arquitectura de AprendeMasV2

## Diagrama de Componentes

```mermaid
graph TD
    A[AprendeMas.UI] -->|Comandos START/STOP<br>via AprendeMasPipe (out)| B[AprendeMasWindowsService]
    B -->|Notificaciones<br>via CanalNotificaciones (out)| C[AprendeMasNotificationService]
    C -->|Muestra ícono y notificaciones| D[System Tray]
    B -->|Escribe logs| E[AprendeMasService.log, NotificationManager.log, etc.]
    C -->|Escribe logs| F[NotificationService.log, PipeServer.log, etc.]
    A -->|Escribe logs| G[MainForm.log, PipeClient.log, etc.]
    C -->|Guarda estado| H[config.json]
```

## Explicación

- **AprendeMas.UI**: Envía comandos (`START`, `STOP`) al servicio a través de un named pipe unidireccional (`AprendeMasPipe`) usando `PipeClient`. La interfaz gráfica (`MainForm`) permite al usuario controlar el servicio.
- **AprendeMasWindowsService**: Procesa comandos recibidos a través de `AprendeMasPipe` (usando `PipeServer`) y gestiona notificaciones mediante `NotificationManager`, que envía mensajes al notificador a través del pipe unidireccional `CanalNotificaciones` (usando `NotifierPipeClient`).
- **AprendeMasNotificationService**: Recibe notificaciones a través de `CanalNotificaciones` (usando `PipeServer`), muestra un ícono en la bandeja del sistema (`System Tray`) con `NotifyIcon`, y guarda el estado de escucha en `config.json`.
- **Logs**: Cada componente genera logs en `C:\Program Files (x86)\Aprende Mas\[Componente]\Logs`, con nombres específicos por clase (por ejemplo, `MainForm.log`, `AprendeMasService.log`, `NotificationService.log`).

## Notas

- Los pipes (`AprendeMasPipe`, `CanalNotificaciones`) son unidireccionales y forman el núcleo de la comunicación entre componentes.
- Clases como `AudioConverter`, `AudioTranscriber`, `KeyboardSimulator`, `MdnsServer`, y `WebSocketServer` son placeholders para funcionalidades futuras y no están incluidas en el diagrama.
- Los directorios de logs requieren permisos de escritura, por lo que los componentes deben ejecutarse con privilegios adecuados.