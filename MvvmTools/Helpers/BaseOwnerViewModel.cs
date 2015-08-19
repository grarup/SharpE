using System.Resources;

namespace SharpE.MvvmTools.Helpers
{
  public class BaseOwnerViewModel : BaseViewModel, IOwnerViewModel
  {
    private readonly MessageBoxViewModel m_messageBoxViewModel;
    private readonly DialogViewModel m_dialogViewModel = new DialogViewModel();

    public BaseOwnerViewModel(ResourceManager resourceManager = null)
    {
      m_messageBoxViewModel = new MessageBoxViewModel(resourceManager);
    }

    public MessageBoxViewModel MessageBoxViewModel
    {
      get { return m_messageBoxViewModel; }
    }

    public DialogViewModel DialogViewModel
    {
      get { return m_dialogViewModel; }
    }
  }
}
