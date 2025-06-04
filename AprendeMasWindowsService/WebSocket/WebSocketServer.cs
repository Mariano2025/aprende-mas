using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AprendeMasWindowsService.Audio;
using AprendeMasWindowsService.Configuration;
using AprendeMasWindowsService.Notifications;
using AprendeMasWindowsService.Utilities;

namespace AprendeMasWindowsService.WebSocket
{
    /// <summary>
    /// Servidor WebSocket de alto rendimiento para gestionar conexiones de dispositivos móviles,
    /// mensajería por canales, y transferencia de archivos binarios (audio M4A a WAV), con manejo robusto de errores
    /// y notificaciones integradas.
    /// </summary>
    public class WebSocketServer
    {
        private readonly HttpListener _httpListener;              // Escucha conexiones HTTP/WebSocket entrantes
        private readonly NotificationManager _notificationManager; // Gestiona notificaciones de eventos
        private readonly Logger _logger;                         // Registra actividades y errores
        private readonly int _port;                              // Puerto en el que escucha el servidor
        private readonly ConcurrentDictionary<string, ClientConnection> _clients; // Lista thread-safe de clientes conectados
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _channels; // Mapea canales a IDs de clientes
        private readonly AudioConverter _audioConverter;         // Convierte audio M4A a WAV
        private CancellationTokenSource _cts;                    // Controla la cancelación del servidor
        private bool _isRunning;                                 // Indica si el servidor está activo

        /// <summary>
        /// Inicializa una nueva instancia del servidor WebSocket con la configuración especificada.
        /// </summary>
        /// <param name="port">Puerto en el que escuchará el servidor.</param>
        /// <param name="notificationManager">Gestor de notificaciones para eventos.</param>
        /// <param name="logger">Servicio de logging para registrar eventos.</param>
        /// <param name="audioConverter">Convertidor de audio para procesar M4A a WAV.</param>
        public WebSocketServer(int port, NotificationManager notificationManager, Logger logger, AudioConverter audioConverter)
        {
            _port = port;
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _audioConverter = audioConverter ?? throw new ArgumentNullException(nameof(audioConverter));
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://*:{_port}/ws/"); // Escucha en cualquier dirección
            _clients = new ConcurrentDictionary<string, ClientConnection>();
            _channels = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            _isRunning = false;
        }

        /// <summary>
        /// Inicia el servidor WebSocket de forma asíncrona, comenzando a aceptar conexiones.
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        /// <exception cref="InvalidOperationException">Se lanza si el servidor ya está en ejecución.</exception>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("El servidor WebSocket ya está en ejecución.");
            }

            try
            {
                _cts = new CancellationTokenSource();
                _httpListener.Start();
                _isRunning = true;
                _logger.Info($"Servidor WebSocket iniciado en el puerto {_port}.", nameof(StartAsync));
                await _notificationManager.SendNotificationAsync("Servidor WebSocket iniciado.");
                // Inicia el bucle de aceptación de conexiones en una tarea en segundo plano
                _ = Task.Run(() => AcceptConnectionsAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al iniciar el servidor WebSocket.", ex, nameof(StartAsync));
                await _notificationManager.SendNotificationAsync("Error al iniciar el servidor WebSocket.");
                throw;
            }
        }

        /// <summary>
        /// Detiene el servidor WebSocket de forma asíncrona, cerrando todas las conexiones de clientes.
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _cts.Cancel();
                _httpListener.Stop();
                _isRunning = false;

                // Cierra todas las conexiones de clientes activas
                var closeTasks = _clients.Values.Select(client => client.CloseAsync());
                await Task.WhenAll(closeTasks);
                _clients.Clear();
                _channels.Clear();

