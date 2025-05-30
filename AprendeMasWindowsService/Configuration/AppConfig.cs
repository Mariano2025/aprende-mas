using System;
using System.Collections.Generic;
using System.IO;
using AprendeMasWindowsService.Utilities; // Asumiendo que Logger está en este namespace
using Newtonsoft.Json;

namespace AprendeMasWindowsService.Configuration
{
    /// <summary>
    /// Clase singleton para manejar la configuración global del proyecto AprendeMasV2.
    /// Carga configuraciones desde appsettings.json o variables de entorno y registra eventos mediante Logger.
    /// </summary>
    public class ApiConfig
    {
        private static readonly Lazy<ApiConfig> _instance = new Lazy<ApiConfig>(() => new ApiConfig());

        /// <summary>
        /// Obtiene la instancia única de ApiConfig.
        /// </summary>
        public static ApiConfig Instance => _instance.Value;

        private static Logger _logger;

        // Propiedades de Google Speech
        /// <summary>
        /// Clave de la API de Google Speech utilizada para autenticación.
        /// </summary>
        public string GoogleSpeechApiKey { get; private set; }

        /// <summary>
        /// URL base de la API de Google Speech para realizar peticiones.
        /// </summary>
        public string GoogleSpeechBaseUrl { get; private set; }

        /// <summary>
        /// Tiempo de espera en segundos para las peticiones a la API de Google Speech.
        /// </summary>
        public int GoogleSpeechTimeoutSeconds { get; private set; }

        // Propiedades de WebSocket
        /// <summary>
        /// Puerto utilizado por el servidor WebSocket.
        /// </summary>
        public ushort WebSocketPort { get; private set; }

        /// <summary>
        /// Número máximo de conexiones permitidas en el servidor WebSocket.
        /// </summary>
        public int WebSocketMaxConnections { get; private set; }

        /// <summary>
        /// Tiempo de espera en segundos para las operaciones de WebSocket.
        /// </summary>
        public int WebSocketTimeoutSeconds { get; private set; }

        // Propiedades de mDNS
        /// <summary>
        /// Nombre del servicio publicado mediante mDNS.
        /// </summary>
        public string MdnsServiceName { get; private set; }

        /// <summary>
        /// Tipo de servicio para mDNS (por ejemplo, "_http._tcp").
        /// </summary>
        public string MdnsServiceType { get; private set; }

        /// <summary>
        /// Nombre de usuario asociado al servicio mDNS.
        /// </summary>
        public string MdnsUsername { get; private set; }

        /// <summary>
        /// Puerto utilizado para el servicio mDNS.
        /// </summary>
        public ushort MdnsPort { get; private set; }

        /// <summary>
        /// Tiempo de vida (TTL) en segundos para los registros mDNS.
        /// </summary>
        public int MdnsTTL { get; private set; }

        /// <summary>
        /// Intervalo en segundos para anunciar el servicio mDNS.
        /// </summary>
        public int MdnsAdvertiseIntervalSeconds { get; private set; }

        // Propiedades de red
        /// <summary>
        /// Dirección IP local del servidor.
        /// </summary>
        public string NetworkLocalIp { get; private set; }

        /// <summary>
        /// Lista de rangos de IPs permitidos para conexiones.
        /// </summary>
        public List<string> NetworkAllowedIps { get; private set; }

        /// <summary>
        /// Número de intentos de reintento en caso de fallo de conexión.
        /// </summary>
        public int NetworkRetryAttempts { get; private set; }

        /// <summary>
        /// Retraso en segundos entre intentos de reintento.
        /// </summary>
        public int NetworkRetryDelaySeconds { get; private set; }

        /// <summary>
        /// Constructor privado que inicializa la configuración al crear la instancia.
        /// </summary>
        private ApiConfig()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Establece la instancia de Logger para registrar eventos y errores en la clase.
        /// </summary>
        /// <param name="logger">Instancia de Logger para logging centralizado.</param>
        public static void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Carga las configuraciones desde el archivo appsettings.json o variables de entorno.
        /// Registra errores o éxitos mediante el Logger.
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                // Ruta del archivo de configuración
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

