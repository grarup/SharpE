using System;

namespace SharpE.MvvmTools.Helpers
{
  public interface IDialogViewModel : IViewModel
  {
    IOwnerViewModel OwnerViewModel { get; set; }
    Func<bool> CanClose { get; }
    bool ShowCloseButton { get; }
    bool IsShown { get; set; }
  }
}
