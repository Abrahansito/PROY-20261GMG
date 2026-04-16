using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Response;
using Microsoft.EntityFrameworkCore;
using PROY_20252SGMG.Dtos.Request;
using PROY_20252SGMG.Dtos.Response;
using PROY_20252SGMG.Services;
using SGMG.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PROY_20252SGMG.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RecetaMedicaController : ControllerBase
  {
    private readonly ILogger<RecetaMedicaController> _logger;
    private readonly IRecetaService _recetaService;
    private readonly ApplicationDbContext _context;

    public RecetaMedicaController(ILogger<RecetaMedicaController> logger, IRecetaService recetaService, ApplicationDbContext context)
    {
      _logger = logger;
      _recetaService = recetaService;
      _context = context;
    }

    /// <summary>
    /// Agregar un medicamento individual con estado PENDIENTE
    /// </summary>
    [HttpPost]
    [Route("AgregarMedicamento")]
    public async Task<ActionResult<GenericResponse<DetalleRecetaResponseDTO>>> AgregarMedicamento(
        [FromBody] AgregarMedicamentoRequestDTO request)
    {
      try
      {
        var result = await _recetaService.AgregarMedicamentoPendienteAsync(request);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al agregar medicamento");
        return BadRequest(new GenericResponse<DetalleRecetaResponseDTO>
        {
          Success = false,
          Message = "Error al agregar medicamento: " + ex.Message
        });
      }
    }

    /// <summary>
    /// Obtener medicamentos PENDIENTES por HC, Médico y Paciente
    /// </summary>
    [HttpGet]
    [Route("MedicamentosPendientes")]
    public async Task<ActionResult<GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>>> GetMedicamentosPendientes(
        [FromQuery] int idCita,
        [FromQuery] int idMedico,
        [FromQuery] int idPaciente,
        [FromQuery] int? idHistoriaClinica = null)
    {
      try
      {
        var result = await _recetaService.GetMedicamentosPendientesAsync(
            idCita, idMedico, idPaciente, idHistoriaClinica);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener medicamentos pendientes");
        return BadRequest(new GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>
        {
          Success = false,
          Message = "Error: " + ex.Message
        });
      }
    }

    /// <summary>
    /// Eliminar un medicamento PENDIENTE
    /// </summary>
    [HttpDelete]
    [Route("EliminarMedicamento/{idDetalle}")]
    public async Task<ActionResult<GenericResponse<bool>>> EliminarMedicamento(int idDetalle)
    {
      try
      {
        var result = await _recetaService.EliminarMedicamentoPendienteAsync(idDetalle);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar medicamento");
        return BadRequest(new GenericResponse<bool>
        {
          Success = false,
          Message = "Error: " + ex.Message
        });
      }
    }

    /// <summary>
    /// Guardar receta: Cambia todos los medicamentos PENDIENTES a COMPLETADO
    /// </summary>
    [HttpPost]
    [Route("GuardarReceta")]
    public async Task<ActionResult<GenericResponse<RecetaResponseDTO>>> GuardarReceta(
        [FromBody] RecetaRequestDTO request)
    {
      try
      {
        var result = await _recetaService.CompletarRecetaAsync(request);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al guardar receta");
        return BadRequest(new GenericResponse<RecetaResponseDTO>
        {
          Success = false,
          Message = "Error al guardar la receta: " + ex.Message
        });
      }
    }
    [HttpGet]
    [Route("paciente/{idPaciente}/lista")]
    public async Task<ActionResult<GenericResponse<IEnumerable<RecetaResponseDTO>>>> GetRecetasListByPaciente(int idPaciente)
    {
      var result = await _recetaService.GetRecetasByPacienteAsyncCompleto(idPaciente);
      return Ok(result);
    }

    /// <summary>
    /// Obtener receta por ID
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<GenericResponse<RecetaResponseDTO>>> GetRecetaById(int id)
    {
      var result = await _recetaService.GetRecetaByIdAsync(id);
      return Ok(result);
    }

    /// <summary>
    /// Obtener recetas por paciente
    /// </summary>
    [HttpGet]
    [Route("paciente/{idPaciente}")]
    public async Task<ActionResult<GenericResponse<IEnumerable<RecetaResponseDTO>>>> GetRecetasByPaciente(int idPaciente)
    {
      var result = await _recetaService.GetRecetasByPacienteAsync(idPaciente);
      return Ok(result);
    }

    /// <summary>
    /// Obtener recetas por médico
    /// </summary>
    [HttpGet]
    [Route("medico/{idMedico}")]
    public async Task<ActionResult<GenericResponse<IEnumerable<RecetaResponseDTO>>>> GetRecetasByMedico(int idMedico)
    {
      var result = await _recetaService.GetRecetasByMedicoAsync(idMedico);
      return Ok(result);
    }

    /// <summary>
    /// Obtener receta por cita
    /// </summary>
    [HttpGet]
    [Route("cita/{idCita}")]
    public async Task<ActionResult<GenericResponse<RecetaResponseDTO>>> GetRecetaByCita(int idCita)
    {
      var result = await _recetaService.GetRecetaByCitaAsync(idCita);
      return Ok(result);
    }

    [HttpGet("imprimir/{idReceta}")]
    public async Task<IActionResult> ImprimirReceta(int idReceta)
    {
      try
      {
        var receta = await _context.Recetas
            .Include(r => r.Medico)
            .Include(r => r.Paciente)
            .Include(r => r.Detalles)
            .Include(r => r.Cita)
            .FirstOrDefaultAsync(r => r.IdReceta == idReceta);

        if (receta == null)
        {
          return NotFound(new { success = false, message = "Receta no encontrada" });
        }

        var codigoReceta = $"RX-{receta.FechaEmision.Year}-{receta.IdReceta:D4}";
        var nombreCompletoMedico = $"Dr(a). {receta.Medico.Nombre} {receta.Medico.ApellidoPaterno} {receta.Medico.ApellidoMaterno}";
        var nombreCompletoPaciente = $"{receta.Paciente.Nombre} {receta.Paciente.ApellidoPaterno} {receta.Paciente.ApellidoMaterno}";

        var document = Document.Create(container =>
        {
          container.Page(page =>
            {
              page.Size(PageSizes.A4);
              page.Margin(40);
              page.PageColor(Colors.White);
              page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Black));

              page.Header().Column(column =>
                {
                  // Encabezado del hospital
                  column.Item().Border(2).BorderColor(Colors.Black).Padding(15).Column(headerColumn =>
                    {
                      headerColumn.Item().AlignCenter().Text("HOSPITAL NACIONAL HIPÓLITO UNANUE")
                            .Bold().FontSize(16).FontColor(Colors.Black);

                      headerColumn.Item().AlignCenter().Text("RECETA MÉDICA")
                            .Bold().FontSize(14).FontColor(Colors.Black);

                      headerColumn.Item().PaddingTop(5).AlignCenter().Text("Av. César Vallejo 1390, El Agustino - Lima, Perú")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);

                      headerColumn.Item().AlignCenter().Text("Tel: (01) 362-5110")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                  column.Item().PaddingTop(10).Row(row =>
                    {
                      row.RelativeItem().Column(col =>
                        {
                          col.Item().Text(text =>
                            {
                              text.Span("Código de Receta: ").Bold().FontSize(11);
                              text.Span(codigoReceta).FontSize(11);
                            });
                          col.Item().Text(text =>
                            {
                              text.Span("Fecha: ").Bold();
                              text.Span(receta.FechaEmision.ToString("dd/MM/yyyy HH:mm"));
                            });
                        });

                      row.RelativeItem().AlignRight().Column(col =>
                        {
                          col.Item().Text(text =>
                            {
                              text.Span("Consultorio: ").Bold();
                              text.Span(receta.Cita?.Consultorio ?? "N/A");
                            });
                        });
                    });
                });

              page.Content().PaddingVertical(10).Column(column =>
                {
                  // Datos del Paciente - SIN BORDE
                  column.Item().ShowOnce().PaddingVertical(5).Column(pacienteColumn =>
                    {
                      pacienteColumn.Item().Text("DATOS DEL PACIENTE")
                            .Bold().FontSize(11).FontColor(Colors.Black);

                      pacienteColumn.Item().PaddingTop(8).Row(row =>
                        {
                          row.RelativeItem().Column(col =>
                            {
                              col.Item().Text(text =>
                                {
                                  text.Span("Nombre: ").Bold();
                                  text.Span(nombreCompletoPaciente);
                                });
                              col.Item().Text(text =>
                                {
                                  text.Span("Edad: ").Bold();
                                  text.Span($"{receta.Paciente.Edad} años");
                                });
                            });

                          row.RelativeItem().Column(col =>
                            {
                              col.Item().Text(text =>
                                {
                                  text.Span($"{receta.Paciente.TipoDocumento}: ").Bold();
                                  text.Span(receta.Paciente.NumeroDocumento);
                                });
                              col.Item().Text(text =>
                                {
                                  text.Span("Sexo: ").Bold();
                                  text.Span(receta.Paciente.Sexo);
                                });
                            });
                        });
                    });

                  // Datos del Médico - SIN BORDE
                  column.Item().ShowOnce().PaddingTop(10).Column(medicoColumn =>
                    {
                      medicoColumn.Item().Text("MÉDICO TRATANTE")
                            .Bold().FontSize(11).FontColor(Colors.Black);

                      medicoColumn.Item().PaddingTop(8).Row(row =>
                        {
                          row.RelativeItem().Column(col =>
                            {
                              col.Item().Text(text =>
                                {
                                  text.Span("Nombre: ").Bold();
                                  text.Span(nombreCompletoMedico);
                                });
                              col.Item().Text(text =>
                                {
                                  text.Span("Especialidad: ").Bold();
                                  text.Span(receta.Medico.AreaServicio);
                                });
                            });

                          row.RelativeItem().Column(col =>
                            {
                              col.Item().Text(text =>
                                {
                                  text.Span("CMP: ").Bold();
                                  text.Span(receta.Medico.NumeroColegiatura);
                                });
                            });
                        });
                    });

                  // Medicamentos
                  column.Item().PaddingTop(15).Column(medColumn =>
                    {
                      medColumn.Item().Background(Colors.Grey.Lighten2).Padding(8).Text("Rp/ MEDICAMENTOS PRESCRITOS")
                            .Bold().FontSize(12).FontColor(Colors.Black);

                      medColumn.Item().PaddingTop(10);

                      var contador = 1;
                      foreach (var detalle in receta.Detalles)
                      {
                        medColumn.Item().PaddingBottom(15).Border(1).BorderColor(Colors.Black)
                              .Padding(12).Column(detalleColumn =>
                          {
                            detalleColumn.Item().Row(row =>
                              {
                                row.ConstantItem(25).Text($"{contador}.")
                                      .Bold().FontSize(11).FontColor(Colors.Black);
                                row.RelativeItem().Text(detalle.ProductoFarmaceutico)
                                      .Bold().FontSize(11).FontColor(Colors.Black);
                              });

                            detalleColumn.Item().PaddingLeft(25).Text(text =>
                              {
                                text.Span("Concentración: ").Italic().FontColor(Colors.Grey.Darken2);
                                text.Span(detalle.Concentracion).Bold();
                              });

                            detalleColumn.Item().PaddingTop(5).PaddingLeft(25).Column(indicacionesCol =>
                              {
                                indicacionesCol.Item().Text(text =>
                                  {
                                    text.Span("• Frecuencia: ").Bold();
                                    text.Span(detalle.Frecuencia);
                                  });

                                indicacionesCol.Item().Text(text =>
                                  {
                                    text.Span("• Duración: ").Bold();
                                    text.Span(detalle.Duracion);
                                  });

                                indicacionesCol.Item().Text(text =>
                                  {
                                    text.Span("• Vía: ").Bold();
                                    text.Span(detalle.ViaAdministracion);
                                  });

                                if (!string.IsNullOrWhiteSpace(detalle.Observaciones))
                                {
                                  indicacionesCol.Item().Text(text =>
                                    {
                                      text.Span("• Observaciones: ").Bold();
                                      text.Span(detalle.Observaciones).Italic();
                                    });
                                }
                              });
                          });

                        contador++;
                      }
                    });

                  // Observaciones Generales - SIN BORDE
                  if (!string.IsNullOrWhiteSpace(receta.ObservacionesGenerales))
                  {
                    column.Item().ShowOnce().PaddingTop(15).Column(obsColumn =>
                      {
                        obsColumn.Item().Text("OBSERVACIONES MÉDICAS")
                              .Bold().FontSize(11).FontColor(Colors.Black);

                        obsColumn.Item().PaddingTop(8).Text(receta.ObservacionesGenerales)
                              .FontSize(10).LineHeight(1.3f);
                      });
                  }

                  // Firma - Siempre en la última página
                  column.Item().AlignBottom().AlignRight().PaddingTop(40).Column(firmaColumn =>
                    {
                      firmaColumn.Item().BorderTop(1).BorderColor(Colors.Black).Width(200);
                      firmaColumn.Item().PaddingTop(5).AlignCenter().Text(nombreCompletoMedico)
                            .Bold().FontSize(10);
                      firmaColumn.Item().AlignCenter().Text($"CMP: {receta.Medico.NumeroColegiatura}")
                            .FontSize(9);
                    });
                });

              page.Footer().AlignCenter().Text(text =>
                {
                  text.Span("Página ");
                  text.CurrentPageNumber();
                  text.Span(" de ");
                  text.TotalPages();
                });
            });
        });

        // Generar PDF
        var pdfBytes = document.GeneratePdf();

        // Actualizar estado
        receta.EstadoReceta = "Impresa";
        await _context.SaveChangesAsync();

        // Retornar PDF para descarga directa
        var nombreArchivo = $"Receta_{codigoReceta}.pdf";
        return File(pdfBytes, "application/pdf", nombreArchivo);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al imprimir receta {IdReceta}", idReceta);
        return StatusCode(500, new { success = false, message = "Error al generar el PDF" });
      }
      /// <summary>
      /// Marcar receta como impresa
      /// </summary>



    }
  }
}