                if (File.Exists(configPath))
                {
                    // Carga desde appsettings.json
                    var json = File.ReadAllText(configPath);
                    dynamic config = JsonConvert.DeserializeObject(json);

                    // Cargar GoogleSpeech
                    GoogleSpeechApiKey = config.GoogleSpeech?.ApiKey;
                    GoogleSpeechBaseUrl = config.GoogleSpeech?.BaseUrl;
                    GoogleSpeechTimeoutSeconds = config.GoogleSpeech?.TimeoutSeconds ?? 30;

                    // Cargar WebSocket
                    WebSocketPort = config.WebSocket?.Port ?? 5000;
                    WebSocketMaxConnections = config.WebSocket?.MaxConnections ?? 100;
                    WebSocketTimeoutSeconds = config.WebSocket?.TimeoutSeconds ?? 60;

                    // Cargar Mdns
                    MdnsServiceName = config.Mdns?.ServiceName ?? "AprendeMasWebSocket";
                    MdnsServiceType = config.Mdns?.ServiceType ?? "_http._tcp";
                    MdnsUsername = config.Mdns?.Username;
                    MdnsPort = config.Mdns?.Port ?? 5353;
                    MdnsTTL = config.Mdns?.TTL ?? 120;
                    MdnsAdvertiseIntervalSeconds = config.Mdns?.AdvertiseIntervalSeconds ?? 300;

                    // Cargar Network
                    NetworkLocalIp = config.Network?.LocalIp;
                    NetworkAllowedIps = config.Network?.AllowedIps?.ToObject<List<string>>() ?? new List<string>();
                    NetworkRetryAttempts = config.Network?.RetryAttempts ?? 3;
                    NetworkRetryDelaySeconds = config.Network?.RetryDelaySeconds ?? 5;
                }
                else
                {
                    // Carga desde variables de entorno si no hay appsettings.json
                    GoogleSpeechApiKey = Environment.GetEnvironmentVariable("GOOGLE_SPEECH_API_KEY");
                    GoogleSpeechBaseUrl = Environment.GetEnvironmentVariable("GOOGLE_SPEECH_BASE_URL");
                    GoogleSpeechTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("GOOGLE_SPEECH_TIMEOUT_SECONDS"), out int timeout) ? timeout : 30;

                    string portString = Environment.GetEnvironmentVariable("WEB_SOCKET_PORT");
                    WebSocketPort = ushort.TryParse(portString, out ushort port) ? port : (ushort)5000;
                    WebSocketMaxConnections = int.TryParse(Environment.GetEnvironmentVariable("WEB_SOCKET_MAX_CONNECTIONS"), out int maxConn) ? maxConn : 100;
                    WebSocketTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("WEB_SOCKET_TIMEOUT_SECONDS"), out int wsTimeout) ? wsTimeout : 60;

                    MdnsServiceName = Environment.GetEnvironmentVariable("MDNS_SERVICE_NAME") ?? "AprendeMasWebSocket";
                    MdnsServiceType = Environment.GetEnvironmentVariable("MDNS_SERVICE_TYPE") ?? "_http._tcp";
                    MdnsUsername = Environment.GetEnvironmentVariable("MDNS_USERNAME");
                    MdnsPort = ushort.TryParse(Environment.GetEnvironmentVariable("MDNS_PORT"), out ushort mdnsPort) ? mdnsPort : (ushort)5353;
                    MdnsTTL = int.TryParse(Environment.GetEnvironmentVariable("MDNS_TTL"), out int ttl) ? ttl : 120;
                    MdnsAdvertiseIntervalSeconds = int.TryParse(Environment.GetEnvironmentVariable("MDNS_ADVERTISE_INTERVAL_SECONDS"), out int interval) ? interval : 300;

                    NetworkLocalIp = Environment.GetEnvironmentVariable("NETWORK_LOCAL_IP");
                    string allowedIpsString = Environment.GetEnvironmentVariable("NETWORK_ALLOWED_IPS");
                    NetworkAllowedIps = !string.IsNullOrEmpty(allowedIpsString) ? allowedIpsString.Split(',').ToList() : new List<string>();
                    NetworkRetryAttempts = int.TryParse(Environment.GetEnvironmentVariable("NETWORK_RETRY_ATTEMPTS"), out int retryAttempts) ? retryAttempts : 3;
                    NetworkRetryDelaySeconds = int.TryParse(Environment.GetEnvironmentVariable("NETWORK_RETRY_DELAY_SECONDS"), out int retryDelay) ? retryDelay : 5;
                }

                // Validación de configuraciones requeridas
                if (string.IsNullOrEmpty(GoogleSpeechApiKey) || string.IsNullOrEmpty(GoogleSpeechBaseUrl) || WebSocketPort == 0)
                {
                    _logger?.Error("API Keys o configuraciones no encontradas. Verifica appsettings.json o variables de entorno.");
                }
                else
                {
                    _logger?.Info("Configuración cargada correctamente desde appsettings.json o variables de entorno.");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error al cargar la configuración: {ex.Message}", ex);
            }
        }
    }
}