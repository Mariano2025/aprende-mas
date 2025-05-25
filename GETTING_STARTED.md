# Cómo Empezar con AprendeMasV2

¡Bienvenido, crack! 😎 Esta guía te llevará de cero a entender el proyecto **AprendeMasV2**, un sistema de notificaciones para Windows con interfaz gráfica, servicio, y notificador en la bandeja del sistema. Si eres un nuevo desarrollador, aquí sabrás por dónde empezar, qué revisar, y cómo meterte al código como pro. 🚀

## ¿Qué es AprendeMasV2?

Es una app que envía notificaciones en Windows, controlada por una UI y mostrada en la bandeja del sistema. Usa named pipes para comunicación, logs detallados, y un instalador para configurarlo todo. Está dividido en tres componentes:
- **AprendeMas.UI**: Interfaz gráfica para enviar comandos (`START`, `STOP`).
- **AprendeMasWindowsService**: Servicio que procesa comandos y envía notificaciones.
- **AprendeMasNotificationService**: Notificador que muestra mensajes en la bandeja.

## Paso 1: Obtén la Visión General

**Objetivo**: Entender el propósito y la estructura del proyecto.

1. **Lee el `README.md`**:
   - Abre [`README.md`](./README.md) en el repositorio (`https://github.com/Mariano2025/aprende-mas`).
   - **Qué buscar**:
     - **Características**: Qué hace la app (botones, notificaciones, logs).
     - **Estructura del Proyecto**: Los tres componentes y sus roles.
     - **Instalación y Uso**: Cómo ejecutarla.
   - **Tiempo**: 5-10 minutos.

2. **Explora la Documentación**:
   - Revisa los archivos en la raíz:
     - [`CHANGELOG.md`](./CHANGELOG.md): Historial de cambios (qué se ha añadido).
     - [`DEVELOPMENT.md`](./DEVELOPMENT.md): Guía para configurar el entorno.
   - **Nota**: Estos archivos te dan contexto sobre la evolución y setup del proyecto.

**Resultado**: Sabes qué hace la app, sus componentes, y cómo se instala.

---

## Paso 2: Entiende la Arquitectura

**Objetivo**: Visualizar cómo encajan las piezas.

1. **Diagrama de Componentes**:
   - Abre [`ARCHITECTURE.md`](./ARCHITECTURE.md).
   - Muestra cómo `AprendeMas.UI` envía comandos al servicio via `AprendeMasPipe`, y cómo el servicio notifica al notificador via `CanalNotificaciones`.
   - **Qué buscar**:
     - Los tres componentes y sus conexiones.
     - **Logs**: Dónde se generan (`Service.log`, `NotificationService.log`, `UI.log`).
   - **Tiempo**: 5 minutos.

2. **Diagrama de Clases**:
   - Abre [`CLASS_DIAGRAM.md`](./CLASSES_DIAGRAM.md).
   - Detalla las clases principales (`MainForm`, `AprendeMasService`, `Logger`, etc.) y sus relaciones.
   - **Qué buscar**:
     - Cómo `MainForm` usa `PipeClient` para enviar comandos.
     - Cómo `AprendeMasService` contiene `NotificationManager` y `PipeServer`.
     - El rol central de `Logger` en todos los componentes.
   - **Tiempo**: 10 minutos.

3. **Diagrama de Secuencia**:
   - Abre [`SEQUENCE.md`](./SEQUENCE.md).
   - Muestra el flujo de un comando `START` desde la UI hasta la notificación.
   - **Qué buscar**:
     - El orden de interacción: UI → Servicio → Notificador → Bandeja.
     - **Logs** y `config.json` actualizados en el proceso.
   - **Tiempo**: 5 minutos.

**Resultado**: Entiendes la arquitectura, las clases clave, y el flujo de un comando.

---

## Paso 3: Configura y Ejecuta la App

**Objetivo**: Ver la app en acción.

