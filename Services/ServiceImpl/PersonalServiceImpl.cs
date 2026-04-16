using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Repository;
using SGMG.Services;
using SGMG.common.exception;


namespace SGMG.Services.ServiceImpl
{
  public class PersonalServiceImpl : IPersonalService
  {
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<PersonalServiceImpl> _logger;

    public PersonalServiceImpl(IPersonalRepository personalRepository, ILogger<PersonalServiceImpl> logger)
    {
      _personalRepository = personalRepository;
      _logger = logger;
    }

    public async Task<GenericResponse<ResumenPersonalResponse>> GetResumenPersonalAsync()
    {
      try
      {
        var resumen = await _personalRepository.GetResumenPersonalAsync();
        if (resumen == null)
          return new GenericResponse<ResumenPersonalResponse>(false, "No se pudo obtener el resumen del personal.");

        // Logging detallado de cada propiedad
        _logger.LogInformation(
            "ResumenPersonalResponse: MedicosActivos={MedicosActivos}, TecnicosActivos={TecnicosActivos}, Consultorios={Consultorios}, PersonalCaja={PersonalCaja}, TotalCargos={TotalCargos}, TotalConsultoriosList={TotalConsultoriosList}",
            resumen.MedicosActivos,
            resumen.TecnicosActivos,
            resumen.Consultorios,
            resumen.PersonalCaja,
            resumen.Cargos?.Count ?? 0,
            resumen.ConsultoriosList?.Count ?? 0
        );
        return new GenericResponse<ResumenPersonalResponse>(true, resumen, "Resumen del personal obtenido correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al obtener el resumen del personal.", ex);
      }
    }
    public async Task<GenericResponse<IEnumerable<PersonalRegistradoResponse>>> BuscarPersonalAsync(PersonalFiltroRequest filtro)
    {
      try
      {
        if (filtro == null)
          return new GenericResponse<IEnumerable<PersonalRegistradoResponse>>(false, "Los filtros de búsqueda no pueden ser nulos.");
        // Validar que si busca por consultorio, el tipo de personal sea Medico o Enfermeria
        if (filtro.IdConsultorio.HasValue && filtro.IdConsultorio.Value > 0)
        {
          if (!string.IsNullOrEmpty(filtro.TipoPersonal))
          {
            var tipoValido = filtro.TipoPersonal.Equals("Medico General", StringComparison.OrdinalIgnoreCase) ||
                            filtro.TipoPersonal.Equals("Enfermeria", StringComparison.OrdinalIgnoreCase) ||
                            filtro.TipoPersonal.Equals("Todos", StringComparison.OrdinalIgnoreCase);

            if (!tipoValido)
              return new GenericResponse<IEnumerable<PersonalRegistradoResponse>>(false, "El filtro de consultorio solo aplica para Médicos y Enfermería.");
          }
        }
        var personal = await _personalRepository.BuscarPersonalAsync(filtro);
        if (personal == null || !personal.Any())
          return new GenericResponse<IEnumerable<PersonalRegistradoResponse>>(false, "No se encontraron registros con los filtros especificados.");
        return new GenericResponse<IEnumerable<PersonalRegistradoResponse>>(true, personal, $"Se encontraron {personal.Count()} registro(s) de personal.");
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al buscar el personal.", ex);
      }
    }
  }
}