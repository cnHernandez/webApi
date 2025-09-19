using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ApiSwagger.Data;
using ApiSwagger.Models;

namespace ApiSwagger.Services
{
    public class CsvKilometrajeService
    {
        private readonly AppDbContext _context;
        private readonly string _csvFolder;
        private readonly string _procesadosFolder;

        public CsvKilometrajeService(AppDbContext context, string csvFolder)
        {
            _context = context;
            _csvFolder = csvFolder;
            _procesadosFolder = Path.Combine(csvFolder, "procesados");
            if (!Directory.Exists(_procesadosFolder))
                Directory.CreateDirectory(_procesadosFolder);
        }

        public void ProcesarArchivosCsv()
        {
            var archivos = Directory.GetFiles(_csvFolder, "*.csv");
            Console.WriteLine($"Archivos encontrados: {archivos.Length}");
            foreach (var archivo in archivos)
            {
                Console.WriteLine($"Procesando archivo: {archivo}");
                ProcesarArchivo(archivo);
                var nombreArchivo = Path.GetFileName(archivo);
                var destino = Path.Combine(_procesadosFolder, nombreArchivo);
                File.Move(archivo, destino, true);
                Console.WriteLine($"Movido a: {destino}");
            }
        }

        private void ProcesarArchivo(string archivo)
        {
            using var reader = new StreamReader(archivo);
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
    }
}
