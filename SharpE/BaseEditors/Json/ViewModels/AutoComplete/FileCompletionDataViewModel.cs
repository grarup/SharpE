using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using SharpE.Json.AutoComplet;

namespace SharpE.BaseEditors.Json.ViewModels.AutoComplete
{
  class FileCompletionDataViewModel : ICompletionData
  {
    private readonly ImageSource m_image = null;
    private readonly string m_text;
    private readonly AutoCompleteValue m_autoCompleteValue;
    private readonly JsonEditorViewModel m_jsonEditorViewModel;
    private const double c_priority = 0;

    public FileCompletionDataViewModel(AutoCompleteValue autoCompleteValue, JsonEditorViewModel jsonEditorViewModel)
    {
      m_autoCompleteValue = autoCompleteValue;
      m_jsonEditorViewModel = jsonEditorViewModel;
      m_text = autoCompleteValue.Value.ToString();
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
      if (m_jsonEditorViewModel.IsBetweenQoats)
      {
        int offset = m_jsonEditorViewModel.Caret.Offset;
        List<char> startChars = new List<char> {m_autoCompleteValue.SchemaObject.AutoCompletePathSeperator, '"'};
        if (m_autoCompleteValue.SchemaObject.Prefix.Length > 0)
          startChars.Add(m_autoCompleteValue.SchemaObject.Prefix.Last());
        int indexQuotStart = m_jsonEditorViewModel.TextDocument.Text.LastIndexOfAny(startChars.ToArray(), offset - 1, offset - 1) + 1;
        int indexQuatEnd = m_jsonEditorViewModel.TextDocument.Text.IndexOf('"', offset);
        int indexLineBreak = m_jsonEditorViewModel.TextDocument.Text.IndexOf('\n', offset);
        if (indexQuatEnd >= indexQuotStart && indexQuatEnd < indexLineBreak)
          completionSegment = new SelectionSegment(indexQuotStart, indexQuatEnd);
      }
      int endOffset = completionSegment.Offset + Text.Length;
      textArea.Document.Replace(completionSegment, Text + m_autoCompleteValue.SchemaObject.Suffix);
      m_jsonEditorViewModel.Caret.Offset = endOffset;
      string value = m_autoCompleteValue.SchemaObject.RemovePrefixAndSuffix(m_jsonEditorViewModel.GetCurrentValue());
      if (Text.StartsWith(".." + m_autoCompleteValue.SchemaObject.AutoCompletePathSeperator) && value.Any(char.IsLetterOrDigit))
      {
        value = value.Substring(0, value.Length - 4);
        int startIndex = value.LastIndexOf(m_autoCompleteValue.SchemaObject.AutoCompletePathSeperator);
        if (startIndex == -1)
          startIndex = m_autoCompleteValue.SchemaObject.Prefix.Length;
        else
        {
          startIndex++;
        }
        m_jsonEditorViewModel.TextDocument.Replace(
          m_jsonEditorViewModel.Caret.Offset - 4 - (value.Length - startIndex), (value.Length - startIndex) + 4, "");
      }
      m_jsonEditorViewModel.UpdateAutoCompletList = true;
    }

    public ImageSource Image
    {
      get { return m_image; }
    }

    public string Text
    {
      get { return m_text; }
    }

    public object Content
    {
      get { return m_autoCompleteValue.Value; }
    }

    public object Description
    {
      get { return null; }
    }

    public double Priority
    {
      get { return c_priority; }
    }
  }
}
