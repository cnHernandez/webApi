using ApiSwagger.Data;
using ApiSwagger.Models;
using ApiSwagger.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ApiSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ImportDataController> _logger;
        private readonly ExcelService _excelService;

        public ImportDataController(AppDbContext context, ILogger<ImportDataController> logger)
        {
            _context = context;
            _logger = logger;
            _excelService = new ExcelService(context);
        }

        // Genera un archivo Excel con la estructura del modelo especificado.
        [HttpGet("modelexcel/{modelo}")]
        public IActionResult GenerarExcelModelo(string modelo)
        {
            try
            {
                var excelData = _excelService.GenerarPlantillaExcel(modelo);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{modelo}_Plantilla.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la plantilla Excel para el modelo {Modelo}", modelo);
                return StatusCode(500, "Ocurrió un error al generar la plantilla Excel.");
            }
        }

        // Carga datos masivamente desde un archivo Excel y los guarda en la base de datos.
        [HttpPost("import/{modelo}")]
        public async Task<IActionResult> CargarMasivo(string modelo, IFormFile file)
        {
            try
            {
                var (registrosExitosos, errores) = await _excelService.CargarDatosDesdeExcel(modelo, file);
                return Ok(new { message = $"{registrosExitosos} registros cargados con éxito.", errores });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos masivamente para el modelo {Modelo}", modelo);
                return StatusCode(500, "Ocurrió un error durante la importación de datos.");
            }
        }
    }

    public static class DateTimeExtensions
    {
        public static DateOnly ToDateOnly(this DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }
    }
}