using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ApiSwagger.Data;
using ApiSwagger.Models;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

namespace ApiSwagger.Services
{
    public class CsvKilometrajeService
    {
        private readonly AppDbContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _procesadosPrefix;

        public CsvKilometrajeService(AppDbContext context, IAmazonS3 s3Client, string bucketName)
        {
            _context = context;
            _s3Client = s3Client;
            _bucketName = bucketName;
            _procesadosPrefix = "procesados/";
        }

        public void ProcesarArchivosCsv()
        {
            var archivos = ListarArchivosCsvAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Archivos encontrados: {archivos.Count}");
            foreach (var archivoKey in archivos)
            {
                Console.WriteLine($"Procesando archivo: {archivoKey}");
                using var stream = DescargarArchivoAsync(archivoKey).GetAwaiter().GetResult();
                ProcesarArchivo(stream);
                MoverArchivoProcesadoAsync(archivoKey).GetAwaiter().GetResult();
                Console.WriteLine($"Movido a: {_procesadosPrefix}{Path.GetFileName(archivoKey)}");
            }
        }

        private void ProcesarArchivo(Stream archivoStream)
        {
            using var reader = new StreamReader(archivoStream, Encoding.UTF8, true, 1024, leaveOpen: false);
            string? header = reader.ReadLine();
            if (header == null)
            {
                Console.WriteLine("Archivo vacío o sin encabezado.");
                return;
            }
            var columnas = header.Split(';');
            int idxInterno = Array.FindIndex(columnas, c => c.Trim().ToUpper() == "INTERNO");
            int idxKm = Array.FindIndex(columnas, c => c.Trim().ToUpper() == "KM RECORRIDOS");
            if (idxInterno == -1 || idxKm == -1)
            {
                Console.WriteLine($"No se encontraron columnas INTERNO o KM RECORRIDOS en el archivo. Encabezado detectado: {header}");
                return;
            }

            int filasProcesadas = 0;
            int colectivosActualizados = 0;
            while (!reader.EndOfStream)
            {
                var linea = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(linea)) continue;
                var datos = linea.Split(';');
                if (datos.Length <= Math.Max(idxInterno, idxKm)) continue;
                var interno = datos[idxInterno].Trim();
                var kmStr = datos[idxKm].Trim().Replace(",", "."); // Asegura punto decimal
                Console.WriteLine($"Fila: INTERNO={interno}, KM RECORRIDOS={kmStr}");
                if (!decimal.TryParse(kmStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal kmRecorridos))
                {
                    Console.WriteLine($"No se pudo convertir KM RECORRIDOS: {kmStr}");
                    continue;
                }
                var colectivo = _context.Colectivos.FirstOrDefault(c => c.NroColectivo.Trim() == interno);
                if (colectivo != null)
                {
                    decimal kmAntes = colectivo.Kilometraje ?? 0m;
                    decimal kmSumar = kmRecorridos;
                    colectivo.Kilometraje = kmAntes + kmSumar;
                    colectivosActualizados++;
                    Console.WriteLine($"Actualizado colectivo {interno}: {kmAntes:F2} + {kmSumar:F2} = {colectivo.Kilometraje:F2}");
                }
                else
                {
                    Console.WriteLine($"No se encontró colectivo con INTERNO={interno}");
                }
                filasProcesadas++;
            }
            _context.SaveChanges();
            Console.WriteLine($"Filas procesadas: {filasProcesadas}, Colectivos actualizados: {colectivosActualizados}");
        }

        private async Task<List<string>> ListarArchivosCsvAsync()
        {
            var archivos = new List<string>();
            string? continuationToken = null;
            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = string.Empty,
                    ContinuationToken = continuationToken
                };
                var response = await _s3Client.ListObjectsV2Async(request);
                archivos.AddRange(response.S3Objects
                    .Where(o => o.Key.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && !o.Key.StartsWith(_procesadosPrefix))
                    .Select(o => o.Key));
                continuationToken = (response.IsTruncated == true) ? response.NextContinuationToken : null;
            } while (continuationToken != null);
            return archivos;
        }

        private async Task<Stream> DescargarArchivoAsync(string key)
        {
            var response = await _s3Client.GetObjectAsync(_bucketName, key);
            var memStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memStream);
            memStream.Position = 0;
            return memStream;
        }

        private async Task MoverArchivoProcesadoAsync(string key)
        {
            var destinoKey = _procesadosPrefix + Path.GetFileName(key);
            // Copiar a procesados
            await _s3Client.CopyObjectAsync(_bucketName, key, _bucketName, destinoKey);
            // Borrar original
            await _s3Client.DeleteObjectAsync(_bucketName, key);
        }
    }
}