1. **Configura el Entorno**:
   - Sigue la sección **"Instalación"** en `README.md`:
     - Instala [.NET Framework 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
     - (Opcional) Instala Visual Studio 2022 con la carga de trabajo ".NET desktop development".
     - Instala [Inno Setup 6](https://jrsoftware.org/isinfo.php).
     - Clona el repo:
       ```bash
       git clone https://github.com/Mariano2025/aprende-mas.git
       cd aprende-mas
       ```
   - Compila:
     ```bash
     dotnet build --configuration Release
     ```
   - Genera el instalador con `Setup.iss` en Inno Setup (Ctrl+F9).
   - Instala con `AprendeMasSetup.exe` (como administrador).
   - **Tiempo**: 20-30 minutos.

2. **Ejecuta la App**:
   - Abre `AprendeMas.UI.exe` desde `C:\Program Files (x86)\Aprende Mas\AprendeMas.UI` o el Menú de Inicio.
   - Haz clic en **"Iniciar Servicio"** y observa el ícono en la bandeja del sistema (debería decir "Escuchando notificaciones...").
   - Haz clic en **"Detener Servicio"** y verifica que el ícono cambie a "Notificador activo, pero en pausa".
   - **Tiempo**: 5 minutos.

3. **Revisa los Logs**:
   - Abre los archivos de log:
     - `C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService\Logs\Service.log`
     - `C:\Program Files (x86)\Aprende Mas\AprendeMasNotificationService\Logs\NotificationService.log`
     - `C:\Program Files (x86)\Aprende Mas\AprendeMas.UI\Logs\UI.log`
   - Usa:
     ```powershell
     type "C:\Program Files (x86)\Aprende Mas\*\Logs\*.log"
     ```
   - **Qué buscar**:
     - Entradas como `[2025-05-25 09:28:23.456] INFO [ExecuteAsync] Servicio iniciando...`.
     - Confirmación de comandos (`START`, `STOP`).
   - **Tiempo**: 5 minutos.

**Resultado**: Tienes la app corriendo y ves cómo funcionan los componentes en tiempo real.

---

## Paso 4: Sigue el Flujo de Ejecución

**Objetivo**: Entender cómo un comando viaja por la app.

1. **Simula un Comando `START`**:
   - Abre la UI y haz clic en "Iniciar Servicio".
   - Usa el diagrama de secuencia ([`SEQUENCE.md`](./SEQUENCE.md)) para seguir el flujo:
     - `MainForm` → `PipeClient` → `AprendeMasPipe` → `PipeServer` → `AprendeMasService`.
     - `AprendeMasService` → `NotificationManager` → `CanalNotificaciones` → `Program` (notificador).
     - Notificador actualiza `config.json` y muestra "Escuchando...".
   - Revisa los logs para confirmar cada paso.

2. **Identifica los Archivos Involucrados**:
   - **UI**: `AprendeMas.UI/Form/MainForm.cs`, `AprendeMas.UI/Helpers/PipeClient.cs`.
   - **Servicio**: `AprendeMasWindowsService/Service/AprendeMasService.cs`, `AprendeMasWindowsService/Notifications/NotificationManager.cs`, `AprendeMasWindowsService/Communication/PipeServer.cs`, `AprendeMasWindowsService/Communication/NotifierPipeClient.cs`.
   - **Notificador**: `AprendeMasNotificationService/Program.cs`, `AprendeMasNotificationService/Config.cs`.
   - **Logs**: `AprendeMasWindowsService/Utilities/Logger.cs`.

**Resultado**: Sabes cómo un comando fluye desde la UI hasta el notificador, y qué archivos lo manejan.

---

## Paso 5: Explora el Código Clave

**Objetivo**: Sumérgete en los archivos más importantes.

Revisa estos archivos en orden, usando Visual Studio o un editor (como VS Code):

1. **`MainForm.cs` (AprendeMas.UI/Form)**:
   - **Qué hace**: Interfaz gráfica con botones "Iniciar" y "Detener".
   - **Métodos clave**:
     - `btnStart_Click`: Envía `START` via `PipeClient`.
     - `btnStop_Click`: Envía `STOP`.
   - **Por dónde empezar**: Busca `pipeClient.SendCommandAsync("START")`.

2. **`PipeClient.cs` (AprendeMas.UI/Helpers)**:
   - **Qué hace**: Envía comandos al servicio via `AprendeMasPipe`.
   - **Método clave**: `SendCommandAsync`.
   - **Por dónde empezar**: Lee cómo se conecta al pipe (`NamedPipeClientStream`).

3. **`AprendeMasService.cs` (AprendeMasWindowsService/Service)**:
   - **Qué hace**: Servicio de Windows que procesa comandos.
   - **Métodos clave**:
     - `ExecuteAsync`: Bucle principal.
     - `HandleCommand`: Procesa `START`/`STOP`.
   - **Por dónde empezar**: Busca la inicialización de `pipeServer` y `notificationManager`.

4. **`NotificationManager.cs` (AprendeMasWindowsService/Notifications)**:
   - **Qué hace**: Gestiona notificaciones y las envía al notificador.
   - **Métodos clave**:
     - `StartAsync`, `StopAsync`.
     - `SendNotificationAsync`.
   - **Por dónde empezar**: Lee cómo usa `NotifierPipeClient`.

5. **`PipeServer.cs` (AprendeMasWindowsService/Communication)**:
   - **Qué hace**: Escucha comandos en `AprendeMasPipe`.
   - **Método clave**: `Start`.
   - **Por dónde empezar**: Busca el bucle que lee comandos (`NamedPipeServerStream`).

6. **`NotifierPipeClient.cs` (AprendeMasWindowsService/Communication)**:
   - **Qué hace**: Envía notificaciones al notificador via `CanalNotificaciones`.
   - **Método clave**: `SendMessageAsync`.
   - **Por dónde empezar**: Lee la conexión al pipe.

7. **`Program.cs` (AprendeMasNotificationService)**:
   - **Qué hace**: Gestiona el ícono en la bandeja y procesa notificaciones.
   - **Métodos clave**:
     - `Main`: Inicializa el notificador.
     - `ListenForMessagesAsync`: Escucha `CanalNotificaciones`.
   - **Por dónde empezar**: Busca `notifyIcon` y `HandleMessage`.

8. **`Config.cs` (AprendeMasNotificationService)**:
   - **Qué hace**: Guarda el estado (`IsListening`) en `config.json`.
   - **Por dónde empezar**: Lee las propiedades serializables.

9. **`Logger.cs` (AprendeMasWindowsService/Utilities)**:
   - **Qué hace**: Registra eventos en archivos `.log`.
   - **Métodos clave**:
     - `Info`, `Warning`, `Error`.
   - **Por dónde empezar**: Busca el método `Log` y el `lock` para thread-safety.

**Consejo**: Usa los comentarios XML (`///`) en el código para entender cada método. Abre el proyecto en Visual Studio (`AprendeMas.sln`) y usa "Go to Definition" (F12) para navegar entre clases.

**Tiempo**: 1-2 horas, dependiendo de tu experiencia.

**Resultado**: Conoces las clases principales y su propósito.

---

## Paso 6: Depura para Profundizar

**Objetivo**: Ver el código en acción y entender su comportamiento.

1. **Configura Visual Studio**:
   - Abre `AprendeMas.sln`.
   - Establece `AprendeMas.UI` como proyecto de inicio para probar la UI.
   - Para depurar el servicio, usa `AprendeMasWindowsService` con `dotnet run`.

2. **Añade Puntos de Interrupción**:
   - En `MainForm.cs`: `btnStart_Click`.
   - En `AprendeMasService.cs`: `HandleCommand`.
   - En `Program.cs` (notificador): `HandleMessage`.
   - En `Logger.cs`: Método `Log`.

3. **Ejecuta en Modo Debug**:
   - Presiona F5 en Visual Studio.
   - Haz clic en "Iniciar Servicio" en la UI.
   - Sigue los puntos de interrupción para ver cómo el comando `START` viaja por el sistema.

4. **Revisa los Logs en Tiempo Real**:
   - Abre los archivos `.log` en un editor que se actualice (como VS Code con la extensión "Live Server").
   - O usa:
     ```powershell
     Get-Content "C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService\Logs\Service.log" -Wait
     ```
   - Compara los logs con el flujo en el diagrama de secuencia.

**Tiempo**: 30-60 minutos.

**Resultado**: Entiendes cómo el código ejecuta cada paso y dónde buscar si algo falla.

---

## Próximos Pasos

1. **Contribuye**:
   - Lee la sección "Contribución" en `README.md`.
   - Crea una rama, haz cambios, y abre un Pull Request:
     ```bash
     git checkout -b feature/nueva-funcion
     git commit -m "Añadir nueva función"
     git push origin feature/nueva-funcion
     ```

2. **Explora Más**:
   - Revisa [`DEVELOPMENT.md`](./DEVELOPMENT.md) para detalles sobre compilación y pruebas.
   - Experimenta con el código:
     - Añade un nuevo comando en `MainForm.cs`.
     - Modifica el formato de los logs en `Logger.cs`.
     - Personaliza las notificaciones en `Program.cs`.

3. **Pregunta**:
   - Si tienes dudas, abre un [issue](https://github.com/Mariano2025/aprende-mas/issues) en el repositorio.
   - Describe qué no entiendes y en qué archivo estás trabajando.

---

## Resumen

1. Lee `README.md` para el contexto general.
2. Usa `ARCHITECTURE.md`, `CLASS_DIAGRAM.md`, y `SEQUENCE.md` para entender la estructura y el flujo.
3. Configura el entorno y ejecuta la app para verla en acción.
4. Sigue el flujo de un comando `START` en los logs y diagramas.
5. Explora los archivos clave (`MainForm.cs`, `AprendeMasService.cs`, etc.).
6. Depura con Visual Studio para ver el código en tiempo real.

**Tiempo Total**: 2-4 horas, dependiendo de tu experiencia.

¡Listo, crack! 😎 Con esta guía, estarás navegando el código como pro en nada. Sigue los pasos, revisa los archivos, y si necesitas ayuda, abre un issue. ¡A meterle flow! 🚀