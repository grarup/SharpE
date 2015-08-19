using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SharpE.MvvmTools.Helpers
{
  public class BindableRichTextBox : RichTextBox
  {
    public static readonly DependencyProperty ViewModelProperty =
      DependencyProperty.Register("ViewModel", typeof(IBindableRichTextBoxViewModel), typeof(BindableRichTextBox), new PropertyMetadata(default(IBindableRichTextBoxViewModel), ViewModelChangedCallback));

    private static void ViewModelChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      RichTextBox richTextBox = dependencyObject as RichTextBox;
      if (richTextBox == null) return;
      IBindableRichTextBoxViewModel viewModel = dependencyPropertyChangedEventArgs.NewValue as IBindableRichTextBoxViewModel;
      if (richTextBox.Dispatcher == Application.Current.Dispatcher)
        richTextBox.Document = viewModel != null && viewModel.Document != null ? viewModel.Document : new FlowDocument();
      else
      {
        richTextBox.Dispatcher.Invoke(new Action(() => richTextBox.Document = viewModel != null && viewModel.Document != null ? viewModel.Document : new FlowDocument()));
      }
    }


    public IBindableRichTextBoxViewModel ViewModel
    {
      get { return (IBindableRichTextBoxViewModel)GetValue(ViewModelProperty); }
      set { SetValue(ViewModelProperty, value); }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
    {
      if (textChangedEventArgs.Changes.Count <= 0) return;
      if (ViewModel != null && ViewModel.TextChanged != null)
        ViewModel.TextChanged();
    }

    public BindableRichTextBox()
    {
      TextChanged += OnTextChanged;
      SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
    {
      ViewModel.CursorPosition = Selection.Start.GetOffsetToPosition(ViewModel.Document.ContentStart);
    }
  }
}
