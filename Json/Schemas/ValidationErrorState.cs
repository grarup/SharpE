using System;

namespace SharpE.Json.Schemas
{
  [Flags]
  public enum ValidationErrorState
  {
    Good = 0x00,
    NotInSchema = 0x01,
    WrongData = 0x02,
    NotCorrectJson = 0x04,
    Unknown = 0x08,
    ToMany = 0x10,
    MissingChild = 0x20,
  }
}