using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using SharpE.Json.Schemas;

namespace SharpE.BaseEditors.Json.ViewModels.AutoComplete
{
  class StringCompletionDataViewModel : ICompletionData
  {
    private readonly ImageSource m_image = null;
    private readonly string m_text;
    private readonly object m_content;
    private readonly JsonEditorViewModel m_jsonEditorViewModel;
    private readonly object m_description;
    private const double c_priority = 0;
    private readonly SchemaObject m_schemaObject;

    public StringCompletionDataViewModel(object content, JsonEditorViewModel jsonEditorViewModel)
    {
      m_content = content;
      m_jsonEditorViewModel = jsonEditorViewModel;
      m_text = content.ToString();
      m_schemaObject = content as SchemaObject;
      if (m_schemaObject != null)
        m_description = m_schemaObject.Description;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
      if (m_jsonEditorViewModel.IsBetweenQoats)
      {
        int offset = m_jsonEditorViewModel.Caret.Offset;
        int indexQuotStart = m_jsonEditorViewModel.TextDocument.Text.LastIndexOf('"', offset - 1, offset - 1) + 1;
        int indexQuatEnd = m_jsonEditorViewModel.TextDocument.Text.IndexOf('"', offset);
        int indexLineBreak = m_jsonEditorViewModel.TextDocument.Text.IndexOf('\n', offset);
        if (indexQuatEnd > indexQuotStart && indexQuatEnd < indexLineBreak)
          completionSegment = new SelectionSegment(indexQuotStart, indexQuatEnd);
      }

      int endOffset = completionSegment.Offset + Text.Length;
      textArea.Document.Replace(completionSegment, Text);
      m_jsonEditorViewModel.Caret.Offset = endOffset;
      int stepBack = 0;
      if (m_jsonEditorViewModel.IsInKey)
      {
        if (m_jsonEditorViewModel.TextDocument.Text[m_jsonEditorViewModel.Caret.Offset] == '\"')
        {
          textArea.Document.Insert(m_jsonEditorViewModel.Caret.Offset + 1, " : ");
          m_jsonEditorViewModel.Caret.Offset += 4;
        }
        else
          textArea.Document.Insert(m_jsonEditorViewModel.Caret.Offset, "\" : ");
        SchemaObject schemaObject = m_jsonEditorViewModel.Schema.GetSchemaObject(m_jsonEditorViewModel.Path);
        string end;
        if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
        {
          if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
          {
            StringBuilder stringBuilder = new StringBuilder();
            schemaObject.GenerateAll(stringBuilder, m_jsonEditorViewModel.DeterminDepth(m_jsonEditorViewModel.Caret.Offset));
            end = stringBuilder.ToString();
          }
          else
          {
            StringBuilder stringBuilder = new StringBuilder();
            schemaObject.GenerateRequired(stringBuilder, m_jsonEditorViewModel.DeterminDepth(m_jsonEditorViewModel.Caret.Offset));
            end = stringBuilder.ToString();
          }
        }
        else
        {
          StringBuilder stringBuilder = new StringBuilder();
          stepBack = schemaObject.GenerateMin(stringBuilder, m_jsonEditorViewModel.File.Path);
          end = stringBuilder.ToString();
        }
        textArea.Document.Insert(m_jsonEditorViewModel.Caret.Offset, end);
        m_jsonEditorViewModel.Caret.Offset -= stepBack;
      }
      else
      {
        SchemaObject schemaObject = m_jsonEditorViewModel.Schema.GetSchemaObject(m_jsonEditorViewModel.Path);
        switch (schemaObject.SchemaAutoCompletType)
        {
          case SchemaAutoCompletType.FileAbsolute:
          case SchemaAutoCompletType.FileRelative:
            {
              string value = m_jsonEditorViewModel.GetCurrentValue();
              if (Text == "..\\" && value.Any(char.IsLetterOrDigit))
              {
                value = value.Substring(0, value.Length - 4);
                int startIndex = value.LastIndexOf('\\');
                if (startIndex == -1)
                  startIndex = 0;
                else
                {
                  startIndex++;
                }
                m_jsonEditorViewModel.TextDocument.Replace(m_jsonEditorViewModel.Caret.Offset - 4 - (value.Length - startIndex), (value.Length - startIndex) + 4, "");
              }
              m_jsonEditorViewModel.UpdateAutoCompletList = true;
              return;
            }
          case SchemaAutoCompletType.Key:
            break;
        }
        switch (schemaObject.Type)
        {
          case SchemaDataType.String:
            if (textArea.Document.Text[m_jsonEditorViewModel.Caret.Offset] == '"')
              m_jsonEditorViewModel.Caret.Offset++;
            else
              textArea.Document.Insert(m_jsonEditorViewModel.Caret.Offset, "\"");
            break;
        }
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
      get { return m_content; }
    }

    public object Description
    {
      get { return m_description; }
    }

    public double Priority
    {
      get { return c_priority; }
    }
  }
}
