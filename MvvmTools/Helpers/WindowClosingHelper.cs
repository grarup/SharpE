using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace SharpE.MvvmTools.Helpers
{
  public class WindowClosingHelper
  {
    public static readonly DependencyProperty ClosingProperty =
      DependencyProperty.RegisterAttached("Closing", typeof(Func<Task<bool>>), typeof(WindowClosingHelper), new UIPropertyMetadata(ClosingChanged));

    private static void ClosingChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (DesignerProperties.GetIsInDesignMode(dependencyObject)) return;

      Window window = dependencyObject as Window;
      if (window == null) return;
      if (e.OldValue != null)
        window.Closing -= WindowOnClosing;
      if (e.NewValue != null)
        window.Closing += WindowOnClosing;
    }

    private static async void WindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
    {
      Func<Task<bool>> command = GetClosing((DependencyObject)sender);
      if (command == null) return;
      cancelEventArgs.Cancel = true;
      try
      {
        if (await command.Invoke())
        {
          Window window = sender as Window;
          window.Closing -= WindowOnClosing;
          bool succes = false;
          while (!succes)
          {
            try
            {
              window.Close();
              succes = true;
            }
            catch (Exception)
            {
            }
            await Task.Delay(10);
          }
        }
      }
      catch (Exception)
      {
        cancelEventArgs.Cancel = false;
      }
    }

    public static Func<Task<bool>> GetClosing(DependencyObject dependencyObject)
    {
      return (Func<Task<bool>>)dependencyObject.GetValue(ClosingProperty);
    }

    public static void SetClosing(DependencyObject dependencyObject, Func<Task<bool>> command)
    {
      dependencyObject.SetValue(ClosingProperty, command);
    }
  }
}
