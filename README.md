# AprendeMas

Aplicación educativa que combina un servicio de Windows (`AprendeMasWindowsService`) y un notificador (`AprendeMasNotificationService`) para enviar notificaciones personalizadas a los usuarios.

## Características
- Servicio de fondo que ejecuta tareas programadas.
- Notificador que muestra mensajes al usuario mediante pipes nombrados.
- Instalador profesional creado con Inno Setup.

## Instalación
1. Descarga `AprendeMasSetup.exe` desde los releases.
2. Ejecútalo como administrador para instalar el servicio y el notificador.
3. El servicio se inicia automáticamente, y el notificador muestra notificaciones.

## Requisitos
- Windows 10 o superior.
- .NET 8.0 Runtime.

## Desarrollo
- Clona el repositorio: `git clone https://github.com/Mariano2025/aprende-mas.git`
- Abre `AprendeMas.sln` en Visual Studio.
- Compila en modo Release (`dotnet build -c Release`).
- Usa Inno Setup para generar el instalador.

## Licencia
MIT License (o la licencia que prefieras). 
