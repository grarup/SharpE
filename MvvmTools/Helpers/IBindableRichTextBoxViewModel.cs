using System;
using System.Windows.Documents;

namespace SharpE.MvvmTools.Helpers
{
  public interface IBindableRichTextBoxViewModel
  {
    FlowDocument Document { get; }
    Action TextChanged { get; }
    int CursorPosition { get; set; }
  }
}
