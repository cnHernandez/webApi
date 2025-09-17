using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using ApiSwagger.Data;
using ApiSwagger.Models;

namespace ApiSwagger.Services
{
    public class ExcelService
    {
        private readonly AppDbContext _context;

        public ExcelService(AppDbContext context)
        {
            _context = context;
        }

        public byte[] GenerarPlantillaExcel(string modelo)
        {
            Type? tipoModelo = ObtenerModelo(modelo);
            if (tipoModelo == null)
                throw new Exception($"El modelo '{modelo}' no existe.");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(modelo);
                var propiedades = tipoModelo.GetProperties()
                    .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                for (int col = 0; col < propiedades.Length; col++)
                {
                    worksheet.Cell(1, col + 1).Value = propiedades[col].Name;
                    worksheet.Cell(1, col + 1).Style.Font.Bold = true;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<(int, List<object>)> CargarDatosDesdeExcel(string modelo, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("El archivo es obligatorio.");

            Type? tipoModelo = ObtenerModelo(modelo);
            if (tipoModelo == null)
                throw new Exception($"El modelo '{modelo}' no existe.");

            var listaObjetos = CrearListaTipada(tipoModelo);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var headers = worksheet.Row(1).Cells().Select(c => c.Value.ToString()).ToList();
                    var propiedades = tipoModelo.GetProperties();

                    // Agregar registro detallado para inspeccionar las propiedades de los objetos generados
                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        var instancia = Activator.CreateInstance(tipoModelo);

                        for (int col = 0; col < headers.Count; col++)
                        {
                            var prop = propiedades.FirstOrDefault(p => p.Name.Equals(headers[col], StringComparison.OrdinalIgnoreCase));
                            if (prop == null) continue;

                            var celda = row.Cell(col + 1);
                            object? valor = ConvertirValor(prop.PropertyType, celda.Value);

                            // Asegurarse de que IdColectivo no se asigne desde el archivo Excel
                            if (prop.Name.Equals("IdColectivo", StringComparison.OrdinalIgnoreCase))
                            {
                                continue; // Saltar la asignación de IdColectivo
                            }

                            prop.SetValue(instancia, valor);
                        }

                        listaObjetos.Add(instancia);
                        Console.WriteLine($"Objeto generado: {instancia}");
                        foreach (var prop in propiedades)
                        {
                            Console.WriteLine($"{prop.Name}: {prop.GetValue(instancia)}");
                        }
                    }
                }
            }

            // Reemplazar el uso de reflexión con un manejo explícito del DbSet
            int registrosExitosos = 0;
            var errores = new List<object>();
            if (tipoModelo == typeof(ApiSwagger.Models.Colectivo))
            {
                var dbSet = _context.Colectivos;
                foreach (var objeto in listaObjetos.Cast<ApiSwagger.Models.Colectivo>())
                {
                    dbSet.Add(objeto);
                    registrosExitosos++;
                }
            }
            else
            {
                throw new Exception("El modelo proporcionado no está soportado.");
            }

            // Guardar los cambios en la base de datos
            _context.SaveChanges();

            return (registrosExitosos, errores);
        }

        private Type? ObtenerModelo(string modelo)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(modelo, StringComparison.OrdinalIgnoreCase));
        }

        private IList CrearListaTipada(Type tipoModelo)
        {
            var tipoLista = typeof(List<>).MakeGenericType(tipoModelo);
            return (IList)Activator.CreateInstance(tipoLista)!;
        }

        private object? ConvertirValor(Type tipo, object valorCelda)
        {
            if (valorCelda == null || string.IsNullOrWhiteSpace(valorCelda.ToString()))
                return tipo.IsValueType ? Activator.CreateInstance(tipo) : null;

            try
            {
                if (tipo == typeof(int) || tipo == typeof(int?))
                    return int.TryParse(valorCelda.ToString(), out int intValue) ? intValue : null;

                if (tipo == typeof(decimal) || tipo == typeof(decimal?))
                    return decimal.TryParse(valorCelda.ToString(), out decimal decValue) ? decValue : null;

                if (tipo == typeof(DateTime) || tipo == typeof(DateTime?))
                    return DateTime.TryParse(valorCelda.ToString(), out DateTime dateValue) ? dateValue : null;

                if (tipo == typeof(string))
                    return valorCelda.ToString();

                return Convert.ChangeType(valorCelda, tipo);
            }
            catch
            {
                return tipo.IsValueType ? Activator.CreateInstance(tipo) : null;
            }
        }
    }
}