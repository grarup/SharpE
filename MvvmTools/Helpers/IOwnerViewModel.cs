namespace SharpE.MvvmTools.Helpers
{
  public interface IOwnerViewModel : IViewModel
  {
    MessageBoxViewModel MessageBoxViewModel { get; }
    DialogViewModel DialogViewModel { get; }
  }
}
