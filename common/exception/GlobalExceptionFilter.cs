using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using SGMG.Dtos.Response;

namespace SGMG.common.exception
{
  /// <summary>
  /// Filtro global para manejar excepciones de base de datos y aplicación
  /// NO maneja errores de validación ni tipos de dato (manejados por middleware y filtros)
  /// </summary>
  public class GlobalExceptionFilter : IExceptionFilter
  {
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
      _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
      var exception = context.Exception;

      // Log del error
      _logger.LogError(exception, "Error en: {Path}", context.HttpContext.Request.Path);

      GenericResponse<object> response;
      int statusCode;

      // Manejo específico según el tipo de excepción
      // NOTA: JsonException NO se maneja aquí - lo maneja JsonValidationMiddleware
      if (exception is DbUpdateException dbUpdateException)
      {
        response = HandleDbUpdateException(dbUpdateException);
        statusCode = StatusCodes.Status400BadRequest;
      }
      else if (exception is SqliteException sqliteException)
      {
        response = HandleSqliteException(sqliteException);
        statusCode = StatusCodes.Status400BadRequest;
      }
      else if (exception is SqlException sqlException)
      {
        response = HandleSqlException(sqlException);
        statusCode = StatusCodes.Status409Conflict;
      }
      else if (exception is GenericException genericException)
      {
        response = HandleGenericException(genericException);
        statusCode = StatusCodes.Status400BadRequest;
      }
      else if (exception is UnauthorizedAccessException)
      {
        response = HandleUnauthorizedException();
        statusCode = StatusCodes.Status401Unauthorized;
      }
      else if (exception is KeyNotFoundException || exception is InvalidOperationException)
      {
        response = HandleNotFoundException(exception);
        statusCode = StatusCodes.Status404NotFound;
      }
      else if (exception is ArgumentException || exception is ArgumentNullException)
      {
        response = HandleArgumentException(exception);
        statusCode = StatusCodes.Status400BadRequest;
      }
      else
      {
        // Manejo genérico para todas las demás excepciones
        response = HandleAllException(exception);
        statusCode = StatusCodes.Status500InternalServerError;
      }

      context.Result = new ObjectResult(response)
      {
        StatusCode = statusCode
      };

      context.ExceptionHandled = true;
    }

    #region Database Exceptions

    /// <summary>
    /// Maneja errores de Entity Framework (incluye FK, unique constraints, etc.)
    /// </summary>
    private GenericResponse<object> HandleDbUpdateException(DbUpdateException ex)
    {
      _logger.LogError(ex, "Error de base de datos: {Message}", ex.Message);

      // Verificar si es un error de SQLite
      if (ex.InnerException is SqliteException sqliteEx)
      {
        return HandleSqliteException(sqliteEx);
      }

      // Verificar si es un error de SQL Server
      if (ex.InnerException is SqlException sqlEx)
      {
        return HandleSqlException(sqlEx);
      }

      // Mensaje genérico para otros errores de BD
      return new GenericResponse<object>
      {
        Success = false,
        Message = "Error al guardar los cambios en la base de datos. Verifique los datos enviados.",
        Data = new
        {
          ErrorType = "DB_UPDATE_ERROR",
          Detail = ex.InnerException?.Message ?? ex.Message
        }
      };
    }