                _logger.Info("Servidor WebSocket detenido.", nameof(StopAsync));
                await _notificationManager.SendNotificationAsync("Servidor WebSocket detenido.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error al detener el servidor WebSocket.", ex, nameof(StopAsync));
                await _notificationManager.SendNotificationAsync("Error al detener el servidor WebSocket.");
            }
        }

        /// <summary>
        /// Acepta conexiones WebSocket entrantes de dispositivos móviles hasta que se cancele.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        var clientId = Guid.NewGuid().ToString();
                        var client = new ClientConnection(clientId, webSocketContext.WebSocket);
                        _clients.TryAdd(clientId, client);

                        _logger.Info($"Cliente {clientId} conectado.", nameof(AcceptConnectionsAsync));
                        await _notificationManager.SendNotificationAsync($"Dispositivo móvil {clientId} conectado.");
                        // Inicia el manejo del cliente en una tarea separada
                        _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400; // Solicitud inválida
                        context.Response.Close();
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is HttpListenerException)
                {
                    // Terminación normal al cancelar o detener el listener
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error al aceptar conexión WebSocket.", ex, nameof(AcceptConnectionsAsync));
                }
            }
        }

        /// <summary>
        /// Maneja la comunicación con un cliente conectado, procesando mensajes y desconexiones.
        /// </summary>
        /// <param name="client">Conexión del cliente.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        private async Task HandleClientAsync(ClientConnection client, CancellationToken cancellationToken)
        {
            try
            {
                var buffer = new byte[1024 * 16]; // Buffer de 16KB para mensajes entrantes
                while (client.WebSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessTextMessageAsync(client.ClientId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Procesar mensaje binario con protocolo de tamaño + datos
                        await ProcessBinaryMessageAsync(client.ClientId, buffer, result, client.WebSocket, cancellationToken);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleClientDisconnectAsync(client.ClientId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al manejar cliente {client.ClientId}.", ex, nameof(HandleClientAsync));
                await HandleClientDisconnectAsync(client.ClientId);
            }
        }

        /// <summary>
        /// Procesa mensajes de texto recibidos de un cliente, manejando suscripciones y envíos a canales.
        /// </summary>
        /// <param name="clientId">Identificador único del cliente.</param>
        /// <param name="message">Mensaje recibido.</param>
        private async Task ProcessTextMessageAsync(string clientId, string message)
        {
            try
            {
                if (message.StartsWith("subscribe:"))
                {
                    var channel = message.Substring("subscribe:".Length).Trim();
                    if (!string.IsNullOrEmpty(channel))
                    {
                        var channelClients = _channels.GetOrAdd(channel, _ => new ConcurrentBag<string>());
                        channelClients.Add(clientId);
                        _logger.Info($"Cliente {clientId} suscrito al canal {channel}.", nameof(ProcessTextMessageAsync));
                        await _notificationManager.SendNotificationAsync($"Cliente {clientId} suscrito al canal {channel}.");
                    }
                }
                else if (message.StartsWith("unsubscribe:"))
                {
                    var channel = message.Substring("unsubscribe:".Length).Trim();
                    UnsubscribeFromChannel(clientId, channel);
                }
                else if (message.StartsWith("sendtochannel:"))
                {
                    var parts = message.Substring("sendtochannel:".Length).Split(new[] { ':' }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var channel = parts[0].Trim();
                        var msg = parts[1].Trim();
                        if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(msg))
                        {
                            await SendMessageToChannelAsync(channel, msg);
                            _logger.Info($"Cliente {clientId} envió mensaje al canal {channel}: {msg}", nameof(ProcessTextMessageAsync));
                            await _notificationManager.SendNotificationAsync($"Cliente {clientId} envió mensaje al canal {channel}: {msg}");
                        }
                    }
                }
                else
                {
                    _logger.Warning($"Mensaje no reconocido recibido de cliente {clientId}: {message}", nameof(ProcessTextMessageAsync));
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar mensaje de texto de cliente {clientId}.", ex, nameof(ProcessTextMessageAsync));
            }
        }

        /// <summary>
        /// Procesa mensajes binarios (por ejemplo, archivos de audio M4A) recibidos de un cliente, utilizando protocolo de tamaño + datos.
        /// </summary>
        /// <param name="clientId">Identificador único del cliente.</param>
        /// <param name="buffer">Buffer inicial con datos recibidos.</param>
        /// <param name="initialResult">Resultado inicial de recepción.</param>
        /// <param name="webSocket">WebSocket del cliente.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        private async Task ProcessBinaryMessageAsync(string clientId, byte[] buffer, WebSocketReceiveResult initialResult, System.Net.WebSockets.WebSocket webSocket, CancellationToken cancellationToken)
        {
            try
            {
                // Leer los primeros 4 bytes para obtener el tamaño del archivo (big-endian)
                if (initialResult.Count < 4)
                {
                    _logger.Error($"Mensaje binario inicial demasiado corto para cliente {clientId}.");
                    await _notificationManager.SendNotificationAsync($"Mensaje binario inválido de cliente {clientId}.");
                    return;
                }

                int fileSize = BitConverter.ToInt32(buffer.Take(4).Reverse().ToArray(), 0); // Convertir de big-endian a int
                _logger.Info($"Recibiendo archivo de {fileSize} bytes del cliente {clientId}.", nameof(ProcessBinaryMessageAsync));

                // Buffer para almacenar el archivo completo
                using var memoryStream = new MemoryStream();

                // Si el mensaje inicial contiene datos después del tamaño, agregarlos
                if (initialResult.Count > 4)
                {
                    await memoryStream.WriteAsync(buffer, 4, initialResult.Count - 4, cancellationToken);
                }

                // Continuar recibiendo fragmentos hasta completar el archivo
                int bytesReceived = initialResult.Count - 4;
                while (bytesReceived < fileSize && webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType != WebSocketMessageType.Binary)
                    {
                        _logger.Error($"Mensaje no binario recibido mientras se esperaba archivo de cliente {clientId}.");
                        await _notificationManager.SendNotificationAsync($"Formato de mensaje inválido de cliente {clientId}.");
                        return;
                    }

                    await memoryStream.WriteAsync(buffer, 0, result.Count, cancellationToken);
                    bytesReceived += result.Count;
                    _logger.Info($"Recibidos {bytesReceived}/{fileSize} bytes del cliente {clientId}.", nameof(ProcessBinaryMessageAsync));
                }

                if (bytesReceived != fileSize)
                {
                    _logger.Error($"Tamaño de archivo recibido ({bytesReceived} bytes) no coincide con el esperado ({fileSize} bytes) para cliente {clientId}.");
                    await _notificationManager.SendNotificationAsync($"Error en la recepción de archivo de cliente {clientId}.");
                    return;
                }

                // Convertir M4A a WAV usando AudioConverter
                byte[] fileData = memoryStream.ToArray();
                byte[] convertedAudio = await _audioConverter.ConvertM4AToWav(fileData);
                if (convertedAudio == null || convertedAudio.Length == 0)
                {
                    _logger.Error($"La conversión de audio falló para el cliente {clientId}.");
                    await _notificationManager.SendNotificationAsync($"La conversión de audio falló para el cliente {clientId}.");
                    return;
                }

                // Enviar el audio convertido (WAV) al canal "audio"
                await SendBinaryToChannelAsync("audio", convertedAudio);
                _logger.Info($"Cliente {clientId} envió audio M4A, convertido a WAV y transmitido al canal 'audio'.", nameof(ProcessBinaryMessageAsync));
                await _notificationManager.SendNotificationAsync($"Cliente {clientId} envió archivo de audio, convertido a WAV y transmitido al canal 'audio'.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar audio binario de cliente {clientId}.", ex, nameof(ProcessBinaryMessageAsync));
                await _notificationManager.SendNotificationAsync($"Error al procesar audio de cliente {clientId}.");
            }
        }

        /// <summary>
        /// Suscribe a un cliente a un canal específico.
        /// </summary>
        /// <param name="clientId">Identificador único del cliente.</param>
        /// <param name="channel">Nombre del canal al que suscribirse.</param>
        internal void SubscribeToChannel(string clientId, string channel)
        {
            var channelClients = _channels.GetOrAdd(channel, _ => new ConcurrentBag<string>());
            channelClients.Add(clientId);
            _logger.Info($"Cliente {clientId} suscrito al canal {channel}.", nameof(SubscribeToChannel));
        }

        /// <summary>
        /// Desuscribe a un cliente de un canal específico.
        /// </summary>
        /// <param name="clientId">Identificador único del cliente.</param>
        /// <param name="channel">Nombre del canal del que desuscribirse.</param>
        internal void UnsubscribeFromChannel(string clientId, string channel)
        {
            if (_channels.TryGetValue(channel, out var channelClients))
            {
                channelClients = new ConcurrentBag<string>(channelClients.Except(new[] { clientId }));
                if (channelClients.IsEmpty)
                {
                    _channels.TryRemove(channel, out _);
                }
                else
                {
                    _channels[channel] = channelClients;
                }
                _logger.Info($"Cliente {clientId} desuscrito del canal {channel}.", nameof(UnsubscribeFromChannel));
                _notificationManager.SendNotificationAsync($"Cliente {clientId} desuscrito del canal {channel}.");
            }
        }

        /// <summary>
        /// Envía un mensaje de texto a todos los clientes suscritos a un canal específico.
        /// </summary>
        private async Task SendMessageToChannelAsync(string channel, string message)
        {
            if (_channels.TryGetValue(channel, out var channelClients))
            {
                var tasks = channelClients.Select(clientId =>
                {
                    if (_clients.TryGetValue(clientId, out var client))
                    {
                        return client.SendMessageAsync(message);
                    }
                    return Task.CompletedTask;
                });
                await Task.WhenAll(tasks);
                _logger.Info($"Mensaje enviado al canal {channel}: {message}", nameof(SendMessageToChannelAsync));
            }
        }

        /// <summary>
        /// Envía datos binarios (por ejemplo, archivos de audio WAV) a todos los clientes suscritos a un canal.
        /// </summary>
        /// <param name="channel">Nombre del canal objetivo.</param>
        /// <param name="data">Datos binarios a enviar.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        internal async Task SendBinaryToChannelAsync(string channel, byte[] data)
        {
            if (_channels.TryGetValue(channel, out var channelClients))
            {
                var tasks = channelClients.Select(clientId =>
                {
                    if (_clients.TryGetValue(clientId, out var client))
                    {
                        return client.SendBinaryAsync(data);
                    }
                    return Task.CompletedTask;
                });
                await Task.WhenAll(tasks);
                _logger.Info($"Datos binarios enviados al canal {channel}.", nameof(SendBinaryToChannelAsync));
            }
        }

        /// <summary>
        /// Maneja la desconexión de un cliente, eliminándolo de todos los canales y notificando al sistema.
        /// </summary>
        /// <param name="clientId">Identificador único del cliente.</param>
        internal async Task HandleClientDisconnectAsync(string clientId)
        {
            if (_clients.TryRemove(clientId, out var client))
            {
                await client.CloseAsync();
                foreach (var channel in _channels.Keys.ToList())
                {
                    UnsubscribeFromChannel(clientId, channel);
                }
                _logger.Info($"Cliente {clientId} desconectado.", nameof(HandleClientDisconnectAsync));
                await _notificationManager.SendNotificationAsync($"Dispositivo móvil {clientId} desconectado.");
            }
        }
    }

    /// <summary>
    /// Representa una conexión activa de un cliente WebSocket.
    /// </summary>
    internal class ClientConnection
    {
        public string ClientId { get; }                  // Identificador único del cliente
        public System.Net.WebSockets.WebSocket WebSocket { get; } // Conexión WebSocket del cliente

        public ClientConnection(string clientId, System.Net.WebSockets.WebSocket webSocket)
        {
            ClientId = clientId;
            WebSocket = webSocket;
        }

        /// <summary>
        /// Envía un mensaje de texto al cliente.
        /// </summary>
        /// <param name="message">Mensaje a enviar.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        public async Task SendMessageAsync(string message)
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Envía datos binarios al cliente.
        /// </summary>
        /// <param name="data">Datos binarios a enviar.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        public async Task SendBinaryAsync(byte[] data)
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                await WebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Cierra la conexión WebSocket de forma limpia.
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        public async Task CloseAsync()
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexión cerrada", CancellationToken.None);
            }
        }
    }
}