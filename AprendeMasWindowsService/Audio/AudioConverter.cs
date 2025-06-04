using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AprendeMasWindowsService.Utilities; // Para Logger

namespace AprendeMasWindowsService.Audio
{
    /// <summary>
    /// Clase para convertir archivos de audio M4A a formato WAV utilizando FFmpeg.
    /// Diseñada para integrarse con el servidor WebSocket, procesando datos binarios de entrada
    /// y devolviendo el audio convertido como datos binarios.
    /// </summary>
    public class AudioConverter
    {
        private readonly Logger _logger; // Logger para registrar eventos y errores

        /// <summary>
        /// Inicializa una nueva instancia de AudioConverter con un logger.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        public AudioConverter(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Convierte un archivo de audio M4A a WAV con configuraciones específicas (PCM 16-bit, 16 kHz, mono).
        /// </summary>
        /// <param name="m4aData">Datos binarios del archivo M4A a convertir.</param>
        /// <returns>Datos binarios del archivo WAV generado.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si m4aData es null o vacío.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si falla la conversión, el archivo WAV no se genera correctamente, o los datos M4A son inválidos.</exception>
        public async Task<byte[]> ConvertM4AToWav(byte[] m4aData)
        {
            // Validar que los datos de entrada no sean null ni vacíos
            if (m4aData == null || m4aData.Length == 0)
            {
                _logger.Error("Datos de audio M4A null o vacíos.");
                throw new ArgumentNullException(nameof(m4aData), "Los datos de audio M4A no pueden ser null o vacíos.");
            }

            // Registrar información de depuración
            _logger.Info($"Recibidos {m4aData.Length} bytes de audio M4A. Primeros 8 bytes: {BitConverter.ToString(m4aData.Take(8).ToArray())}", nameof(ConvertM4AToWav));

            // Validar que los datos tengan una estructura M4A básica (átomo 'ftyp')
            if (!IsValidM4A(m4aData))
            {
                _logger.Error("Los datos recibidos no son un archivo M4A válido (falta átomo 'ftyp').");
                throw new InvalidOperationException("Los datos recibidos no son un archivo M4A válido.");
            }

            // Generar rutas temporales únicas para los archivos M4A y WAV
            string tempM4aPath = Path.Combine(Path.GetTempPath(), $"m4a_{Guid.NewGuid()}.m4a");
            string tempWavPath = Path.Combine(Path.GetTempPath(), $"wav_{Guid.NewGuid()}.wav");

            try
            {
                // Escribir los datos M4A en un archivo temporal
                await File.WriteAllBytesAsync(tempM4aPath, m4aData);
                _logger.Info($"Archivo M4A temporal creado en: {tempM4aPath}", nameof(ConvertM4AToWav));

                // Ejecutar la conversión de M4A a WAV usando FFmpeg
                await ExecuteFFmpegConversionAsync(tempM4aPath, tempWavPath);

                // Verificar que el archivo WAV se generó correctamente
                if (!File.Exists(tempWavPath) || new FileInfo(tempWavPath).Length < 44) // 44 bytes es el tamaño mínimo de un encabezado WAV
                {
                    _logger.Error($"La conversión a WAV falló: el archivo {tempWavPath} no se generó o está corrupto.");
                    throw new InvalidOperationException("La conversión a WAV falló: el archivo de salida no se generó o está corrupto.");
                }

                // Leer el archivo WAV como byte[] y devolverlo
                byte[] wavData = await File.ReadAllBytesAsync(tempWavPath);
                _logger.Info($"Archivo WAV generado con éxito: {wavData.Length} bytes.", nameof(ConvertM4AToWav));
                return wavData;
            }
            catch (Exception ex)
            {
                // Registrar el error y relanzar una excepción con contexto
                _logger.Error($"Error al convertir audio M4A a WAV: {ex.Message}", ex, nameof(ConvertM4AToWav));
                throw new InvalidOperationException($"Error al convertir audio M4A a WAV: {ex.Message}", ex);
            }
            finally
            {
                // Limpiar archivos temporales para evitar acumulación de datos
                try
                {
                    //if (File.Exists(tempM4aPath)) File.Delete(tempM4aPath);
                    //if (File.Exists(tempWavPath)) File.Delete(tempWavPath);
                    _logger.Info($"Archivos temporales eliminados: {tempM4aPath}, {tempWavPath}", nameof(ConvertM4AToWav));
                }
                catch (Exception ex)
                {
                    // Registrar error de limpieza
                    _logger.Warning($"No se pudieron eliminar los archivos temporales: {ex.Message}", nameof(ConvertM4AToWav));
                }
            }
        }

