using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using SGMG.Dtos.Request.Triaje;
using SGMG.Data;
using Microsoft.EntityFrameworkCore;

namespace SGMG.Services.ServiceImpl
{
  public class TriajeService : ITriajeService
  {
    private readonly ITriajeRepository _triajeRepository;
    private readonly ApplicationDbContext _context;

    public TriajeService(ITriajeRepository triajeRepository, ApplicationDbContext context)
    {
      _triajeRepository = triajeRepository;
      _context = context;
    }

    public async Task<GenericResponse<Triaje>> AddTriajeAsync(TriajeRequestDTO triajeRequestDTO)
    {
      try
      {
        if (triajeRequestDTO == null)
          return new GenericResponse<Triaje>(false, "El triaje no puede ser nulo.");

        var triaje = MapToTriaje(triajeRequestDTO);

        // Registrar el triaje
        await _triajeRepository.AddTriajeAsync(triaje);

        // Actualizar el estado de la cita a "Triada"
        await ActualizarEstadoCita(triajeRequestDTO.IdPaciente);

        return new GenericResponse<Triaje>(true, triaje, "Triaje registrado exitosamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<Triaje>(false, $"Error al registrar el triaje: {ex.Message}");
      }
    }

    private async Task ActualizarEstadoCita(int idPaciente)
    {
      try
      {
        Console.WriteLine($"Actualizando cita del paciente: {idPaciente}");

        // Traer todas las citas confirmadas del paciente
        var citas = await _context.Citas
            .Where(c => c.IdPaciente == idPaciente && c.EstadoCita == "Confirmada")
            .ToListAsync();

        // Ordenar en memoria (evita error de TimeSpan en SQLite)
        var cita = citas
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .FirstOrDefault();

        if (cita != null)
        {
          Console.WriteLine($"Cita encontrada ID: {cita.IdCita}, Estado: {cita.EstadoCita}");

          cita.EstadoCita = "Triada";
          _context.Entry(cita).Property(c => c.EstadoCita).IsModified = true;

          await _context.SaveChangesAsync();

          Console.WriteLine($"Cita ID {cita.IdCita} actualizada a: Triada");
        }
        else
        {
          Console.WriteLine($"No se encontró cita confirmada para el paciente {idPaciente}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error al actualizar cita: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
      }
    }

    public async Task<GenericResponse<TriajeResponseDTO>> GetTriajeByIdAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<TriajeResponseDTO>(false, "El ID del triaje no es válido.");

        var triaje = await _triajeRepository.GetTriajeByIdAsync(id);

        if (triaje == null)
          return new GenericResponse<TriajeResponseDTO>(false, "Triaje no encontrado.");

        // Mapear a DTO completo
        var triajeDTO = new TriajeResponseDTO
        {
          // Datos del triaje
          IdTriaje = triaje.IdTriage,
          Temperatura = triaje.Temperatura,
          PresionArterial = triaje.PresionArterial,
          Saturacion = triaje.Saturacion,
          FrecuenciaCardiaca = triaje.FrecuenciaCardiaca,
          FrecuenciaRespiratoria = triaje.FrecuenciaRespiratoria,
          Peso = triaje.Peso,
          Talla = triaje.Talla,
          PerimetroAbdominal = triaje.PerimetroAbdominal,
          SuperficieCorporal = triaje.SuperficieCorporal,
          Imc = triaje.Imc,
          ClasificacionImc = triaje.ClasificacionImc,
          RiesgoEnfermedad = triaje.RiesgoEnfermedad,
          EstadoTriage = triaje.EstadoTriage,
          FechaTriage = triaje.FechaTriage,
          HoraTriage = triaje.HoraTriage,
          Observaciones = triaje.Observaciones,

          // Datos del paciente (COMPLETOS)
          IdPaciente = triaje.IdPaciente,
          NumeroDocumento = triaje.Paciente?.NumeroDocumento ?? "",
          TipoDocumento = triaje.Paciente?.TipoDocumento ?? "",
          Nombre = triaje.Paciente?.Nombre ?? "",
          ApellidoPaterno = triaje.Paciente?.ApellidoPaterno ?? "",
          ApellidoMaterno = triaje.Paciente?.ApellidoMaterno ?? "",
          Sexo = triaje.Paciente?.Sexo ?? "",
          Edad = triaje.Paciente?.Edad ?? 0,
          NombreCompletoPaciente = triaje.Paciente != null
                ? $"{triaje.Paciente.ApellidoPaterno} {triaje.Paciente.ApellidoMaterno} {triaje.Paciente.Nombre}".Trim()
                : ""
        };

        return new GenericResponse<TriajeResponseDTO>(true, triajeDTO, "Triaje obtenido correctamente.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en GetTriajeByIdAsync: {ex.Message}");
        return new GenericResponse<TriajeResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<TriajeResponseDTO>> UpdateTriajeAsync(TriajeRequestDTO triajeRequestDTO)
    {
      try
      {
        if (triajeRequestDTO == null || triajeRequestDTO.IdPaciente <= 0)
          return new GenericResponse<TriajeResponseDTO>(false, "Datos inválidos.");

        // Obtener triaje (incluye Paciente)
        var triaje = await _triajeRepository.GetTriajeByIdAsync(triajeRequestDTO.IdTriaje);
        if (triaje == null)
          return new GenericResponse<TriajeResponseDTO>(false, "Triaje no encontrado.");

        // Guardar referencia al paciente
        var paciente = triaje.Paciente;

        // Actualizar solo campos del triaje
        triaje = MapToTriaje(triajeRequestDTO, triaje);
        await _triajeRepository.UpdateTriajeAsync(triaje);

        // Mapear a DTO (sin consulta adicional)
        var triajeDTO = new TriajeResponseDTO
        {
          // Datos del triaje (actualizados)
          IdTriaje = triaje.IdTriage,
          Temperatura = triaje.Temperatura,
          PresionArterial = triaje.PresionArterial,
          Saturacion = triaje.Saturacion,
          FrecuenciaCardiaca = triaje.FrecuenciaCardiaca,
          FrecuenciaRespiratoria = triaje.FrecuenciaRespiratoria,
          Peso = triaje.Peso,
          Talla = triaje.Talla,
          PerimetroAbdominal = triaje.PerimetroAbdominal,
          SuperficieCorporal = triaje.SuperficieCorporal,
          Imc = triaje.Imc,
          ClasificacionImc = triaje.ClasificacionImc,
          RiesgoEnfermedad = triaje.RiesgoEnfermedad,
          EstadoTriage = triaje.EstadoTriage,
          FechaTriage = triaje.FechaTriage,
          HoraTriage = triaje.HoraTriage,
          Observaciones = triaje.Observaciones,

          // Datos del paciente (de memoria, no de BD)
          IdPaciente = triaje.IdPaciente,
          NumeroDocumento = paciente?.NumeroDocumento ?? "",
          TipoDocumento = paciente?.TipoDocumento ?? "",
          Nombre = paciente?.Nombre ?? "",
          ApellidoPaterno = paciente?.ApellidoPaterno ?? "",
          ApellidoMaterno = paciente?.ApellidoMaterno ?? "",
          Sexo = paciente?.Sexo ?? "",
          Edad = paciente?.Edad ?? 0
        };

        return new GenericResponse<TriajeResponseDTO>(true, triajeDTO, "Triaje actualizado correctamente.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en UpdateTriajeAsync: {ex.Message}");
        return new GenericResponse<TriajeResponseDTO>(false, $"Error: {ex.Message}");
      }
    }


    public async Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> GetAllTriajesAsync()
    {
      try
      {
        //throw new Exception("Error simulado para pruebas");
        Console.WriteLine("Obteniendo todos los triajes...");

        // Traer todos los triajes
        var triajes = await _triajeRepository.GetAllTriajesAsync();

        if (triajes == null || !triajes.Any())
        {
          return new GenericResponse<IEnumerable<TriajeResponseDTO>>(
              true,
              new List<TriajeResponseDTO>(),
              "No hay triajes registrados."
          );
        }

        // Obtener IDs únicos de pacientes
        var idsPacientes = triajes.Select(t => t.IdPaciente).Distinct().ToList();

        // Traer TODAS las citas "Triadas" de esos pacientes en UNA sola consulta
        var citasPagadas = await _context.Citas
        .Include(c => c.Medico)
        .Where(c => idsPacientes.Contains(c.IdPaciente) &&
                    c.EstadoCita == "Pagado" &&           // ✅ Estado Pagado
                    c.IdTriage != null)                   // ✅ Tiene triaje
        .ToListAsync();



        // Mapear a DTO
        var triajesDTO = triajes.Select(t =>
        {
          // Buscar la cita del paciente (ya está en memoria)
          var cita = citasPagadas
              .Where(c => c.IdPaciente == t.IdPaciente)
              .OrderByDescending(c => c.FechaCita)
              .ThenByDescending(c => c.HoraCita)
              .FirstOrDefault();

          return new TriajeResponseDTO
          {
            // Datos del triaje
            IdTriaje = t.IdTriage,
            Temperatura = t.Temperatura,
            PresionArterial = t.PresionArterial,
            Saturacion = t.Saturacion,
            FrecuenciaCardiaca = t.FrecuenciaCardiaca,
            FrecuenciaRespiratoria = t.FrecuenciaRespiratoria,
            Peso = t.Peso,
            Talla = t.Talla,
            PerimetroAbdominal = t.PerimetroAbdominal,
            SuperficieCorporal = t.SuperficieCorporal,
            Imc = t.Imc,
            ClasificacionImc = t.ClasificacionImc,
            RiesgoEnfermedad = t.RiesgoEnfermedad,
            EstadoTriage = t.EstadoTriage,
            FechaTriage = t.FechaTriage,
            HoraTriage = t.HoraTriage,
            Observaciones = t.Observaciones,

            // Datos del paciente
            IdPaciente = t.IdPaciente,
            NumeroDocumento = t.Paciente?.NumeroDocumento ?? "",
            TipoDocumento = t.Paciente?.TipoDocumento ?? "",
            Nombre = t.Paciente?.Nombre ?? "",
            ApellidoPaterno = t.Paciente?.ApellidoPaterno ?? "",
            ApellidoMaterno = t.Paciente?.ApellidoMaterno ?? "",
            Sexo = t.Paciente?.Sexo ?? "",
            Edad = t.Paciente?.Edad ?? 0,
            NombreCompletoPaciente = t.Paciente != null
                          ? $"{t.Paciente.ApellidoPaterno} {t.Paciente.ApellidoMaterno} {t.Paciente.Nombre}".Trim()
                          : "",

            // Datos de la cita
            Consultorio = cita?.Consultorio ?? "N/A",
            HoraCita = cita?.HoraCita.ToString(@"hh\:mm") ?? "00:00",
            FechaCita = cita?.FechaCita,
            NombreCompletoMedico = cita?.Medico != null
                          ? $"{cita.Medico.ApellidoPaterno} {cita.Medico.ApellidoMaterno} {cita.Medico.Nombre}".Trim()
                          : "N/A"
          };
        }).ToList();

        Console.WriteLine($"Total triajes procesados: {triajesDTO.Count}");

        return new GenericResponse<IEnumerable<TriajeResponseDTO>>(
            true,
            triajesDTO,
            "Triajes obtenidos correctamente."
        );
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en GetAllTriajesAsync: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        return new GenericResponse<IEnumerable<TriajeResponseDTO>>(
            false,
            "No se pudo cargar la lista de pacientes. Por favor, intente nuevamente"
        );
      }
    }

    private Triaje MapToTriaje(TriajeRequestDTO dto, Triaje? triaje = null)
    {
      if (triaje == null)
        triaje = new Triaje();

      triaje.IdPaciente = dto.IdPaciente;
      triaje.Temperatura = dto.Temperatura;
      triaje.PresionArterial = dto.PresionArterial;
      triaje.Saturacion = dto.Saturacion;
      triaje.FrecuenciaCardiaca = dto.FrecuenciaCardiaca;
      triaje.FrecuenciaRespiratoria = dto.FrecuenciaRespiratoria;
      triaje.Peso = dto.Peso;
      triaje.Talla = dto.Talla;
      triaje.PerimetroAbdominal = dto.PerimAbdominal;
      triaje.SuperficieCorporal = dto.SuperficieCorporal;
      triaje.Imc = dto.Imc;
      triaje.ClasificacionImc = dto.ClasificacionImc;
      triaje.RiesgoEnfermedad = dto.RiesgoEnfermedad;
      triaje.EstadoTriage = dto.EstadoTriage;
      triaje.Observaciones = dto.Observaciones;

      return triaje;
    }

    public async Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> BuscarTriajesAsync(string? tipoDoc, string? numeroDoc, DateTime? fechaInicio, DateTime? fechaFin)
    {
      try
      {
        Console.WriteLine($"Buscando triajes: TipoDoc={tipoDoc}, NumDoc={numeroDoc}, FechaInicio={fechaInicio}, FechaFin={fechaFin}");

        // Construir query base
        var query = _context.Triages
            .Include(t => t.Paciente)
            .AsQueryable();

        // Filtrar por documento si se proporcionó
        if (!string.IsNullOrWhiteSpace(tipoDoc) && !string.IsNullOrWhiteSpace(numeroDoc))
        {
          query = query.Where(t =>
              t.Paciente.TipoDocumento == tipoDoc &&
              t.Paciente.NumeroDocumento == numeroDoc);
        }

        // Filtrar por rango de fechas
        if (fechaInicio.HasValue)
        {
          query = query.Where(t => t.FechaTriage >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
          var fechaFinInclusiva = fechaFin.Value.Date.AddDays(1);
          query = query.Where(t => t.FechaTriage < fechaFinInclusiva);
        }

        var triajes = await query.ToListAsync();

        if (!triajes.Any())
        {
          return new GenericResponse<IEnumerable<TriajeResponseDTO>>(
              true,
              new List<TriajeResponseDTO>(),
              "No se encontraron triajes con los criterios especificados");
        }

        // Ordenar en memoria
        triajes = triajes
            .OrderByDescending(t => t.FechaTriage)
            .ThenByDescending(t => t.HoraTriage)
            .ToList();

        // Obtener IDs de pacientes
        var idsPacientes = triajes.Select(t => t.IdPaciente).Distinct().ToList();

        // Traer citas triadas
        var citasTriadas = await _context.Citas
            .Include(c => c.Medico)
            .Where(c => idsPacientes.Contains(c.IdPaciente) && c.EstadoCita == "Triada")
            .ToListAsync();

        var citasPorPaciente = citasTriadas
            .GroupBy(c => c.IdPaciente)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.FechaCita)
                       .ThenByDescending(c => c.HoraCita)
                       .First()
            );

        // Mapear a DTO
        var triajesDTO = triajes.Select(t =>
        {
          citasPorPaciente.TryGetValue(t.IdPaciente, out var cita);

          return new TriajeResponseDTO
          {
            IdTriaje = t.IdTriage,
            Temperatura = t.Temperatura,
            PresionArterial = t.PresionArterial,
            Saturacion = t.Saturacion,
            FrecuenciaCardiaca = t.FrecuenciaCardiaca,
            FrecuenciaRespiratoria = t.FrecuenciaRespiratoria,
            Peso = t.Peso,
            Talla = t.Talla,
            PerimetroAbdominal = t.PerimetroAbdominal,
            SuperficieCorporal = t.SuperficieCorporal,
            Imc = t.Imc,
            ClasificacionImc = t.ClasificacionImc,
            RiesgoEnfermedad = t.RiesgoEnfermedad,
            EstadoTriage = t.EstadoTriage,
            FechaTriage = t.FechaTriage,
            HoraTriage = t.HoraTriage,
            Observaciones = t.Observaciones,

            IdPaciente = t.IdPaciente,
            NumeroDocumento = t.Paciente?.NumeroDocumento ?? "",
            TipoDocumento = t.Paciente?.TipoDocumento ?? "",
            Nombre = t.Paciente?.Nombre ?? "",
            ApellidoPaterno = t.Paciente?.ApellidoPaterno ?? "",
            ApellidoMaterno = t.Paciente?.ApellidoMaterno ?? "",
            Sexo = t.Paciente?.Sexo ?? "",
            Edad = t.Paciente?.Edad ?? 0,
            NombreCompletoPaciente = t.Paciente != null
                          ? $"{t.Paciente.ApellidoPaterno} {t.Paciente.ApellidoMaterno} {t.Paciente.Nombre}".Trim()
                          : "",

            Consultorio = cita?.Consultorio ?? "N/A",
            HoraCita = cita?.HoraCita.ToString(@"hh\:mm") ?? "00:00",
            FechaCita = cita?.FechaCita,
            NombreCompletoMedico = cita?.Medico != null
                          ? $"{cita.Medico.ApellidoPaterno} {cita.Medico.ApellidoMaterno} {cita.Medico.Nombre}".Trim()
                          : "N/A"
          };
        }).ToList();

        Console.WriteLine($"Triajes encontrados: {triajesDTO.Count}");

        return new GenericResponse<IEnumerable<TriajeResponseDTO>>(
            true,
            triajesDTO,
            $"Se encontraron {triajesDTO.Count} triajes");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en BuscarTriajesAsync: {ex.Message}");
        return new GenericResponse<IEnumerable<TriajeResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPagadasPorConsultorioEnfermeraAsync(int idEnfermera)
    {
      try
      {
        // Obtener el consultorio de la enfermera
        var enfermera = await _context.Enfermerias
            .Include(e => e.ConsultorioAsignado)
            .FirstOrDefaultAsync(e => e.IdEnfermeria == idEnfermera);

        if (enfermera == null)
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "Enfermera no encontrada");
        }

        if (enfermera.ConsultorioAsignado == null)
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "La enfermera no tiene consultorio asignado");
        }

        var idConsultorio = enfermera.ConsultorioAsignado.IdConsultorio;

        // Buscar citas con estado "Pagado" de médicos del mismo consultorio
        var citas = await _context.Citas
    .Include(c => c.Paciente)
    .Include(c => c.Medico)
    .Where(c => c.EstadoCita == "Pagado" &&
               c.Medico != null &&
               c.Medico.IdConsultorio == idConsultorio &&
               c.IdTriage == null) // ✅ Solo citas SIN triaje
    .ToListAsync();


        // ORDENAR EN MEMORIA (después de obtener los datos)
        citas = citas
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraCita)
            .ToList();

        var citasDTO = citas.Select(c => new CitaResponseDTO
        {
          IdCita = c.IdCita,
          IdPaciente = c.IdPaciente,
          IdMedico = c.IdMedico,
          NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
          NombreCompletoPaciente = c.Paciente != null
                ? $"{c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno} {c.Paciente.Nombre}".Trim()
                : "",
          Consultorio = c.Consultorio,
          HoraCita = c.HoraCita,
          FechaCita = c.FechaCita,
          NombreCompletoMedico = c.Medico != null
                ? $"{c.Medico.ApellidoPaterno} {c.Medico.ApellidoMaterno} {c.Medico.Nombre}".Trim()
                : "",
          EstadoCita = c.EstadoCita,
          Especialidad = c.Especialidad
        }).ToList();

        return new GenericResponse<IEnumerable<CitaResponseDTO>>(
            true,
            citasDTO,
            $"Citas pagadas del consultorio {enfermera.ConsultorioAsignado.Nombre} cargadas correctamente");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }



