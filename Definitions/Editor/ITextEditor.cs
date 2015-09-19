using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpE.Definitions.Editor
{
  public interface ITextEditor : IEditor
  {
    void JumpToLine(int lineNumber);
  }
}
