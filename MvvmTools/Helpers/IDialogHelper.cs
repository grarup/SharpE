using System;
using System.Threading.Tasks;
using System.Windows;

namespace SharpE.MvvmTools.Helpers
{
  public delegate void WindowClosedDelegate(IWindowViewModel viewModel);

  public interface IDialogHelper
  {
    /// <summary>
    /// shows a dialog
    /// </summary>
    /// <param name="ownerViewModel">the viewmodel that represents the owner window, if null the dialog will be shown free from other windows.</param>
    /// <param name="viewModel">the viewmodel for the window to be shown.</param>
    /// <returns>the result from when the dialog is closed</returns>
    bool? ShowDialogInWindow(IViewModel viewModel, IViewModel ownerViewModel);

    void ShowDialog(IDialogViewModel viewModel, IOwnerViewModel ownerViewModel = null);

    /// <summary>
    /// shows an extra window
    /// </summary>
    /// <param name="viewModel">the viewmodel for the window to be shown.</param>
    /// <param name="rect">position and size</param>
    void ShowWindow(IWindowViewModel viewModel, Rect? rect = null);

    /// <summary>
    /// Shows a messagebox
    /// </summary>
    /// <param name="ownerViewModel">the viewmodel that represents the owner window</param>
    /// <param name="text">the text of the messagebox</param>
    /// <param name="resultAction">the function called when the mesagebox closes.</param>
    /// <param name="title">the title of the messagebox</param>
    /// <param name="messageBoxButton">the buttons showed in the messagebox</param>
    /// <param name="icon">the icon showen in the messagebox</param>
    /// <returns>the result of the messagebox</returns>
    void ShowMessageBox(Action<MessageBoxResult> resultAction , string title, string text, MessageBoxButton messageBoxButton = MessageBoxButton.OK, UIElement icon = null, IOwnerViewModel ownerViewModel = null);

    /// <summary>
    /// Shows a messagebox
    /// </summary>
    /// <param name="ownerViewModel">the viewmodel that represents the owner window</param>
    /// <param name="text">the text of the messagebox</param>
    /// <param name="values">the values the resultAction will be called with</param>
    /// <param name="title">the title of the messagebox</param>
    /// <param name="messageBoxButton">the buttons showed in the messagebox</param>
    /// <param name="icon">the icon showen in the messagebox</param>
    /// <param name="resultAction">The action that is called when the messagebox is closed</param>
    /// <returns>the result of the messagebox</returns>
    void ShowMessageBox(Action<MessageBoxResult, object[]> resultAction, object[] values, string title, string text, MessageBoxButton messageBoxButton = MessageBoxButton.OK, UIElement icon = null, IOwnerViewModel ownerViewModel = null);

    Task<MessageBoxResult> ShowMessageBox(string title, string text,
                                                MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                                                UIElement icon = null, IOwnerViewModel ownerViewModel = null);

    MessageBoxResult ShowMessageBoxFromNonUiThread(string title, string text,
                                                MessageBoxButton messageBoxButton = MessageBoxButton.OK,
                                                UIElement icon = null, IOwnerViewModel ownerViewModel = null);

      /// <summary>
      /// Closes a messagebox
      /// </summary>
      /// <param name="ownerViewModel">the viewmodel that is showing the messagebox</param>
      void CloseMessageBox(IOwnerViewModel ownerViewModel);

    /// <summary>
    /// Closes a dialog
    /// </summary>
    /// <param name="viewModel">the viewmodel for the views to close. If more than one view uses the viewModel all are closed.</param>
    void CloseDialog(IWindowViewModel viewModel);

    void CloseDialog(IDialogViewModel viewModel, IOwnerViewModel ownerViewModel = null);

    /// <summary>
    /// Fired when a window closes.
    /// </summary>
    event WindowClosedDelegate WindowClosed;

    void BringToFront(IWindowViewModel viewModel);

    Rect GetSizeAndPosition(IWindowViewModel viewModel);

    void SetSizeAndPosition(IWindowViewModel viewModel, Rect rect);
  }
}