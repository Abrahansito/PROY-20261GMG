using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Response;

namespace SGMG.common.middleware
{
  /// <summary>
  /// Middleware para pre-validar JSON contra el tipo de DTO esperado y capturar múltiples errores
  /// </summary>
  public class JsonValidationMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<JsonValidationMiddleware> _logger;

    public JsonValidationMiddleware(RequestDelegate next, ILogger<JsonValidationMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      // Solo procesar POST, PUT, PATCH con JSON
      if (context.Request.ContentType?.Contains("application/json") == true &&
          (context.Request.Method == "POST" ||
           context.Request.Method == "PUT" ||
           context.Request.Method == "PATCH"))
      {
        _logger.LogInformation("📥 Procesando solicitud {Method} con contenido JSON en {Path}",
          context.Request.Method, context.Request.Path);

        // Obtener el tipo esperado del parámetro del action
        var expectedType = GetExpectedBodyType(context);

        if (expectedType != null)
        {
          _logger.LogInformation("📘 Se detectó tipo esperado para el cuerpo: {ExpectedType}", expectedType.Name);

          context.Request.EnableBuffering();

          using var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

          var body = await reader.ReadToEndAsync();
          context.Request.Body.Position = 0;

          if (string.IsNullOrWhiteSpace(body))
          {
            _logger.LogWarning("⚠️ Cuerpo de la solicitud vacío en {Path}", context.Request.Path);
          }

          // Validar el JSON contra el tipo esperado
          var errors = ValidateJsonAgainstType(body, expectedType);

          if (errors.Any())
          {
            _logger.LogWarning("❌ Se encontraron {Count} errores de validación JSON en {Path}", errors.Count, context.Request.Path);
            _logger.LogDebug("📄 Detalles de errores: {Errors}", JsonSerializer.Serialize(errors));

            var response = new GenericResponse<object>
            {
              Success = false,
              Message = "Errores de tipo de dato en la solicitud.",
              Data = errors
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(response);
            return;
          }
          else
          {
            _logger.LogInformation("✅ Validación JSON exitosa para {TypeName}", expectedType.Name);
          }
        }
        else
        {
          _logger.LogDebug("ℹ️ No se detectó tipo de cuerpo esperado para la ruta {Path}.", context.Request.Path);
        }
      }

      await _next(context);
    }

    /// <summary>
    /// Obtiene el tipo esperado del body del endpoint actual
    /// </summary>
    private Type GetExpectedBodyType(HttpContext context)
    {
      try
      {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return null;

        var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null) return null;

        var bodyParameter = actionDescriptor.Parameters
          .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

        return bodyParameter?.ParameterType;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "❗ Error obteniendo el tipo esperado del cuerpo.");
        return null;
      }
    }

    /// <summary>
    /// Valida el JSON contra el tipo esperado y retorna todos los errores encontrados
    /// </summary>
    private List<object> ValidateJsonAgainstType(string jsonBody, Type expectedType)
    {
      var errors = new Dictionary<string, List<string>>();

      try
      {
        using var document = JsonDocument.Parse(jsonBody);
        var root = document.RootElement;

        var properties = expectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in root.EnumerateObject())
        {
          var propertyInfo = properties.FirstOrDefault(p =>
            string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase));

          if (propertyInfo != null)
          {
            ValidateProperty(property.Value, property.Name, propertyInfo.PropertyType, errors);
          }
          else
          {
            _logger.LogDebug("⚠️ Propiedad '{Property}' no existe en el tipo {ExpectedType}.", property.Name, expectedType.Name);
          }
        }
      }
      catch (JsonException ex)
      {
        _logger.LogError(ex, "❌ Error al parsear el JSON recibido.");
        errors["request"] = new List<string> { "El formato JSON es inválido." };
      }

      return errors.Select(e => new
      {
        Field = e.Key,
        Errors = e.Value.ToArray()
      }).Cast<object>().ToList();
    }

    /// <summary>
    /// Valida una propiedad contra su tipo esperado
    /// </summary>
    private void ValidateProperty(JsonElement element, string propertyName, Type expectedType, Dictionary<string, List<string>> errors)
    {
      var underlyingType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

      if (element.ValueKind == JsonValueKind.Null)
        return;

      // 🔹 Mover la validación de listas antes de objetos
      if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>))
      {
        if (element.ValueKind != JsonValueKind.Array)
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser una lista válida.");
        }
        else
        {
          var itemType = underlyingType.GetGenericArguments()[0];
          int index = 0;
          foreach (var item in element.EnumerateArray())
          {
            _logger.LogDebug("🔍 Validando elemento {Property}[{Index}] de tipo {ItemType}", propertyName, index, itemType.Name);
            ValidateProperty(item, $"{propertyName}[{index}]", itemType, errors);
            index++;
          }
        }
      }
      else if (underlyingType == typeof(int) || underlyingType == typeof(long))
      {
        if (element.ValueKind != JsonValueKind.Number || !TryParseInteger(element))
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un número entero válido.");
          _logger.LogDebug("🧩 Error de tipo en campo {Field}: esperado entero.", propertyName);
        }
      }
      else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
      {
        if (element.ValueKind != JsonValueKind.Number)
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un número decimal válido.");
        }
      }
      else if (underlyingType == typeof(bool))
      {
        if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un valor booleano (true/false).");
        }
      }
      else if (underlyingType == typeof(DateTime))
      {
        if (element.ValueKind != JsonValueKind.String || !element.TryGetDateTime(out _))
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser una fecha válida (YYYY-MM-DD o YYYY-MM-DDTHH:mm:ss).");
        }
      }
      else if (underlyingType == typeof(Guid))
      {
        if (element.ValueKind != JsonValueKind.String || !element.TryGetGuid(out _))
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un identificador único (GUID) válido.");
        }
      }
      else if (underlyingType == typeof(string))
      {
        if (element.ValueKind != JsonValueKind.String)
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser una cadena de texto válida.");
        }
      }
      else if (underlyingType.IsEnum)
      {
        if (element.ValueKind == JsonValueKind.String)
        {
          var stringValue = element.GetString();
          if (!Enum.IsDefined(underlyingType, stringValue) && !int.TryParse(stringValue, out _))
          {
            var validValues = string.Join(", ", Enum.GetNames(underlyingType));
            AddError(errors, propertyName, $"El campo '{propertyName}' debe ser uno de los siguientes valores: {validValues}.");
          }
        }
        else if (element.ValueKind == JsonValueKind.Number)
        {
          if (!element.TryGetInt32(out var intValue) || !Enum.IsDefined(underlyingType, intValue))
          {
            var validValues = string.Join(", ", Enum.GetNames(underlyingType));
            AddError(errors, propertyName, $"El campo '{propertyName}' debe ser uno de los siguientes valores: {validValues}.");
          }
        }
        else
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un valor de enumeración válido.");
        }
      }
      else if (underlyingType.IsClass && underlyingType != typeof(string))
      {
        if (element.ValueKind == JsonValueKind.Object)
        {
          var nestedProperties = underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
          foreach (var nestedProp in element.EnumerateObject())
          {
            var nestedPropertyInfo = nestedProperties.FirstOrDefault(p =>
              string.Equals(p.Name, nestedProp.Name, StringComparison.OrdinalIgnoreCase));

            if (nestedPropertyInfo != null)
            {
              ValidateProperty(nestedProp.Value, $"{propertyName}.{nestedProp.Name}", nestedPropertyInfo.PropertyType, errors);
            }
          }
        }
        else
        {
          AddError(errors, propertyName, $"El campo '{propertyName}' debe ser un objeto válido.");
        }
      }
    }

    private bool TryParseInteger(JsonElement element)
    {
      return element.TryGetInt32(out _) || element.TryGetInt64(out _);
    }

    private void AddError(Dictionary<string, List<string>> errors, string fieldName, string errorMessage)
    {
      var cleanFieldName = fieldName.Split('.').Last();

      if (!errors.ContainsKey(cleanFieldName))
      {
        errors[cleanFieldName] = new List<string>();
      }

      if (!errors[cleanFieldName].Contains(errorMessage))
      {
        errors[cleanFieldName].Add(errorMessage);
      }
    }
  }
}
