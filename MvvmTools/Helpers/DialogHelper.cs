using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SharpE.MvvmTools.Helpers
{
  public class DialogHelper : IDialogHelper
  {
    #region declares
    private readonly List<FrameworkElement> m_views = new List<FrameworkElement>();
    private readonly IOwnerViewModel m_rootView;
    #endregion

    #region constructor
    public DialogHelper(IOwnerViewModel rootView)
    {
      m_rootView = rootView;
      Register(rootView.View);
    }
    #endregion

    #region private methods
    private void Register(FrameworkElement view)
    {
      Window owner = view as Window ?? Window.GetWindow(view);
      if (owner == null)
      {
        view.Loaded += ViewOnLoaded;
        return;
      }

      owner.Closed += OwnerOnClosed;
      m_views.Add(view);
    }

    private void OwnerOnClosed(object sender, EventArgs eventArgs)
    {
      Window owner = sender as Window;
      if (owner == null) return;
      List<FrameworkElement> frameworkElementsToClose = m_views.Where(n => Equals(Window.GetWindow(n), owner)).ToList();
      foreach (FrameworkElement frameworkElement in frameworkElementsToClose)
      {
        WindowClosed(frameworkElement.DataContext as IWindowViewModel);
        Unregister(frameworkElement);
      }
      owner.DataContext = null;
    }

    private void ViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      FrameworkElement view = sender as FrameworkElement;
      if (view == null) return;
      view.Loaded -= ViewOnLoaded;
      Register(view);
    }

    private void Unregister(FrameworkElement view)
    {
      m_views.Remove(view);
    }

    private Window FindOwner(object ownerViewModel)
    {
      FrameworkElement view = m_views.FirstOrDefault(n => ReferenceEquals(n.DataContext, ownerViewModel));
      if (view == null)
        throw new ArgumentException("The viewModel does not have a registret view.");

      Window owner = view as Window ?? Window.GetWindow(view);

      if (owner == null)
        throw new InvalidOperationException("The view does not belone to any window");

      return owner;
    }
    #endregion

    #region public methods
    public bool? ShowDialogInWindow(IViewModel viewModel, IViewModel ownerViewModel = null)
    {
      if (viewModel.View == null)
        throw new ArgumentException("the view can not be null");
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;

      Window dialog = viewModel.View as Window;
      if (dialog == null)
        throw new ArgumentException("viewType must be of type Window");
      if (ownerViewModel != null)
        dialog.Owner = FindOwner(ownerViewModel);
      dialog.DataContext = viewModel;
      Register(dialog);
      return dialog.ShowDialog();
    }

    public void ShowDialog(IDialogViewModel viewModel, IOwnerViewModel ownerViewModel = null)
    {
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;
      ownerViewModel.DialogViewModel.Show(viewModel);
    }

    public void ShowWindow(IWindowViewModel viewModel, Rect? rect = null)
    {
      if (!m_rootView.View.Dispatcher.CheckAccess())
      {
        m_rootView.View.Dispatcher.Invoke(() => ShowWindow(viewModel, rect));
        return;
      }
      Window dialog = Activator.CreateInstance(viewModel.WindowsType) as Window;
      if (dialog == null)
        throw new ArgumentException("viewType must be of type Window");
      dialog.DataContext = viewModel;
      Register(dialog);
      if (rect != null)
        SetSizeAndPosition(viewModel, (Rect)rect);
      dialog.Show();
    }

    public void ShowMessageBox(Action<MessageBoxResult> resultAction, string title,
                               string text, MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                               UIElement icon = null, IOwnerViewModel overViewModel = null)
    {
      ShowMessageBox(resultAction == null ? (Action<MessageBoxResult, object[]>)null : (result, objects) => resultAction(result), null, title, text, messageBoxButton, icon, overViewModel);
    }

    public void ShowMessageBox(Action<MessageBoxResult, object[]> resultAction, object[] values, string title, string text, MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                                           UIElement icon = null, IOwnerViewModel ownerViewModel = null)
    {
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;
      if (ownerViewModel.MessageBoxViewModel.IsShown)
        throw new Exception("Only one messagebox at a time.");
      ownerViewModel.MessageBoxViewModel.Title = title;
      ownerViewModel.MessageBoxViewModel.Text = text;
      ownerViewModel.MessageBoxViewModel.Button = messageBoxButton;
      ownerViewModel.MessageBoxViewModel.ResultFunction = resultAction;
      ownerViewModel.MessageBoxViewModel.Icon = icon;
      ownerViewModel.MessageBoxViewModel.Values = values;
      ownerViewModel.MessageBoxViewModel.IsShown = true;
    }

    public async Task<MessageBoxResult> ShowMessageBox(string title, string text,
                                                       MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                                                       UIElement icon = null,
                                                       IOwnerViewModel ownerViewModel = null)
    {
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;
      MessageBoxResult messageBoxResult = MessageBoxResult.None;
      ShowMessageBox(result => messageBoxResult = result, title, text, messageBoxButton, icon, ownerViewModel);
      Task waitTask = new Task(() =>
      {
        while (messageBoxResult == MessageBoxResult.None)
        {
          Task.Delay(50);
        }
      });

      waitTask.Start();
      await waitTask;
      return messageBoxResult;
    }
    public MessageBoxResult ShowMessageBoxFromNonUiThread(string title, string text,
                                                  MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                                                  UIElement icon = null,
                                                  IOwnerViewModel ownerViewModel = null)
    {
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;
      MessageBoxResult messageBoxResult = MessageBoxResult.None;
      ShowMessageBox(result => messageBoxResult = result, title, text, messageBoxButton, icon, ownerViewModel);
// ReSharper disable once LoopVariableIsNeverChangedInsideLoop (will change in other thread)
      while (messageBoxResult == MessageBoxResult.None)
      {
        Thread.Sleep(50);
      }
      return messageBoxResult;
    }

    public void CloseMessageBox(IOwnerViewModel ownerViewModel)
    {
      ownerViewModel.MessageBoxViewModel.CloseCommand.Execute(null);
    }

    public void CloseDialog(IWindowViewModel viewModel)
    {
      List<FrameworkElement> windowsToClose = m_views.Where(n => ReferenceEquals(n.DataContext, viewModel)).ToList();
      foreach (Window window in windowsToClose)
      {
        window.Close();
      }
    }

    public void CloseDialog(IDialogViewModel viewModel, IOwnerViewModel ownerViewModel = null)
    {
      if (ownerViewModel == null)
        ownerViewModel = m_rootView;
      ownerViewModel.DialogViewModel.CloseCommand.Execute(viewModel);
    }

    public event WindowClosedDelegate WindowClosed;

    public void BringToFront(IWindowViewModel viewModel)
    {
      Application.Current.Dispatcher.Invoke(() => BringToFront(FindOwner(viewModel)));

    }

    private void BringToFront(Window window)
    {
      if (window.WindowState == WindowState.Minimized)
        window.WindowState = WindowState.Normal;

      window.Activate();
      window.Topmost = true;
      window.Topmost = false;
      window.Focus();
    }

    public Rect GetSizeAndPosition(IWindowViewModel viewModel)
    {
      Window window = m_views.First(n => ReferenceEquals(n.DataContext, viewModel)) as Window;
      if (window == null) return default(Rect);
      return new Rect(window.Left, window.Top, window.Width, window.Height);
    }

    public void SetSizeAndPosition(IWindowViewModel viewModel, Rect rect)
    {
      Window window = m_views.First(n => ReferenceEquals(n.DataContext, viewModel)) as Window;
      if (window == null) return;
      window.Left = rect.X;
      window.Top = rect.Y;
      window.Width = rect.Width;
      window.Height = rect.Height;
    }

    #endregion
  }
}
