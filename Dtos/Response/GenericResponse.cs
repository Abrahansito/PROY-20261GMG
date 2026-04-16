using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SGMG.Dtos.Response
{
  public class GenericResponse<T>
  {
    public bool? Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }

    // Constructor vacío
    public GenericResponse() { }

    // Constructor solo con éxito y mensaje
    public GenericResponse(bool success, string message)
    {
      Success = success;
      Message = message;
    }

    // Constructor con éxito, datos y mensaje
    public GenericResponse(bool success, T data, string message)
    {
      Success = success;
      Data = data;
      Message = message;
    }

    // Constructor solo con datos (success = true por defecto)
    public GenericResponse(T data)
    {
      Data = data;
    }
  }
}