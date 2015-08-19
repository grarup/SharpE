using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SharpE.MvvmTools.Helpers
{
  class MvvmTextBox : TextBox
  {
    protected override void OnSelectionChanged(RoutedEventArgs e)
    {
      base.OnSelectionChanged(e);
      CursorPosition = SelectionStart;
      LineIndex = GetLineIndexFromCharacterIndex(CursorPosition);
      if (LineIndex != -1)
        CharIndex = CursorPosition - GetCharacterIndexFromLineIndex(LineIndex);
    }

    public static readonly DependencyProperty CharIndexProperty =
      DependencyProperty.Register("CharIndex", typeof (int), typeof (MvvmTextBox), new PropertyMetadata(default(int)));

    public int CharIndex
    {
      get { return (int) GetValue(CharIndexProperty); }
      set { SetValue(CharIndexProperty, value); }
    }

    public static readonly DependencyProperty LineIndexProperty =
      DependencyProperty.Register("LineIndex", typeof (int), typeof (MvvmTextBox), new PropertyMetadata(default(int)));

    public int LineIndex
    {
      get { return (int) GetValue(LineIndexProperty); }
      set { SetValue(LineIndexProperty, value); }
    }

    public static readonly DependencyProperty CursorPositionProperty =
      DependencyProperty.Register("CursorPosition", typeof (int), typeof (MvvmTextBox), new PropertyMetadata(default(int), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      MvvmTextBox textBox = dependencyObject as MvvmTextBox;
      if (textBox == null) return;
      textBox.SelectionStart = (int) dependencyPropertyChangedEventArgs.NewValue;
    }

    public int CursorPosition
    {
      get { return (int) GetValue(CursorPositionProperty); }
      set { SetValue(CursorPositionProperty, value); }
    }

    public static readonly DependencyProperty TabWidthProperty =
      DependencyProperty.Register("TabWidth", typeof (int), typeof (MvvmTextBox), new PropertyMetadata(2));

    public int TabWidth
    {
      get { return (int) GetValue(TabWidthProperty); }
      set { SetValue(TabWidthProperty, value); }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          e.Handled = true;
          int jump = TabWidth - (CharIndex%TabWidth);
          if (string.IsNullOrEmpty(SelectedText))
          {
            int selectionStart = SelectionStart;
            Text = Text.Insert(SelectionStart, new string(' ', jump));
            SelectionStart = selectionStart + jump;
          }
          else
            SelectedText = new string(' ', jump);

          break;
      }
      base.OnPreviewKeyDown(e);
    }

    public static readonly DependencyProperty TextInserterProperty =
      DependencyProperty.Register("TextInserter", typeof (ITextInserter), typeof (MvvmTextBox), new PropertyMetadata(default(ITextInserter), TextInserterPropertyChangedCallback));

    private static void TextInserterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      MvvmTextBox textBox = dependencyObject as MvvmTextBox;
      if (textBox == null) return;
      ITextInserter oldTextInserter = dependencyPropertyChangedEventArgs.OldValue as ITextInserter;
      if (oldTextInserter != null)
        oldTextInserter.ReplaceText -= textBox.ReplaceText;
      ITextInserter newTextInserter = dependencyPropertyChangedEventArgs.NewValue as ITextInserter;
      if (newTextInserter != null)
        newTextInserter.ReplaceText += textBox.ReplaceText;
    }

    private void ReplaceText(string text, int start, int length)
    {
      SelectionStart = start;
      SelectionLength = length;
      SelectedText = text;
      SelectionStart = start + text.Length;
      SelectionLength = 0;
    }

    public ITextInserter TextInserter
    {
      get { return (ITextInserter) GetValue(TextInserterProperty); }
      set { SetValue(TextInserterProperty, value); }
    }
  }
}
