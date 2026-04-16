using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Response;

namespace SGMG.common.exception
{
  public class ValidationFilter : IActionFilter
  {
    private readonly ILogger<ValidationFilter> _logger;

    public ValidationFilter(ILogger<ValidationFilter> logger)
    {
      _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
      var cad = context.ActionDescriptor as ControllerActionDescriptor;



      var method = context.HttpContext.Request.Method;
      if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogDebug("🚫 No se aplica validación en peticiones {Method}.", method);
        return;
      }

      if (!context.ModelState.IsValid)
      {
        _logger.LogWarning("⚠️ Errores de validación detectados en {Controller}.{Action}.", cad?.ControllerName, cad?.ActionName);

        var errors = new List<object>();

        foreach (var entry in context.ModelState)
        {
          if (IsRootObjectError(entry.Key))
            continue;

          if (entry.Value?.Errors.Count > 0)
          {
            var fieldErrors = new List<string>();

            foreach (var error in entry.Value.Errors)
            {
              var friendlyMessage = GetFriendlyErrorMessage(error, entry.Key);
              if (!string.IsNullOrEmpty(friendlyMessage))
              {
                fieldErrors.Add(friendlyMessage);
              }
            }

            if (fieldErrors.Count > 0)
            {
              errors.Add(new
              {
                Field = CleanFieldName(entry.Key),
                Errors = fieldErrors.ToArray()
              });
            }
          }
        }

        _logger.LogInformation("📦 Campos con errores: {ErrorsJson}", JsonSerializer.Serialize(errors));

        var response = new GenericResponse<object>
        {
          Success = false,
          Message = "Errores de validación en la solicitud.",
          Data = errors
        };

        context.Result = new BadRequestObjectResult(response);

        _logger.LogWarning("🚫 Petición detenida por errores de validación. Se devuelve HTTP 400.");
      }
      else
      {
        _logger.LogInformation("✅ Validación completada sin errores.");
      }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
      // Log final opcional
      _logger.LogDebug("🏁 Finalizó la ejecución del filtro de validación para la acción.");
    }

    private bool IsRootObjectError(string fieldName)
    {
      return !fieldName.Contains(".") && !fieldName.StartsWith("$") &&
             (fieldName.EndsWith("DTO") || fieldName.EndsWith("Request") ||
              fieldName.EndsWith("RequestDTO") || char.IsLower(fieldName[0]));
    }

    private string CleanFieldName(string fieldName)
    {
      if (fieldName.StartsWith("$."))
      {
        fieldName = fieldName.Substring(2);
      }

      if (fieldName.Contains("."))
      {
        var parts = fieldName.Split('.');
        fieldName = parts[parts.Length - 1];
      }

      return fieldName;
    }

    private string GetFriendlyErrorMessage(ModelError error, string fieldName)
    {
      var cleanFieldName = CleanFieldName(fieldName);

      if (!string.IsNullOrEmpty(error.ErrorMessage))
      {
        if (!error.ErrorMessage.Contains("could not be converted") &&
            !error.ErrorMessage.Contains("JSON value") &&
            !error.ErrorMessage.Contains("Path:") &&
            !error.ErrorMessage.Contains("LineNumber:") &&
            !error.ErrorMessage.Contains("BytePositionInLine:"))
        {
          if (error.ErrorMessage.Contains("field is required") ||
              error.ErrorMessage.Contains("is required"))
          {
            return $"El campo '{cleanFieldName}' es obligatorio.";
          }

          return error.ErrorMessage;
        }

        if (error.ErrorMessage.Contains("could not be converted") ||
            error.ErrorMessage.Contains("JSON value"))
        {
          return ParseJsonConversionError(error.ErrorMessage, cleanFieldName);
        }
      }

      if (error.Exception != null)
      {
        _logger.LogError(error.Exception, "❌ Excepción durante la validación del campo {Field}.", cleanFieldName);
        return GetTypeConversionError(error.Exception, cleanFieldName);
      }

      return $"El campo '{cleanFieldName}' no es válido.";
    }

    private string GetTypeConversionError(Exception exception, string fieldName)
    {
      var exceptionMessage = exception.Message.ToLower();

      if (exceptionMessage.Contains("boolean"))
      {
        return $"El campo '{fieldName}' debe ser un valor booleano (true/false).";
      }

      if (exceptionMessage.Contains("int32") || exceptionMessage.Contains("int64"))
      {
        return $"El campo '{fieldName}' debe ser un número entero válido.";
      }

      if (exceptionMessage.Contains("decimal") || exceptionMessage.Contains("double") ||
          exceptionMessage.Contains("single"))
      {
        return $"El campo '{fieldName}' debe ser un número decimal válido.";
      }

      if (exceptionMessage.Contains("datetime"))
      {
        return $"El campo '{fieldName}' debe ser una fecha válida (formato: YYYY-MM-DD).";
      }

      if (exceptionMessage.Contains("guid"))
      {
        return $"El campo '{fieldName}' debe ser un identificador único (GUID) válido.";
      }

      return $"El campo '{fieldName}' tiene un formato incorrecto.";
    }

    private string ParseJsonConversionError(string errorMessage, string fieldName)
    {
      var lowerMessage = errorMessage.ToLower();

      if (lowerMessage.Contains("system.int32") || lowerMessage.Contains("nullable`1[system.int32]"))
      {
        return $"El campo '{fieldName}' debe ser un número entero válido.";
      }

      if (lowerMessage.Contains("system.int64") || lowerMessage.Contains("nullable`1[system.int64]"))
      {
        return $"El campo '{fieldName}' debe ser un número entero válido.";
      }

      if (lowerMessage.Contains("system.decimal") || lowerMessage.Contains("nullable`1[system.decimal]"))
      {
        return $"El campo '{fieldName}' debe ser un número decimal válido.";
      }

      if (lowerMessage.Contains("system.double") || lowerMessage.Contains("nullable`1[system.double]"))
      {
        return $"El campo '{fieldName}' debe ser un número decimal válido.";
      }

      if (lowerMessage.Contains("system.boolean") || lowerMessage.Contains("nullable`1[system.boolean]"))
      {
        return $"El campo '{fieldName}' debe ser un valor booleano (true/false).";
      }

      if (lowerMessage.Contains("system.datetime") || lowerMessage.Contains("nullable`1[system.datetime]"))
      {
        return $"El campo '{fieldName}' debe ser una fecha válida (formato: YYYY-MM-DD o YYYY-MM-DDTHH:mm:ss).";
      }

      if (lowerMessage.Contains("system.guid") || lowerMessage.Contains("nullable`1[system.guid]"))
      {
        return $"El campo '{fieldName}' debe ser un identificador único (GUID) válido.";
      }

      if (lowerMessage.Contains("system.string"))
      {
        return $"El campo '{fieldName}' debe ser una cadena de texto válida.";
      }

      return $"El campo '{fieldName}' tiene un formato de dato incorrecto.";
    }
  }
}