    /// <summary>
    /// Maneja errores específicos de SQLite
    /// </summary>
    private GenericResponse<object> HandleSqliteException(SqliteException ex)
    {
      _logger.LogError(ex, "SQLite Error {Code}: {Message}", ex.SqliteErrorCode, ex.Message);

      string userMessage;
      string errorType;
      object additionalData = null;

      switch (ex.SqliteErrorCode)
      {
        case 19: // SQLITE_CONSTRAINT
          if (ex.Message.Contains("FOREIGN KEY constraint failed"))
          {
            errorType = "FOREIGN_KEY_ERROR";
            userMessage = "No se puede completar la operación porque hace referencia a datos que no existen. Verifica que los IDs relacionados sean correctos.";

            additionalData = new
            {
              ErrorType = errorType,
              Suggestion = "Verifica que los IDs de Personal, Consultorio u otras referencias existan en el sistema."
            };
          }
          else if (ex.Message.Contains("UNIQUE constraint failed"))
          {
            errorType = "UNIQUE_CONSTRAINT_ERROR";
            var campo = ExtractFieldFromConstraint(ex.Message, "UNIQUE constraint failed");

            userMessage = !string.IsNullOrEmpty(campo)
              ? $"Ya existe un registro con este '{campo}'. Por favor, use un valor diferente."
              : "Este registro ya existe en el sistema. No se permiten datos duplicados.";

            additionalData = new
            {
              ErrorType = errorType,
              Campo = campo ?? "Campo duplicado"
            };
          }
          else if (ex.Message.Contains("NOT NULL constraint failed"))
          {
            errorType = "NOT_NULL_ERROR";
            var campo = ExtractFieldFromConstraint(ex.Message, "NOT NULL constraint failed");

            userMessage = !string.IsNullOrEmpty(campo)
              ? $"El campo '{campo}' es obligatorio y no puede estar vacío."
              : "Faltan campos obligatorios. Por favor, complete todos los campos requeridos.";

            additionalData = new
            {
              ErrorType = errorType,
              Campo = campo ?? "Campo obligatorio"
            };
          }
          else if (ex.Message.Contains("CHECK constraint failed"))
          {
            errorType = "CHECK_CONSTRAINT_ERROR";
            userMessage = "Los datos no cumplen con las reglas de validación establecidas.";

            additionalData = new { ErrorType = errorType };
          }
          else
          {
            errorType = "CONSTRAINT_ERROR";
            userMessage = "Los datos ingresados no cumplen con las reglas de validación.";
            additionalData = new { ErrorType = errorType };
          }
          break;

        case 1: // SQLITE_ERROR
          errorType = "SQLITE_GENERAL_ERROR";
          userMessage = "Error al procesar la solicitud. Verifica que los datos estén en el formato correcto.";
          additionalData = new { ErrorType = errorType };
          break;

        case 8: // SQLITE_READONLY
          errorType = "READONLY_ERROR";
          userMessage = "No se pueden realizar cambios en este momento. El sistema está en modo de solo lectura.";
          additionalData = new { ErrorType = errorType };
          break;

        case 13: // SQLITE_FULL
          errorType = "DATABASE_FULL_ERROR";
          userMessage = "No hay espacio disponible en la base de datos. Contacta al administrador.";
          additionalData = new { ErrorType = errorType };
          break;

        case 23: // SQLITE_AUTH
          errorType = "AUTHORIZATION_ERROR";
          userMessage = "No tienes permisos suficientes para realizar esta operación.";
          additionalData = new { ErrorType = errorType };
          break;

        default:
          errorType = "SQLITE_UNKNOWN_ERROR";
          userMessage = "Ocurrió un error al procesar tu solicitud. Por favor, intenta nuevamente.";
          additionalData = new { ErrorType = errorType, ErrorCode = ex.SqliteErrorCode };
          break;
      }

      return new GenericResponse<object>
      {
        Success = false,
        Message = userMessage,
        Data = additionalData
      };
    }

    /// <summary>
    /// Maneja errores de SQL Server
    /// </summary>
    private GenericResponse<object> HandleSqlException(SqlException ex)
    {
      _logger.LogError(ex, "SQL Server Error {Number}: {Message}", ex.Number, ex.Message);

      string userMessage;
      string errorType;
      object additionalData = null;

      switch (ex.Number)
      {
        case 547: // Foreign Key constraint
          errorType = "FOREIGN_KEY_ERROR";
          userMessage = "No se puede completar la operación porque hace referencia a datos que no existen o están siendo utilizados.";
          additionalData = new { ErrorType = errorType };
          break;

        case 2627: // Unique constraint
        case 2601: // Unique index
          errorType = "UNIQUE_CONSTRAINT_ERROR";
          var campo = ExtractFieldFromSqlServerError(ex.Message);
          userMessage = !string.IsNullOrEmpty(campo)
            ? $"Ya existe un registro con este '{campo}'."
            : "Este registro ya existe en el sistema.";
          additionalData = new { ErrorType = errorType, Campo = campo };
          break;

        case 515: // NOT NULL constraint
          errorType = "NOT_NULL_ERROR";
          userMessage = "Faltan campos obligatorios que no pueden estar vacíos.";
          additionalData = new { ErrorType = errorType };
          break;

        case -1: // Connection timeout
        case -2: // Timeout expired
          errorType = "TIMEOUT_ERROR";
          userMessage = "La operación tardó demasiado tiempo. Por favor, intenta nuevamente.";
          additionalData = new { ErrorType = errorType };
          break;

        case 1205: // Deadlock
          errorType = "DEADLOCK_ERROR";
          userMessage = "La operación no pudo completarse debido a conflictos con otras operaciones. Intenta nuevamente.";
          additionalData = new { ErrorType = errorType };
          break;

        default:
          errorType = "SQL_SERVER_ERROR";
          userMessage = "Ocurrió un error en la base de datos. Por favor, intenta nuevamente.";
          additionalData = new { ErrorType = errorType, ErrorNumber = ex.Number };
          break;
      }

      return new GenericResponse<object>
      {
        Success = false,
        Message = userMessage,
        Data = additionalData
      };
    }

