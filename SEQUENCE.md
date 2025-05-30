# Flujo de Notificaciones - AprendeMasV2

## Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant U as Usuario
    participant M as MainForm (AprendeMas.UI)
    participant PC as PipeClient (AprendeMas.UI)
    participant AS as AprendeMasService (AprendeMasWindowsService)
    participant NM as NotificationManager (AprendeMasWindowsService)
    participant NPC as NotifierPipeClient (AprendeMasWindowsService)
    participant NS as NotificationService (AprendeMasNotificationService)
    participant PS as PipeServer (AprendeMasNotificationService)
    participant ST as SystemTray

    %% Flujo para START
    U->>M: Clic en "Iniciar Servicio"
    M->>PC: SendCommandAsync("START")
    PC->>AS: Enviar START (AprendeMasPipe)
    AS->>NM: StartAsync()
    NM->>NPC: SendMessageAsync("START")
    NPC->>PS: Enviar START (CanalNotificaciones)
    PS->>NS: HandleMessage("START")
    NS->>NS: Actualizar config.json (IsListening = true)
    NS->>NS: Escribir log (NotificationService.log)
    NS->>ST: Mostrar "Escuchando notificaciones..."
    M->>M: Mostrar MessageBox("Servicio iniciado correctamente")
    M->>M: Escribir log (MainForm.log)
    AS->>AS: Escribir log (AprendeMasService.log)
    NM->>NM: Escribir log (NotificationManager.log)

    %% Flujo para notificación
    Note over AS,NM: Cuando NotificationManager está activo (isRunning = true)
    AS->>NM: SendNotificationAsync(message)
    NM->>NPC: SendMessageAsync(message)
    NPC->>PS: Enviar mensaje (CanalNotificaciones)
    PS->>NS: HandleMessage(message)
    NS->>ST: Mostrar notificación
    NS->>NS: Escribir log (NotificationService.log)
    NM->>NM: Escribir log (NotificationManager.log)

    %% Flujo para STOP
    U->>M: Clic en "Detener Servicio"
    M->>PC: SendCommandAsync("STOP")
    PC->>AS: Enviar STOP (AprendeMasPipe)
    AS->>NM: StopAsync()
    NM->>NPC: SendMessageAsync("STOP")
    NPC->>PS: Enviar STOP (CanalNotificaciones)
    PS->>NS: HandleMessage("STOP")
    NS->>NS: Actualizar config.json (IsListening = false)
    NS->>NS: Escribir log (NotificationService.log)
    NS->>ST: Mostrar "Notificador activo, pero en pausa."
    M->>M: Mostrar MessageBox("Servicio detenido correctamente")
    M->>M: Escribir log (MainForm.log)
    AS->>AS: Escribir log (AprendeMasService.log)
    NM->>NM: Escribir log (NotificationManager.log)
```

## Explicación

1. El usuario hace clic en "Iniciar Servicio" en `MainForm` (UI).
2. `MainForm` usa `PipeClient` para enviar el comando `START` a `AprendeMasService` vía `AprendeMasPipe`.
3. `AprendeMasService` invoca `StartAsync` en `NotificationManager`, que usa `NotifierPipeClient` para enviar `START` a `NotificationService` vía `CanalNotificaciones`.
4. `NotificationService` procesa el comando con `HandleMessage`, actualiza `config.json` (`IsListening = true`), registra el evento en `NotificationService.log`, y muestra "Escuchando notificaciones..." en la bandeja del sistema.
5. `MainForm` muestra un `MessageBox` confirmando el éxito y registra el evento en `MainForm.log`. `AprendeMasService` y `NotificationManager` también registran eventos en sus respectivos logs.
6. Cuando `NotificationManager` está activo (`isRunning = true`), `AprendeMasService` puede enviar notificaciones vía `SendNotificationAsync`. Estas se envían a `NotificationService` a través de `CanalNotificaciones`, se muestran en la bandeja del sistema, y se registran en `NotificationService.log` y `NotificationManager.log`.
7. El usuario puede hacer clic en "Detener Servicio" en `MainForm`, lo que envía el comando `STOP` de manera similar, actualizando `config.json` (`IsListening = false`), mostrando "Notificador activo, pero en pausa." en la bandeja, y registrando eventos en los logs.

## Notas

- Los pipes (`AprendeMasPipe`, `CanalNotificaciones`) son unidireccionales y manejan la comunicación entre componentes.
- Los logs se generan en `C:\Program Files (x86)\Aprende Mas\[Componente]\Logs` con nombres específicos por clase (por ejemplo, `MainForm.log`, `AprendeMasService.log`, `NotificationService.log`).
- Los errores (por ejemplo, fallos de conexión al pipe) se manejan en cada componente, registrándose en los logs y mostrando mensajes al usuario cuando corresponde (por ejemplo, `MessageBox` en la UI).
- Clases como `AudioConverter`, `AudioTranscriber`, `KeyboardSimulator`, `MdnsServer`, y `WebSocketServer` no están incluidas, ya que son placeholders para futuras implementaciones.