    public async Task<GenericResponse<HistorialTriajeDTO>> GetHistorialTriajePacienteAsync(int idPaciente)
    {
      try
      {
        Console.WriteLine($"Obteniendo historial de triajes para paciente {idPaciente}");

        // 1. Obtener información del paciente
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

        if (paciente == null)
          return new GenericResponse<HistorialTriajeDTO>(false, "Paciente no encontrado");

        // 2. Obtener historia clínica para el seguro
        var historiaClinica = await _context.HistoriasClinicas
            .FirstOrDefaultAsync(h => h.IdPaciente == idPaciente);

        // 3. Obtener triajes del paciente
        var triajes = await _triajeRepository.GetTriajesByPacienteAsync(idPaciente);

        // 4. Calcular edad
        int edad = paciente.Edad;
        if (historiaClinica?.FechaNacimiento != null)
        {
          var today = DateTime.Today;
          edad = today.Year - historiaClinica.FechaNacimiento.Year;
          if (historiaClinica.FechaNacimiento.Date > today.AddYears(-edad)) edad--;
        }

        // 5. Mapear a DTO
        var historialDTO = new HistorialTriajeDTO
        {
          Paciente = new PacienteInfoDTO
          {
            Dni = paciente.NumeroDocumento,
            NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim(),
            Sexo = paciente.Sexo == "M" ? "Masculino" : "Femenino",
            Edad = edad,
            Seguro = historiaClinica?.TipoSeguro ?? "No registrado"
          },
          Triajes = triajes.Select(t => new TriajeDetalladoDTO
          {
            CodigoTriaje = $"#TRI{t.IdTriage:D3}",
            FechaHora = $"{t.FechaTriage:dd/MM/yyyy} {t.HoraTriage:hh\\:mm}",
            PresionArterial = $"{t.PresionArterial}/80 mmHg",
            Temperatura = $"{t.Temperatura}°C",
            Signos = new SignosVitalesDTO
            {
              Temperatura = t.Temperatura,
              PresionArterial = t.PresionArterial,
              FrecuenciaCardiaca = t.FrecuenciaCardiaca,
              Saturacion = t.Saturacion
            },
            Medidas = new MedidasAntropometricasDTO
            {
              Peso = t.Peso,
              Talla = t.Talla,
              Imc = t.Imc,
              ImcClasificacion = t.ClasificacionImc
            },
            Informacion = new InformacionTriajeDTO
            {
              Fecha = t.FechaTriage.ToString("dd/MM/yyyy"),
              Hora = t.HoraTriage.ToString(@"hh\:mm"),
              RealizadoPor = "Enf. Juan Pérez" // TODO: Obtener del contexto
            },
            Observaciones = t.Observaciones
          }).ToList()
        };

        Console.WriteLine($"Historial obtenido: {historialDTO.Triajes.Count} triajes");

        return new GenericResponse<HistorialTriajeDTO>(
            true,
            historialDTO,
            "Historial obtenido correctamente"
        );
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en GetHistorialTriajePacienteAsync: {ex.Message}");
        return new GenericResponse<HistorialTriajeDTO>(
            false,
            $"Error: {ex.Message}"
        );
      }
    }


  }
}