    #endregion

    #region Application Exceptions

    /// <summary>
    /// Maneja excepciones personalizadas de la aplicación
    /// </summary>
    private GenericResponse<object> HandleGenericException(GenericException ex)
    {
      _logger.LogWarning(ex, "Excepción de aplicación: {Message}", ex.Message);

      return new GenericResponse<object>
      {
        Success = false,
        Message = ex.Message,
        Data = new { ErrorType = "APPLICATION_ERROR" }
      };
    }

    /// <summary>
    /// Maneja errores de autorización
    /// </summary>
    private GenericResponse<object> HandleUnauthorizedException()
    {
      return new GenericResponse<object>
      {
        Success = false,
        Message = "No tienes autorización para realizar esta operación.",
        Data = new { ErrorType = "UNAUTHORIZED" }
      };
    }

    /// <summary>
    /// Maneja errores de recurso no encontrado
    /// </summary>
    private GenericResponse<object> HandleNotFoundException(Exception ex)
    {
      _logger.LogWarning(ex, "Recurso no encontrado: {Message}", ex.Message);

      return new GenericResponse<object>
      {
        Success = false,
        Message = "El recurso solicitado no fue encontrado.",
        Data = new { ErrorType = "NOT_FOUND", Detail = ex.Message }
      };
    }

    /// <summary>
    /// Maneja errores de argumentos inválidos
    /// </summary>
    private GenericResponse<object> HandleArgumentException(Exception ex)
    {
      _logger.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);

      return new GenericResponse<object>
      {
        Success = false,
        Message = "Los datos proporcionados son inválidos.",
        Data = new { ErrorType = "INVALID_ARGUMENT", Detail = ex.Message }
      };
    }

    /// <summary>
    /// Manejo genérico para todas las demás excepciones
    /// </summary>
    private GenericResponse<object> HandleAllException(Exception ex)
    {
      _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);

      return new GenericResponse<object>
      {
        Success = false,
        Message = "Ocurrió un error inesperado. Por favor, intenta nuevamente o contacta al soporte técnico.",
        Data = new { ErrorType = "UNEXPECTED_ERROR" }
      };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extrae el nombre del campo de un mensaje de constraint de SQLite
    /// </summary>
    private string ExtractFieldFromConstraint(string message, string constraintType)
    {
      try
      {
        var pattern = $@"{constraintType}: \w+\.(\w+)";
        var match = Regex.Match(message, pattern);

        if (match.Success)
        {
          return FormatFieldName(match.Groups[1].Value);
        }
      }
      catch
      {
        // Ignorar errores de parsing
      }

      return null;
    }

    /// <summary>
    /// Extrae el nombre del campo de un mensaje de error de SQL Server
    /// </summary>
    private string ExtractFieldFromSqlServerError(string message)
    {
      try
      {
        // Buscar patrones como "column 'FieldName'" o "'FieldName'"
        var patterns = new[]
        {
          @"column '(\w+)'",
          @"key '(\w+)'",
          @"'(\w+)'"
        };

        foreach (var pattern in patterns)
        {
          var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
          if (match.Success)
          {
            return FormatFieldName(match.Groups[1].Value);
          }
        }
      }
      catch
      {
        // Ignorar errores de parsing
      }

      return null;
    }

    /// <summary>
    /// Formatea el nombre del campo a un formato legible
    /// </summary>
    private string FormatFieldName(string fieldName)
    {
      if (string.IsNullOrEmpty(fieldName))
        return fieldName;

      // Convertir de PascalCase/camelCase a formato legible
      fieldName = Regex.Replace(fieldName, "([a-z])([A-Z])", "$1 $2");

      // Capitalizar primera letra
      if (fieldName.Length > 0)
      {
        fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
      }

      return fieldName;
    }

    #endregion
  }
}