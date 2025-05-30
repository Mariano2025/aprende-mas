using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AprendeMasWindowsService.Configuration;
using Makaretu.Dns; // Biblioteca para mDNS

namespace AprendeMasWindowsService.mDNS
{
    /// <summary>
    /// Servicio que publica el servidor WebSocket mediante mDNS para permitir su descubrimiento automático en la red local.
    /// </summary>
    public class MdnsService
    {
        private readonly string _serviceName; // Nombre del servicio para mDNS
        private readonly ushort _port; // Puerto en el que corre el servicio mdns
        private readonly string _userName; // Usuario que se mostrará en el servicio mdns
        private bool _isRunning; // Indica si el servicio mDNS está activo
        private MulticastService _mdns; // Servicio mDNS para publicar el anuncio
        private ServiceDiscovery _serviceDiscovery; // Objeto para anunciar el servicio

        /// <summary>
        /// Constructor del servicio mDNS.
        /// </summary>
        public MdnsService()
        {
            _serviceName = ApiConfig.Instance.MdnsServiceName ?? throw new ArgumentNullException(nameof(ApiConfig.Instance.MdnsServiceName), "El nombre del servicio no puede ser null.");
            _port = ApiConfig.Instance.MdnsPort;
            _userName = ApiConfig.Instance.MdnsUsername;
            _isRunning = false;
        }

        /// <summary>
        /// Inicia la publicación del servicio WebSocket mediante mDNS.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="InvalidOperationException">Se lanza si el servicio ya está en ejecución o si falla la publicación.</exception>
        public Task StartAsync()
        {
            if (_isRunning) throw new InvalidOperationException("El servicio mDNS ya está en ejecución.");

            try
            {
                // Obtiene la dirección IP local de la máquina
                var localIp = GetLocalIpAddress();
                if (string.IsNullOrEmpty(localIp))
                    throw new InvalidOperationException("No se pudo determinar la dirección IP local.");

                // Inicializa el servicio mDNS
                _mdns = new MulticastService();
                _serviceDiscovery = new ServiceDiscovery(_mdns);

                // Configura el perfil del servicio
                var service = new ServiceProfile(
                    instanceName: $"{_serviceName}",
                    serviceName: ApiConfig.Instance.MdnsServiceType, // Tipo de servicio estándar para Mdns sobre HTTP
                    port: _port, // Ahora es ushort, compatible con ServiceProfile
                    addresses: new[] { IPAddress.Parse(localIp) }
                );

                // Añade el username como TXT si está disponible
                if (!string.IsNullOrEmpty(_userName))
                {
                    var txtRecord = new TXTRecord
                    {
                        Name = $"{_serviceName}._http._tcp.local.",
                        Strings = new System.Collections.Generic.List<string> { $"username={_userName}" },
                        TTL = TimeSpan.FromSeconds(120)
                    };
                    service.Resources.Add(txtRecord);
                }


                // Anuncia el servicio
                _serviceDiscovery.Advertise(service);
                _mdns.Start(); // Inicia el servicio mDNS para enviar paquetes
                _isRunning = true;

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al publicar el servicio mDNS: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detiene la publicación del servicio mDNS.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public Task StopAsync()
        {
            if (!_isRunning) return Task.CompletedTask;

            try
            {
                // Detiene la publicación del servicio
                if (_mdns != null)
                {
                    _mdns.Stop(); // Detiene el envío de paquetes mDNS
                }
                if (_serviceDiscovery != null)
                {
                    _serviceDiscovery.Dispose(); // Libera los recursos del descubrimiento
                }
            }
            finally
            {
                _isRunning = false;
                _mdns = null;
                _serviceDiscovery = null;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Obtiene la dirección IP local de la máquina.
        /// </summary>
        /// <returns>La dirección IP local como cadena, o null si no se encuentra.</returns>
        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
                {
                    return ip.ToString();
                }
            }
            return null;
        }
    }
}