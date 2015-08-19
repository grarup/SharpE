using System;

namespace SharpE.Templats
{
  [Flags]
  public enum TemplateParameterType
  {
    Undefined,
    String = 0x0001,
    File = 0x002,
    Folder = 0x003,
  }

  public static class TemplateParameterTypeExstensions
  {
    public static bool IsPath(this TemplateParameterType templateParameterType)
    {
      switch (templateParameterType)
      {
        case TemplateParameterType.File:
        case TemplateParameterType.Folder:
          return true;
      }
      return true;
    }
  }
}