        /// <summary>
        /// Valida si los datos binarios representan un archivo M4A válido comprobando la presencia del átomo 'ftyp'.
        /// </summary>
        /// <param name="data">Datos binarios a validar.</param>
        /// <returns>True si los datos parecen ser un archivo M4A válido, false en caso contrario.</returns>
        private bool IsValidM4A(byte[] data)
        {
            // Un archivo M4A válido debe tener el átomo 'ftyp' en los primeros bytes
            // Estructura: [4 bytes de tamaño][4 bytes 'ftyp'][4 bytes de marca, por ejemplo, 'M4A ']
            if (data.Length < 12) return false; // Mínimo para contener 'ftyp'

            // Buscar 'ftyp' en la posición 4
            byte[] ftypSignature = Encoding.ASCII.GetBytes("ftyp");
            for (int i = 0; i < 4; i++)
            {
                if (data[i + 4] != ftypSignature[i]) return false;
            }

            // Verificar que los siguientes 4 bytes sean una marca válida (por ejemplo, 'M4A ')
            string brand = Encoding.ASCII.GetString(data, 8, 4);
            return brand == "M4A " || brand == "mp42" || brand == "isom";
        }

        /// <summary>
        /// Ejecuta FFmpeg para convertir un archivo M4A a WAV con parámetros optimizados.
        /// </summary>
        /// <param name="m4aFilePath">Ruta del archivo M4A de entrada.</param>
        /// <param name="wavFilePath">Ruta del archivo WAV de salida.</param>
        /// <exception cref="InvalidOperationException">Se lanza si FFmpeg no se completa con éxito.</exception>
        private async Task ExecuteFFmpegConversionAsync(string m4aFilePath, string wavFilePath)
        {
            try
            {
                // Obtener la ruta del directorio de la aplicación
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string ffmpegPath = Path.Combine(appDirectory, "ffmpeg.exe");

                // Verificar que ffmpeg.exe existe en el directorio de la aplicación
                if (!File.Exists(ffmpegPath))
                {
                    _logger.Error($"No se encontró ffmpeg.exe en {ffmpegPath}.");
                    throw new InvalidOperationException($"No se encontró ffmpeg.exe en {ffmpegPath}. Asegúrese de que esté incluido en el directorio de la aplicación.");
                }

                // Configurar el proceso de FFmpeg
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath, // Usar la ruta completa de ffmpeg.exe
                        Arguments = $"-i \"{m4aFilePath}\" -acodec pcm_s16le -ar 16000 -ac 1 \"{wavFilePath}\"", // PCM 16-bit, 16 kHz, mono
                        UseShellExecute = false, // Ejecutar sin shell para mayor control
                        RedirectStandardOutput = true, // Capturar salida estándar
                        RedirectStandardError = true, // Capturar errores
                        CreateNoWindow = true // Evitar ventana de consola visible
                    }
                };

                // Iniciar el proceso de FFmpeg
                _logger.Info($"Ejecutando FFmpeg: {ffmpegPath} {process.StartInfo.Arguments}", nameof(ExecuteFFmpegConversionAsync));
                process.Start();

                // Capturar errores de FFmpeg de forma asíncrona
                string errorOutput = await process.StandardError.ReadToEndAsync();

                // Esperar a que el proceso termine
                await Task.Run(() => process.WaitForExit());

                // Verificar el código de salida del proceso
                if (process.ExitCode != 0)
                {
                    _logger.Error($"FFmpeg falló con código {process.ExitCode}: {errorOutput}");
                    throw new InvalidOperationException($"FFmpeg falló con código {process.ExitCode}: {errorOutput}");
                }
                _logger.Info("FFmpeg completó la conversión con éxito.", nameof(ExecuteFFmpegConversionAsync));
            }
            catch (Exception ex)
            {
                // Relanzar excepción con contexto adicional
                _logger.Error($"Error al ejecutar FFmpeg: {ex.Message}", ex, nameof(ExecuteFFmpegConversionAsync));
                throw new InvalidOperationException($"Error al ejecutar FFmpeg para la conversión: {ex.Message}", ex);
            }
        }
    }
}