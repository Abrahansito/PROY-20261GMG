using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Services;
using SGMG.common.exception;
using SGMG.Data;
using SGMG.Repository;
using SGMG.Dtos.Request.Enfermeria;

namespace SGMG.Services.ServiceImpl
{
  public class EnfermeriaServiceImpl : IEnfermeriaService
  {
    private readonly IEnfermeriaRepository _enfermeriaRepository;
    private readonly IPersonalTRepository _personalTRepository;
    private readonly ApplicationDbContext _context;

    public EnfermeriaServiceImpl(IEnfermeriaRepository enfermeriaRepository, IPersonalTRepository personalTRepository, ApplicationDbContext context)
    {
      _enfermeriaRepository = enfermeriaRepository;
      _personalTRepository = personalTRepository;
      _context = context;
    }

    public async Task<GenericResponse<IEnumerable<EnfermeriaResponse>>> GetAllEnfermeriasAsync()
    {
      var enfermerias = await _enfermeriaRepository.GetAllEnfermeriasAsync();

      if (enfermerias == null || !enfermerias.Any())
        return new GenericResponse<IEnumerable<EnfermeriaResponse>>(false, "No se encontraron registros de enfermería.");

      return new GenericResponse<IEnumerable<EnfermeriaResponse>>(true, enfermerias, "Registros de enfermería obtenidos correctamente.");
    }

    public async Task<GenericResponse<Enfermeria>> GetEnfermeriaByIdAsync(int id)
    {
      if (id <= 0)
        return new GenericResponse<Enfermeria>(false, "El ID no es válido.");

      var enfermeria = await _enfermeriaRepository.GetEnfermeriaByIdAsync(id);

      if (enfermeria == null)
        return new GenericResponse<Enfermeria>(false, "Registro de enfermería no encontrado.");

      return new GenericResponse<Enfermeria>(true, enfermeria, "Registro de enfermería obtenido correctamente.");
    }

    public async Task<GenericResponse<Enfermeria>> AddEnfermeriaAsync(EnfermeriaRequestDTO dto)
    {
      if (dto == null)
        return new GenericResponse<Enfermeria>(false, "El registro no puede ser nulo.");

      // Crear primero el personal técnico
      var personal = MapToPersonalTecnico(dto);
      _context.PersonalTecnicos.Add(personal);
      await _context.SaveChangesAsync(); // genera el IdPersonal

      var enfermeria = new Enfermeria
      {
        IdPersonal = personal.IdPersonal,
        NumeroColegiaturaEnfermeria = dto.NumeroColegiaturaEnfermeria ?? "",
        NivelProfesional = dto.NivelProfesional ?? "",
        IdConsultorio = dto.IdConsultorio
      };

      await _enfermeriaRepository.AddEnfermeriaAsync(enfermeria);

      return new GenericResponse<Enfermeria>(true, enfermeria, "Registro de enfermería agregado correctamente.");
    }

    public async Task<GenericResponse<Enfermeria>> UpdateEnfermeriaAsync(EnfermeriaRequestDTO dto)
    {
      if (dto == null || !dto.IdEnfermeria.HasValue)
        return new GenericResponse<Enfermeria>(false, "El registro no es válido.");

      var existingEnfermeria = await _enfermeriaRepository.GetEnfermeriaByIdAsync(dto.IdEnfermeria.Value);
      if (existingEnfermeria == null)
        return new GenericResponse<Enfermeria>(false, "Registro de enfermería no encontrado.");

      // Buscar y actualizar personal técnico asociado
      var existingPersonal = await _context.PersonalTecnicos.FindAsync(existingEnfermeria.IdPersonal);
      if (existingPersonal == null)
        return new GenericResponse<Enfermeria>(false, "El personal asociado no existe.");

      UpdatePersonalTecnico(existingPersonal, dto);
      _context.PersonalTecnicos.Update(existingPersonal);

      UpdateEnfermeriaProperties(existingEnfermeria, dto);
      await _enfermeriaRepository.UpdateEnfermeriaAsync(existingEnfermeria);

      return new GenericResponse<Enfermeria>(true, existingEnfermeria, "Registro actualizado correctamente.");
    }


