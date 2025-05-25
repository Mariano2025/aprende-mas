# Flujo de Notificaciones - AprendeMasV2

## Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant U as Usuario
    participant UI as AprendeMas.UI
    participant S as AprendeMasWindowsService
    participant N as AprendeMasNotificationService

    U->>UI: Clic en "Iniciar Servicio"
    UI->>S: Enviar START (AprendeMasPipe)
    S->>N: Enviar START (CanalNotificaciones)
    N->>N: Actualizar config.json (IsListening = true)
    N->>N: Escribir log (NotificationService.log)
    N->>SystemTray: Mostrar "Escuchando notificaciones..."
    S->>S: Escribir log (Service.log)
    UI->>UI: Escribir log (UI.log)

    S->>N: Enviar notificación (CanalNotificaciones)
    N->>SystemTray: Mostrar notificación
    N->>N: Escribir log (NotificationService.log)
```

## Explicación

1. El usuario hace clic en "Iniciar Servicio" en la UI.
2. La UI envía el comando `START` al servicio via `AprendeMasPipe`.
3. El servicio envía `START` al notificador via `CanalNotificaciones`.
4. El notificador actualiza `config.json`, registra el evento, y actualiza el ícono.
5. Cuando el servicio envía una notificación, el notificador la muestra en la bandeja del sistema.