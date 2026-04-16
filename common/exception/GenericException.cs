using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.common.exception
{
  public class GenericException : Exception
  {
    public GenericException(Exception innerException) : base(innerException.Message, innerException)
    {
    }

    public GenericException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public GenericException(string message) : base(message)
    {
    }
  }
}