using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using SharpE.BaseEditors.AvalonTextEditorAddons;
using SharpE.BaseEditors.BaseTextEditor;
using SharpE.BaseEditors.Json.ViewModels.AutoComplete;
using SharpE.BaseEditors.Json.Views;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.Json.AutoComplet;
using SharpE.Json.Data;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Exstensions;
using SharpE.ViewModels;
using SharpE.ViewModels.ContextMenu;
using SharpE.ViewModels.Tree;

namespace SharpE.BaseEditors.Json.ViewModels
{
  class JsonEditorViewModel : BaseTextEditorViewModel
  {

    #region declerations
    private readonly SchemaManager m_schemaManager;
    private int m_cursorLine;
    private CompletionWindow m_completionWindow;
    private bool m_isBetweenQoats;
    private string m_pathString;
    private int m_errorCount;
    private List<string> m_path = new List<string>();
    private bool m_isInKey;
    private List<AutoCompleteValue> m_autoCompleteValues;
    private Schema m_schema;
    private bool m_textNeedCheck;
    private readonly AsyncObservableCollection<LineDescriptionViewModel> m_lineDescriptors = new AsyncObservableCollection<LineDescriptionViewModel>();
    private bool m_updateAutoCompletList;
    private ErrorMargin m_errorMargin;
    private readonly IObservableCollection<ValidationError> m_errors = new AsyncObservableCollection<ValidationError>();
    private bool m_showToolTip;
    private Dictionary<int, List<ValidationError>> m_allErrors;
    private JsonException m_jsonException;
    private string m_type;
    private FoldingManager m_foldingManager;
    private BraceFoldingStrategy m_foldingStrategy;

    private string m_value;

    private SchemaObject m_autoCompleteSchemaObject;
    private UIElement m_extraInfoElement;

    #endregion

    #region constructor
    public JsonEditorViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
      m_supportedFiles = new List<string> { ".json" };
      m_name = "Json editor";
      m_schemaManager = mainViewModel.SchemaManager;
      Task task = new Task(CheckJsonTask);
      task.Start();
      Task task2 = new Task(UpdateAutoCompletTask);
      task2.Start();

      m_menuItems = new ObservableCollection<IMenuItemViewModel>
        {
          new MenuItemViewModel("Generate schema", new ManualCommand(GenerateSchema))
        };
    }
    #endregion

    #region private methods