    public async Task<GenericResponse<Enfermeria>> DeleteEnfermeriaAsync(int id)
    {
      if (id <= 0)
        return new GenericResponse<Enfermeria>(false, "El ID no es válido.");

      var enfermeria = await _enfermeriaRepository.GetEnfermeriaByIdAsync(id);

      if (enfermeria == null)
        return new GenericResponse<Enfermeria>(false, "Registro de enfermería no encontrado.");

      // Buscar el personal técnico asociado
      var personal = await _personalTRepository.GetPersonalTecnicoByIdAsync(enfermeria.IdPersonal);

      if (personal != null)
      {
        await _personalTRepository.DeletePersonalTecnicoAsync(personal.IdPersonal);
      }

      // Eliminar primero el registro de enfermería
      await _enfermeriaRepository.DeleteEnfermeriaAsync(id);

      // Guardar cambios en la base de datos
      await _context.SaveChangesAsync();

      return new GenericResponse<Enfermeria>(true, enfermeria, "Registro de enfermería y personal asociado eliminados correctamente.");
    }


    // ------------------- HELPERS -------------------
    private PersonalTecnico MapToPersonalTecnico(EnfermeriaRequestDTO dto)
    {
      return new PersonalTecnico
      {
        NumeroDni = dto.NumeroDni ?? "",
        Nombre = dto.Nombre ?? "",
        ApellidoPaterno = dto.ApellidoPaterno ?? "",
        ApellidoMaterno = dto.ApellidoMaterno ?? "",
        FechaNacimiento = dto.FechaNacimiento ?? DateTime.MinValue,
        Sexo = dto.Sexo ?? "",
        Direccion = dto.Direccion ?? "",
        Telefono = dto.Telefono ?? "",
        Email = dto.Email ?? "",
        EstadoLaboral = dto.EstadoLaboral ?? "",
        FechaIngreso = dto.FechaIngreso ?? DateTime.Now,
        Turno = dto.Turno ?? "",
        AreaServicio = dto.AreaServicio ?? "",
        Cargo = dto.Cargo ?? ""
      };
    }

    private void UpdatePersonalTecnico(PersonalTecnico personal, EnfermeriaRequestDTO dto)
    {
      if (!string.IsNullOrWhiteSpace(dto.NumeroDni))
        personal.NumeroDni = dto.NumeroDni;

      if (!string.IsNullOrWhiteSpace(dto.Nombre))
        personal.Nombre = dto.Nombre;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoPaterno))
        personal.ApellidoPaterno = dto.ApellidoPaterno;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoMaterno))
        personal.ApellidoMaterno = dto.ApellidoMaterno;

      if (!string.IsNullOrWhiteSpace(dto.Sexo))
        personal.Sexo = dto.Sexo;

      if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento != default)
        personal.FechaNacimiento = dto.FechaNacimiento.Value;

      if (!string.IsNullOrWhiteSpace(dto.Direccion))
        personal.Direccion = dto.Direccion;

      if (!string.IsNullOrWhiteSpace(dto.Telefono))
        personal.Telefono = dto.Telefono;

      if (!string.IsNullOrWhiteSpace(dto.Email))
        personal.Email = dto.Email;

      if (!string.IsNullOrWhiteSpace(dto.EstadoLaboral))
        personal.EstadoLaboral = dto.EstadoLaboral;

      if (dto.FechaIngreso.HasValue && dto.FechaIngreso != default)
        personal.FechaIngreso = dto.FechaIngreso.Value;

      if (!string.IsNullOrWhiteSpace(dto.Turno))
        personal.Turno = dto.Turno;

      if (!string.IsNullOrWhiteSpace(dto.AreaServicio))
        personal.AreaServicio = dto.AreaServicio;

      if (!string.IsNullOrWhiteSpace(dto.Cargo))
        personal.Cargo = dto.Cargo;
    }

    private void UpdateEnfermeriaProperties(Enfermeria enfermeria, EnfermeriaRequestDTO dto)
    {
      if (!string.IsNullOrWhiteSpace(dto.NumeroColegiaturaEnfermeria))
        enfermeria.NumeroColegiaturaEnfermeria = dto.NumeroColegiaturaEnfermeria;

      if (!string.IsNullOrWhiteSpace(dto.NivelProfesional))
        enfermeria.NivelProfesional = dto.NivelProfesional;

      if (dto.IdConsultorio.HasValue)
        enfermeria.IdConsultorio = dto.IdConsultorio.Value;
    }
  }
}