using System.Collections.Generic;
using System.Linq;

namespace SharpE.MvvmTools.Exstensions
{
  public static class StringExstensions
  {
    public static int FirstIndexOfAny(this string text, IList<char> characters, int startOffset = 0, int? length = null)
    {
      if (startOffset >= text.Length)
        return -1;
      if (length == null)
        length = text.Length;
      else if (startOffset + length >= text.Length)
        length = text.Length - startOffset;
      for (int i = startOffset; i < startOffset + length; i++)
      {
        if (characters.Contains(text[i]))
          return i;
      }
      return -1;
    }

    public static int LastIndexOfAny(this string text, IList<char> characters, int startOffset = 0, int? length = null)
    {
      if (startOffset >= text.Length)
        return -1;
      if (length == null)
        length = startOffset;
      else if (startOffset - length < 0)
        length = startOffset;
      for (int i = startOffset; i > startOffset - length; i--)
      {
        if (characters.Contains(text[i]))
          return i;
      }
      return -1;
    }

    public static string StripIndent(this string text)
    {
      return text.Replace("\t", "").Replace("\r", "").Replace("\n", "");
    }
  }
}