    private void GenerateSchema()
    {
      if (m_mainViewModel.LayoutManager.ActiveLayoutElement == null || m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile == null || m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.Exstension != ".json")
        return;
      JsonException jsonException;
      JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.GetContent<string>(), out jsonException);
      if (jsonException != null)
        return;
      JsonNode schemaNode = SchemaHelper.GenerateSchema(jsonNode);
      m_mainViewModel.CreateNewFile();
      m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.SetContent(schemaNode.ToString());
    }
    private string CorrectIndent(string text)
    {
      Stack<char> bracketOrder = new Stack<char>();
      string newText = text;
      int depth = 0;
      int depthChange = 0;
      int linestart = 0;
      int textstart = 0;
      bool textstartfound = false;
      bool earlyDepthchange = false;
      int index = 0;
      while (index < newText.Length)
      {
        char c = newText[index];
        if (!textstartfound && !char.IsWhiteSpace(c))
        {
          textstartfound = true;
          textstart = index;
          earlyDepthchange = c == '}' || c == ']';

        }
        switch (c)
        {
          case '\n':
            if (earlyDepthchange)
              depth += depthChange;
            int indent = textstart - linestart;
            newText = newText.Substring(0, Math.Max(0, linestart)) + (depth > 0 ? string.Concat(Enumerable.Repeat(m_indentString, depth)) : "") + newText.Substring(textstart, newText.Length - textstart);
            textstartfound = false;
            index -= (indent - (depth * m_indentString.Length));
            linestart = index + 1;
            if (!earlyDepthchange)
              depth += depthChange;
            depthChange = 0;
            earlyDepthchange = false;
            break;
          case '[':
            depthChange++;
            bracketOrder.Push('[');
            break;
          case '{':
            depthChange++;
            bracketOrder.Push('{');
            break;
          case ']':
            depthChange--;
            if (bracketOrder.Pop() != '[')
              return text;
            break;
          case '}':
            depthChange--;
            if (bracketOrder.Pop() != '{')
              return text;
            break;
        }
        index++;
      }
      return newText;
    }

    private void CheckJsonTask()
    {
      while (!m_disposed)
      {
        if (m_textNeedCheck)
        {
          m_textNeedCheck = false;
          JsonObject obj = JsonHelperFunctions.Parse(m_text, out m_jsonException);
          if (m_errorMargin != null)
            m_errorMargin.JsonException = m_jsonException;
          m_allErrors = new Dictionary<int, List<ValidationError>>();
          if (m_jsonException != null)
            m_allErrors.Add(m_jsonException.LineIndex, new List<ValidationError> { new ValidationError(ValidationErrorState.NotCorrectJson, m_jsonException.LineIndex, m_jsonException.CharIndex, m_jsonException.Message) });
          if (obj != null && Schema != null) Schema.Validate(obj, m_allErrors, System.IO.Path.GetDirectoryName(m_file.Path));
          if (m_errorMargin != null)
            m_errorMargin.Errors = m_allErrors;
          ErrorCount = m_allErrors == null ? 0 : m_allErrors.Count;
          m_file.ValidationState = m_jsonException != null
                                     ? ValidationState.Errors
                                     : (m_allErrors.Any() ? ValidationState.Warnings : ValidationState.Good);
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }

    protected override Schema GetSettingsSchema(SchemaManager schemaManager)
    {
      return new Schema(Properties.Resources.jsoneditor_schema, schemaManager);
    }

    protected override IFileViewModel GetSettings()
    {
      if (!System.IO.File.Exists(Properties.Settings.Default.SettingPath + "\\jsonedit.settings.json"))
        System.IO.File.WriteAllBytes(Properties.Settings.Default.SettingPath + "\\jsonedit.settings.json", Properties.Resources.jsonedit_settings);
      return m_mainViewModel.GetSetting(Properties.Settings.Default.SettingPath + "\\jsonedit.settings.json");
    }

    public void IndentAll()
    {
      int offset = m_offset;
      m_textDocument.Text = CorrectIndent(m_text);
      m_caret.Offset = Math.Min(m_text.Length, offset);
    }

    //private void IndentLine(int start)
    //{
    //  int cursorStart = start;
    //  int lasttext = start;
    //  int index = m_cursorPosition;
    //  char c = m_text[index];
    //  while (index > 0 && c != '\n')
    //  {
    //    if (!Char.IsWhiteSpace(c))
    //      lasttext = index;
    //    index--;
    //    c = m_text[index];
    //  }
    //  int indent = lasttext - (index + 1);
    //  int depth = m_path.Count * 2 + 2;
    //  if (indent > depth)
    //    RemoveText(index + 1, indent - depth);
    //  else
    //    InsertText(new string(' ', depth - indent), index + 1);
    //  CursorPosition = cursorStart - (indent - depth);
    //}

    public void ShowAutoComplete()
    {
      UpdateAutoCompletList = true;
    }

    private void UpdateAutoCompletTask()
    {
      while (!m_disposed)
      {
        if (UpdateAutoCompletList)
        {
          UpdateAutoCompletList = false;
          Schema schema = Schema;
          if (schema != null)
          {
            if (IsInKey && m_text[m_offset - 1] != '"')
              continue;
            SchemaObject schemaObject = m_autoCompleteSchemaObject ?? schema.GetSchemaObject(m_path);
            if (schemaObject == null)
              continue;

            if (IsBetweenQoats || schemaObject.Type == SchemaDataType.Boolean)
            {
              List<AutoCompleteValue> autoCompleteValues =
                m_mainViewModel.AutoCompleteCollectionManager.GenerateAutoComplete(IsInKey, schemaObject, Value,
                                                                                   m_offset -
                                                                                   m_text.LastIndexOf('"', m_offset - 1) - 1,
                                                                                   m_file.Path);
              lock (this)
              {
                m_autoCompleteValues = autoCompleteValues;
              }
            }
            else
            {
              lock (this)
              {
                m_autoCompleteValues = null;
              }
            }
          }
          UpdateAutoCompletVisibilty();
        }
        else
          Thread.Sleep(10);
      }
    }

    private void UpdateAutoCompletVisibilty()
    {
      if (m_view == null) return;
      if (!m_view.Dispatcher.CheckAccess())
      {
        m_view.Dispatcher.Invoke(UpdateAutoCompletVisibilty);
        return;
      }
      List<AutoCompleteValue> autoCompleteValues;
      lock (this)
      {
        autoCompleteValues = m_autoCompleteValues;
      }
      if (autoCompleteValues != null && autoCompleteValues.Count > 0)
      {
        if (m_completionWindow == null)
        {
          m_completionWindow = new CompletionWindow(m_view.TextEditor.TextArea);
          m_completionWindow.Closed += CompletionWindowOnClosed;
          if (IsBetweenQoats)
          {
            int index = m_text.LastIndexOf('"', m_offset - (m_text[m_offset] == '"' ? 1 : 0)) + 1;
            m_completionWindow.StartOffset = index;
          }
          m_completionWindow.Show();
        }
        m_completionWindow.CompletionList.CompletionData.Clear();
        foreach (AutoCompleteValue obj in autoCompleteValues)
        {
          switch (obj.Type)
          {
            case AutocompleteType.Undefined:
              break;
            case AutocompleteType.String:
              m_completionWindow.CompletionList.CompletionData.Add(new StringCompletionDataViewModel(obj.Value, this));
              break;
            case AutocompleteType.File:
              m_completionWindow.CompletionList.CompletionData.Add(new FileCompletionDataViewModel(obj, this));
              break;
            case AutocompleteType.Selector:
              m_completionWindow.CompletionList.CompletionData.Add(new SelectionCompletionDataViewModel(obj, this));
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }
      else
      {
        if (m_completionWindow != null)
          m_completionWindow.Hide();
      }
    }

    internal string GetCurrentValue()
    {
      if (m_isBetweenQoats)
      {
        int indexQuotStart = m_offset;
        bool isEcaped = true;
        while (isEcaped)
        {
          indexQuotStart = m_text.LastIndexOf('"', m_offset - 1, indexQuotStart);
          isEcaped = indexQuotStart == 0 || m_text[indexQuotStart - 1] == '\\';
        }
        int length = m_offset - indexQuotStart - 1;
        return m_text.Substring(indexQuotStart + 1, length);
      }
      else
      {
        int indexQuotStart = m_text.LastIndexOf(':', m_offset - 1, m_offset);
        int length = m_offset - indexQuotStart - 1;
        return m_text.Substring(indexQuotStart + 1, length);
      }
    }

    private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
    {

      if (textCompositionEventArgs.Text.Length > 0 && m_completionWindow != null)
      {
        if ((textCompositionEventArgs.Text[0] == '"' && IsBetweenQoats))
        {
          if (m_completionWindow.CompletionList.SelectedItem != null)
          {
            m_completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
            textCompositionEventArgs.Handled = true;
          }
          else
          {
            m_completionWindow.Close();
          }
        }
        //else
        //{
        //  UpdateAutoCompletList = true;
        //}
      }
      else
      {
        if (IsInKey && m_text[m_offset - 1] == '"')
        {
          UpdateAutoCompletList = true;
        }
        if (textCompositionEventArgs.Text.Length > 0 && textCompositionEventArgs.Text[0] == '"' && (m_text.Length > m_offset && m_text[m_offset] == '"' && IsBetweenQoats))
        {
          m_caret.Offset++;
          textCompositionEventArgs.Handled = true;
        }
        if (textCompositionEventArgs.Text == "}" && m_text != null && m_text.Length > m_offset && m_text[m_offset] == '}')
        {
          m_caret.Offset++;
          textCompositionEventArgs.Handled = true;
        }
        if (textCompositionEventArgs.Text == "]" && m_text != null && m_text.Length > m_offset && m_text[m_offset] == ']')
        {
          m_caret.Offset++;
          textCompositionEventArgs.Handled = true;
        }
        if (textCompositionEventArgs.Text == "{" && m_text != null && m_text.Length > m_offset && m_text[m_offset] != '}')
        {
          m_textDocument.Insert(m_offset, "}");
          m_caret.Offset--;
        }
        if (textCompositionEventArgs.Text == "[" && m_text != null && m_text.Length > m_offset && m_text[m_offset] != ']')
        {
          m_textDocument.Insert(m_offset, "]");
          m_caret.Offset--;
        }
        if (textCompositionEventArgs.Text == "\"" && m_text != null && m_text.Length > m_offset && m_text[m_offset] != '\"' && !IsBetweenQoats)
        {
          SchemaObject schemaObject;
          if (m_schema != null && (schemaObject = m_schema.GetSchemaObject(m_path)) != null && schemaObject.AutoCompleteTargetKey == null)
          {
            m_textDocument.Insert(m_offset, "\"" + schemaObject.Prefix + schemaObject.Suffix + "\"");
            m_caret.Offset -= 1 + schemaObject.Suffix.Length;
            textCompositionEventArgs.Handled = true;
          }
          else
          {
            m_textDocument.Insert(m_offset, "\"");
            m_caret.Offset--;
          }
        }

        if (textCompositionEventArgs.Text == "\n" && m_offset > 0 && m_text != null && m_offset < m_text.Length && ((m_text[m_offset - 1] == '{' && 
            m_text[m_offset] == '}') | (m_text[m_offset - 1] == '[' &&
            m_text[m_offset] == ']')))
        {
          string text = m_textDocument.GetText(m_textDocument.GetLineByOffset(m_offset));
          int depth = text.Length - text.TrimStart().Length;
          m_textDocument.Insert(m_offset, "\r\n" + string.Concat(Enumerable.Repeat(m_indentString, depth + 1)) + "\r\n" + string.Concat(Enumerable.Repeat(m_indentString, depth)));
          m_caret.Offset -= 2 + depth * m_indentString.Length;
          textCompositionEventArgs.Handled = true;
        }
      }
    }

    protected override void FileChangedExternaley(string text)
    {
      Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
      Match match = scheamRegex.Match(text);
      Schema = match.Success ? m_schemaManager.GetSchema(match.Groups[1].ToString()) : null;
      if (m_errorMargin != null)
        m_errorMargin.HasSchema = m_schema != null;
    }

    protected override void OffsetChanged()
    {
      IsBetweenQoats = JsonHelperFunctions.CountQoatsBefore(m_textDocument.Text, Caret.Offset) % 2 == 1;
      IsInKey = IsBetweenQoats && JsonHelperFunctions.DetectIsInKey(m_textDocument.Text, Caret.Offset);
      JsonHelperFunctions.DeterminPath(m_textDocument.Text, Caret.Offset, out m_path);
      PathString = Path == null ? "" : String.Join("/", Path);
      if (m_schema != null)
      {
        SchemaObject schemaObject = m_schema.GetSchemaObject(Path);
        Type = schemaObject == null ? "-" : schemaObject.Type.ToString();
      }
      if (!IsInKey)
      {
        if (IsBetweenQoats)
        {
          int indexQuotStart = Math.Min(m_offset , m_text.Length - 1);
          bool isEcaped = true;
          while (isEcaped)
          {
            indexQuotStart = m_text.LastIndexOf('"', indexQuotStart - 1);
            isEcaped = indexQuotStart > 0 && m_text[indexQuotStart - 1] == '\\';
          }
          int startIndex = indexQuotStart + 1;
          indexQuotStart = Math.Min(m_offset - 1, m_text.Length - 1);
          isEcaped = true;
          while (isEcaped)
          {
            indexQuotStart = m_text.IndexOf('"', indexQuotStart + 1);
            isEcaped = indexQuotStart > 0 && m_text[indexQuotStart - 1] == '\\';
          }
          int stopIndex = indexQuotStart;
          if (startIndex < 0 || stopIndex < 0 || startIndex > stopIndex)
            Value = "-";
          else
            Value = m_text.Substring(startIndex, stopIndex - startIndex).StripIndent();
        }
        else
        {
          if (m_path.Count > 0)
          {
            Regex scheamRegex = new Regex(@"\[(\d*)\]", RegexOptions.IgnoreCase);
            Match match = scheamRegex.Match(m_path.Last());
            Value = match.Success ? JsonHelperFunctions.GetValue(m_text, m_offset, int.Parse(match.Groups[1].ToString())).StripIndent() : JsonHelperFunctions.GetValue(m_text, m_offset).StripIndent();
          }
          else
            Value = "-";
          //int startIndex = m_text.LastIndexOfAny(new List<char> { '\n', '\r', ':', '{', '[' }, Math.Max(0, Math.Min(m_offset - 1, m_text.Length - 1))) + 1;
          //int stopIndex = m_text.FirstIndexOfAny(new List<char> {'\n', '\r', ',','}',']'}, Math.Min(m_offset, m_text.Length - 1)) - 1;
          //if (startIndex < 0 || stopIndex < 0 || startIndex > stopIndex)
          //  Value = "-";
          //else
          //  Value = m_text.Substring(startIndex, stopIndex - startIndex);
        }
      }
      else
      {
        if (IsBetweenQoats)
        {
          int quatIndex = Math.Min(m_offset, m_text.Length - 1);
          bool isEcaped = true;
          while (isEcaped)
          {
            quatIndex = m_text.LastIndexOf('"', quatIndex - 1);
            isEcaped = quatIndex != 0 && m_text[quatIndex - 1] == '\\';
          }
          if (quatIndex == -1)
            Value = "-";
          {
            int startIndex = quatIndex + 1;
            quatIndex = Math.Min(m_offset - 1, m_text.Length - 1);
            isEcaped = true;
            while (isEcaped)
            {
              quatIndex = m_text.IndexOf('"', quatIndex + 1);
              isEcaped = quatIndex != -1 && quatIndex != 0 && m_text[quatIndex - 1] == '\\';
            }
            if (quatIndex == -1)
              Value = "-";
            else
            {
              int stopIndex = quatIndex;
              if (startIndex < 0 || stopIndex < 0 || startIndex > stopIndex)
                Value = "-";
              else
                Value = m_text.Substring(startIndex, stopIndex - startIndex).StripIndent();
            }
          }
        }
        else
        {
          int startIndex = m_text.IndexOf('"', m_offset) + 1;
          int colonIndex = m_text.IndexOf(':', startIndex);
          if (colonIndex != -1 && m_text.Substring(startIndex, colonIndex - startIndex).Trim().Length == 0)
          {
            Value = JsonHelperFunctions.GetValue(m_text, colonIndex + 1).StripIndent();
          }
          else
            Value = "-";
        }
      }
    }

    protected override void TextChanged()
    {
      if (m_path != null && m_path.Count > 0 && m_path[0] == "$schema")
      {
        Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
        Match match = scheamRegex.Match(m_text);
        Schema = match.Success ? m_schemaManager.GetSchema(match.Groups[1].ToString()) : null;
      }
      UpdateAutoCompletList = true;
      m_textNeedCheck = true;
    }

    protected override void InitFromFile(string text)
    {
      Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
      Match match = scheamRegex.Match(text);
      Schema = match.Success ? m_schemaManager.GetSchema(match.Groups[1].ToString()) : null;
      if (m_errorMargin != null)
        m_errorMargin.HasSchema = m_schema != null;
      m_textDocument.Text = text;
      m_textDocument.UndoStack.ClearAll();
      if (m_foldingStrategy != null)
        m_foldingStrategy.UpdateFoldings(m_foldingManager, m_textDocument);

    }

    protected override void InitEditor()
    {
      m_view.TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");
      m_errorMargin = new ErrorMargin
        {
          TextView = m_view.TextEditor.TextArea.TextView,
          HasSchema = m_schema != null,
          Errors = m_allErrors,
          JsonException = m_jsonException
        };
      m_errorMargin.PropertyChanged += ErrorMarginOnPropertyChanged;
      m_view.TextEditor.TextArea.LeftMargins.Add(m_errorMargin);
      m_view.TextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
      m_view.TextEditor.TextArea.TextEntering += TextAreaOnTextEntering;
      m_foldingManager = FoldingManager.Install(m_view.TextEditor.TextArea);
      m_foldingStrategy = new BraceFoldingStrategy();
      m_foldingStrategy.UpdateFoldings(m_foldingManager, m_textDocument);
    }

    private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
    {
      if (textCompositionEventArgs.Text.Contains('{') || textCompositionEventArgs.Text.Contains('}'))
        m_foldingStrategy.UpdateFoldings(m_foldingManager, m_textDocument);
      UpdateAutoCompletList = true;
    }

    private void ErrorMarginOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "TooltipLine":
          m_errors.Clear();
          ShowToolTip = false;
          if (m_errorMargin.TooltipLine != -1 && m_errorMargin.Errors != null)
          {
            if (m_errorMargin.Errors.ContainsKey(m_errorMargin.TooltipLine))
            {
              foreach (ValidationError validationError in m_errorMargin.Errors[m_errorMargin.TooltipLine])
                m_errors.Add(validationError);
              ShowToolTip = true;
            }
          }
          break;
      }
    }

    private void CompletionWindowOnClosed(object sender, EventArgs eventArgs)
    {
      m_completionWindow = null;
    }

    #endregion

    #region public properties

    public override UIElement ExtraInfoElement
    {
      get { return m_extraInfoElement ?? (m_extraInfoElement = new JsonExtraInfo()); }
    }

    public bool IsBetweenQoats
    {
      get { return m_isBetweenQoats; }
      set
      {
        if (value.Equals(m_isBetweenQoats)) return;
        m_isBetweenQoats = value;
        OnPropertyChanged();
      }
    }

    public string PathString
    {
      get { return m_pathString; }
      set
      {
        if (value == m_pathString) return;
        m_pathString = value;
        m_autoCompleteSchemaObject = null;
        OnPropertyChanged();
      }
    }

    public bool IsInKey
    {
      get { return m_isInKey; }
      set
      {
        if (value.Equals(m_isInKey)) return;
        m_isInKey = value;
        OnPropertyChanged();
      }
    }

    public IObservableCollection<LineDescriptionViewModel> LineDescriptors
    {
      get { return m_lineDescriptors; }
    }

    public int CursorLine
    {
      get { return m_cursorLine; }
      set
      {
        if (value == m_cursorLine) return;
        if (m_cursorLine >= 0 && m_cursorLine < LineDescriptors.Count)
          LineDescriptors[m_cursorLine].IsCurrentLine = false;
        m_cursorLine = value;
        if (m_cursorLine >= 0 && m_cursorLine < LineDescriptors.Count)
          LineDescriptors[m_cursorLine].IsCurrentLine = true;
        OnPropertyChanged();
      }
    }

    public int ErrorCount
    {
      get { return m_errorCount; }
      set
      {
        if (value == m_errorCount) return;
        m_errorCount = value;
        OnPropertyChanged();
      }
    }

    public IObservableCollection<ValidationError> Errors
    {
      get { return m_errors; }
    }

    public bool ShowToolTip
    {
      get { return m_showToolTip; }
      set
      {
        if (value.Equals(m_showToolTip)) return;
        m_showToolTip = value;
        OnPropertyChanged();
      }
    }

    public Schema Schema
    {
      get { return m_schema; }
      set
      {
        if (Equals(value, m_schema)) return;
        m_schema = value;
        if (m_errorMargin != null)
          m_errorMargin.HasSchema = m_schema != null;
        OnPropertyChanged();
      }
    }

    public List<string> Path
    {
      get { return m_path; }
    }

    public string Type
    {
      get { return m_type; }
      set
      {
        if (value == m_type) return;
        m_type = value;
        OnPropertyChanged();
      }
    }

    public string Value
    {
      get { return m_value; }
      set
      {
        if (value == m_value) return;
        m_value = value;
        OnPropertyChanged();
      }
    }

    public SchemaObject AutoCompleteSchemaObject
    {
      get { return m_autoCompleteSchemaObject; }
      set
      {
        if (Equals(value, m_autoCompleteSchemaObject)) return;
        m_autoCompleteSchemaObject = value;
        UpdateAutoCompletList = true;
        OnPropertyChanged();
      }
    }

    public bool UpdateAutoCompletList
    {
      get { return m_updateAutoCompletList; }
      set
      {
        if (value.Equals(m_updateAutoCompletList)) return;
        m_updateAutoCompletList = value;
        OnPropertyChanged();
      }
    }
    #endregion

    #region public methods
    public void OpenFile()
    {
      if (m_schema == null) return;
      SchemaObject schemaObject = m_schema.GetSchemaObject(m_path);
      
      string value;
      if (schemaObject != null)
        value = Value.Substring(schemaObject.Prefix.Length, Value.Length - schemaObject.Prefix.Length - schemaObject.Suffix.Length);
      else
        value = Value;

      if (m_path.Count > 0 && m_path.Last() == "$schema")
      {
        foreach (string path in m_schemaManager.Paths)
        {
          if (System.IO.File.Exists(path + "\\" + value))
            m_mainViewModel.OpenFile(path + "\\" + value);
        }
        Schema schema = m_schemaManager.GetSchema(value);
        if (schema != null)
          m_mainViewModel.OpenFileViewModelCommand.Execute(new FileViewModel(schema.Text, schema.Name));
      }
      else if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(m_file.Path) + "\\" + value))
        m_mainViewModel.OpenFile(System.IO.Path.GetDirectoryName(m_file.Path) + "\\" + value);
    }

    public int DeterminDepth(int offset)
    {
      int startIndex = m_text.LastIndexOf('\n', offset, offset);
      int retval = 0;
      for (int i = startIndex + 1; i < offset; i++)
      {
        if (m_text[i] != '\t')
          return retval;
        retval++;
      }
      return retval;
    }

    #endregion

    #region overrides
    public override IEditor CreateNew()
    {
      return new JsonEditorViewModel(m_mainViewModel);
    }
    #endregion
  }
}
