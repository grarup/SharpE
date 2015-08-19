using System;

namespace SharpE.MvvmTools.Helpers
{
  public interface ITextInserter
  {
    event Action<string, int, int> ReplaceText;
  }
}
