using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Services;
using SGMG.common.exception;
using SGMG.Data;
using SGMG.Repository;
using SGMG.Dtos.Request.PersonalTecnico;
using SGMG.Repository.RepositoryImpl;

namespace SGMG.Services.ServiceImpl
{
  public class PersonalTServiceImpl : IPersonalTservice
  {
    private readonly IPersonalTRepository _personalTecnicoRepository;
    private readonly ILogger<PersonalTServiceImpl> _logger;

    public PersonalTServiceImpl(IPersonalTRepository personalTecnicoRepository, ILogger<PersonalTServiceImpl> logger)
    {
      _personalTecnicoRepository = personalTecnicoRepository;
      _logger = logger;
    }

    public async Task<GenericResponse<PersonalTecnico>> GetPersonalTecnicoByIdAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<PersonalTecnico>(false, "El ID no es válido.");
        var personalTecnico = await _personalTecnicoRepository.GetPersonalTecnicoByIdAsync(id);
        if (personalTecnico == null)
          return new GenericResponse<PersonalTecnico>(false, "Registro de personal técnico no encontrado.");
        if (!personalTecnico.Cargo.Contains("ADMINISTRADOR") && !personalTecnico.Cargo.Contains("CAJERO") && !personalTecnico.Cargo.Contains("ADMISION"))
          return new GenericResponse<PersonalTecnico>(false, "El registro no corresponde a un personal técnico.");
        return new GenericResponse<PersonalTecnico>(true, personalTecnico, "Registro de personal técnico obtenido correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al obtener el registro de personal técnico: ", ex);
      }
    }

    public async Task<GenericResponse<PersonalTecnico>> AddPersonalTecnicoAsync(PersonalTecnicoRequestDTO personalTecnicoRequestDTO)
    {
      try
      {
        if (personalTecnicoRequestDTO == null)
          return new GenericResponse<PersonalTecnico>(false, "El registro de personal técnico no puede ser nulo.");
        var personalTecnico = MapToPersonalTecnico(personalTecnicoRequestDTO);
        await _personalTecnicoRepository.AddPersonalTecnicoAsync(personalTecnico);
        return new GenericResponse<PersonalTecnico>(true, personalTecnico, "Registro de personal técnico agregado correctamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<PersonalTecnico> { Success = false, Message = "Error al agregar el registro de personal técnico: " + ex.Message, Data = null };
      }
    }

    public async Task<GenericResponse<PersonalTecnico>> UpdatePersonalTecnicoAsync(PersonalTecnicoRequestDTO personalTecnicoRequestDTO)
    {
      try
      {
        _logger.LogInformation("Iniciando actualización de PersonalTecnico con ID: {Id}", personalTecnicoRequestDTO?.IdPersonalT);

        if (personalTecnicoRequestDTO == null || personalTecnicoRequestDTO.IdPersonalT <= 0)
        {
          _logger.LogWarning("Solicitud inválida para actualización de PersonalTecnico.");
          return new GenericResponse<PersonalTecnico> { Success = false, Message = "El registro de personal técnico no es válido.", Data = null };
        }

        var personalTecnico = MapToPersonalTecnico(personalTecnicoRequestDTO);
        _logger.LogInformation("Datos mapeados para PersonalTecnico con ID: {Id}", personalTecnico.IdPersonal);
        var existingPersonalTecnico = await _personalTecnicoRepository.GetPersonalTecnicoByIdAsync(personalTecnico.IdPersonal);

        if (existingPersonalTecnico == null)
        {
          _logger.LogWarning("No se encontró el registro de PersonalTecnico con ID: {Id}", personalTecnico.IdPersonal);
          return new GenericResponse<PersonalTecnico> { Success = false, Message = "Registro de personal técnico no encontrado.", Data = null };
        }

        // Actualizar propiedades
        UpdatePersonalTecnicoProperties(existingPersonalTecnico, personalTecnico);

        await _personalTecnicoRepository.UpdatePersonalTecnicoAsync(existingPersonalTecnico);

        _logger.LogInformation("Actualización exitosa de PersonalTecnico con ID: {Id}", existingPersonalTecnico.IdPersonal);

        return new GenericResponse<PersonalTecnico> { Success = true, Message = "Registro de personal técnico actualizado correctamente.", Data = existingPersonalTecnico };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al actualizar el registro de PersonalTecnico con ID: {Id}", personalTecnicoRequestDTO?.IdPersonalT);
        return new GenericResponse<PersonalTecnico> { Success = false, Message = "Error al actualizar el registro de personal técnico: " + ex.Message, Data = null };
      }
    }

    public async Task<GenericResponse<PersonalTecnico>> DeletePersonalTecnicoAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<PersonalTecnico>(false, "El ID no es válido.");

        var personalTecnico = await _personalTecnicoRepository.GetPersonalTecnicoByIdAsync(id);
        if (personalTecnico == null)
          return new GenericResponse<PersonalTecnico>(false, "Registro de personal técnico no encontrado.");

        // Validar el cargo antes de eliminar
        if (!personalTecnico.Cargo.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) &&
            !personalTecnico.Cargo.Equals("CAJERO", StringComparison.OrdinalIgnoreCase))
        {
          return new GenericResponse<PersonalTecnico>(false, "Solo se pueden eliminar registros de personal con cargo ADMINISTRADOR o CAJERO.");
        }

        await _personalTecnicoRepository.DeletePersonalTecnicoAsync(id);

        return new GenericResponse<PersonalTecnico>(true, personalTecnico, "Registro de personal técnico eliminado correctamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<PersonalTecnico>(false, $"Error al eliminar el registro de personal técnico con ID {id}: {ex.Message}");
      }
    }


    private PersonalTecnico MapToPersonalTecnico(PersonalTecnicoRequestDTO dto)
    {
      var personalTecnico = new PersonalTecnico();
      if (dto.IdPersonalT.HasValue)
        personalTecnico.IdPersonal = dto.IdPersonalT.Value;
      if (dto.NumeroDni != null)
        personalTecnico.NumeroDni = dto.NumeroDni;
      if (dto.Nombre != null)
        personalTecnico.Nombre = dto.Nombre;
      if (dto.ApellidoPaterno != null)
        personalTecnico.ApellidoPaterno = dto.ApellidoPaterno;
      if (dto.ApellidoMaterno != null)
        personalTecnico.ApellidoMaterno = dto.ApellidoMaterno;
      if (dto.FechaNacimiento.HasValue)
        personalTecnico.FechaNacimiento = dto.FechaNacimiento.Value;
      if (dto.Sexo != null)
        personalTecnico.Sexo = dto.Sexo;
      if (dto.Direccion != null)
        personalTecnico.Direccion = dto.Direccion;
      if (dto.Telefono != null)
        personalTecnico.Telefono = dto.Telefono;
      if (dto.Email != null)
        personalTecnico.Email = dto.Email;
      if (dto.EstadoLaboral != null)
        personalTecnico.EstadoLaboral = dto.EstadoLaboral;
      if (dto.FechaIngreso.HasValue)
        personalTecnico.FechaIngreso = dto.FechaIngreso.Value;
      if (dto.Turno != null)
        personalTecnico.Turno = dto.Turno;
      if (dto.AreaServicio != null)
        personalTecnico.AreaServicio = dto.AreaServicio;
      if (dto.Cargo != null)
        personalTecnico.Cargo = dto.Cargo;

      return personalTecnico;
    }

    private void UpdatePersonalTecnicoProperties(PersonalTecnico existing, PersonalTecnico updated)
    {
      if (updated.IdPersonal > 0)
        existing.IdPersonal = updated.IdPersonal;
      if (!string.IsNullOrEmpty(updated.NumeroDni))
        existing.NumeroDni = updated.NumeroDni;
      if (!string.IsNullOrEmpty(updated.Nombre))
        existing.Nombre = updated.Nombre;
      if (!string.IsNullOrEmpty(updated.ApellidoPaterno))
        existing.ApellidoPaterno = updated.ApellidoPaterno;
      if (!string.IsNullOrEmpty(updated.ApellidoMaterno))
        existing.ApellidoMaterno = updated.ApellidoMaterno;
      if (updated.FechaNacimiento != default(DateTime))
        existing.FechaNacimiento = updated.FechaNacimiento;
      if (!string.IsNullOrEmpty(updated.Sexo))
        existing.Sexo = updated.Sexo;
      if (!string.IsNullOrEmpty(updated.Direccion))
        existing.Direccion = updated.Direccion;
      if (!string.IsNullOrEmpty(updated.Telefono))
        existing.Telefono = updated.Telefono;
      if (!string.IsNullOrEmpty(updated.Email))
        existing.Email = updated.Email;
      if (!string.IsNullOrEmpty(updated.EstadoLaboral))
        existing.EstadoLaboral = updated.EstadoLaboral;
      if (updated.FechaIngreso != default(DateTime))
        existing.FechaIngreso = updated.FechaIngreso;
      if (!string.IsNullOrEmpty(updated.Turno))
        existing.Turno = updated.Turno;
      if (!string.IsNullOrEmpty(updated.AreaServicio))
        existing.AreaServicio = updated.AreaServicio;
      if (!string.IsNullOrEmpty(updated.Cargo))
        existing.Cargo = updated.Cargo;
    }
  }
}