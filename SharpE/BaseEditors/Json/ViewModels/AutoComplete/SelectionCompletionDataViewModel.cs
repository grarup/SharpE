using System;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using SharpE.Json.AutoComplet;
using SharpE.Json.Schemas;

namespace SharpE.BaseEditors.Json.ViewModels.AutoComplete
{
  class SelectionCompletionDataViewModel : ICompletionData
  {
    private readonly ImageSource m_image = null;
    private readonly string m_text;
    private readonly object m_content;
    private readonly JsonEditorViewModel m_jsonEditorViewModel;
    private readonly object m_description;
    private const double c_priority = 0;
    private readonly SchemaObject m_schemaObject;

    public SelectionCompletionDataViewModel(AutoCompleteValue content, JsonEditorViewModel jsonEditorViewModel)
    {
      m_content = content.Value;
      m_jsonEditorViewModel = jsonEditorViewModel;
      m_text = content.Value.ToString();
      m_schemaObject = content.Value as SchemaObject;
      if (m_schemaObject != null)
        m_description = m_schemaObject.Description;
    }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int stepB = m_schemaObject.GenerateMin(stringBuilder, m_jsonEditorViewModel.File.Path);
      textArea.Document.Replace(completionSegment, stringBuilder.ToString());
      m_jsonEditorViewModel.Caret.Offset -= stepB;
      m_jsonEditorViewModel.AutoCompleteSchemaObject = m_schemaObject;
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
