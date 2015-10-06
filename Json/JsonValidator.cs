using System;
using System.IO;
using SharpE.Json.Data;

namespace SharpE.Json
{
  public class JsonValidator
  {
    public static int Main(string[] args)
    {
      int result = 0;
      if (File.Exists(args[0]))
      {
        JsonException jsonException;
        JsonObject jsonObject = JsonHelperFunctions.Parse(File.ReadAllText(args[0]), out jsonException);
        if (jsonException != null)
        {
          Console.WriteLine(jsonException.Message);
          result = 1;
        }

      }
      Console.ReadKey();
      return result;
    }
  }
}
