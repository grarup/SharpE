using System;

namespace SharpE.Json.Schemas
{
  [Flags]
  public enum SchemaDataType
  {
    Undefined,
    String = 0x01,
    Boolean = 0x02,
    Bool = 0x02,
    Integer = 0x04,
    Number = 0x08,
    Array = 0x10,
    Object = 0x20,
    Nullable = 0x31,
    All = 0x7F
  }
}