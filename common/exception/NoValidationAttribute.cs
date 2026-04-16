using System;

namespace SGMG.common.exception
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
  public class NoValidationAttribute : Attribute
  {
  }
}
