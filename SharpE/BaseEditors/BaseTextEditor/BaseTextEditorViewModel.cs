using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using SharpE.BaseEditors.AvalonTextEditorAddons;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Properties;
using SharpE.ViewModels;
using SharpE.ViewModels.Tree;

namespace SharpE.BaseEditors.BaseTextEditor
{
  internal class BaseTextEditorViewModel : IEditor, IDisposable
  {
    protected readonly MainViewModel m_mainViewModel;
    protected BaseTextEditorView m_view;
    protected IFileViewModel m_file;
    protected IEnumerable<string> m_supportedFiles = new List<string>();
    protected bool m_disposed;
    protected Caret m_caret;
    protected TextDocument m_textDocument;
    protected string m_text;
    private int m_lineNumber;
    private int m_charNumber;
    protected int m_offset;
    protected int m_tabSize = 2;
    protected string m_indentString = "\t";
    protected bool m_convertTabsToSpaces;
    protected IFileViewModel m_settings;
    private string m_searchString;
    private bool m_isFindVisible;
    private EditFocusTag m_focusTag;
    private List<KeyBinding> m_keyBindings;
    private readonly ManualCommand m_findNextCommand;
    private int m_findIndex = -1;
    private string m_replaceString;
    private readonly ManualCommand m_replaceCommand;
    private readonly ManualCommand m_replaceAllCommand;
    private Match m_currentMatch;
    protected string m_name = "Base text editor";
    private TextMarkingBackGroundRender m_textMarkingBackGroundRender;
    protected Schema m_settningsSchema;

    public BaseTextEditorViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      SetHighLight(Properties.Resources.Json_Mode, new[] {".json"}, "Json highlighting");
      SetHighLight(Properties.Resources.CSharp_Mode, new[] { ".cs" }, "C# highlighting");
      SetHighLight(Properties.Resources.CPP_Mode, new[] { ".c", ".h", ".cpp", ".hpp" }, "c/c++ highlighting");
      SetHighLight(Properties.Resources.CSS_Mode, new[] { ".css" }, "Css highlighting");
      SetHighLight(Properties.Resources.HTML_Mode, new[] { ".html", ".htm" }, "Html highlighting");
      SetHighLight(Properties.Resources.JavaScript_Mode, new[] { ".js" }, "Javascript highlighting");
      SetHighLight(Properties.Resources.XmlDoc, new[] { ".xml", ".xshd" }, "Xml highlighting");
      SetHighLight(Properties.Resources.Lua, new[] { ".lua", ".luac" }, "Lua highlighting");


      m_textDocument = new TextDocument();
      m_textDocument.TextChanged += TextDocumentOnTextChanged;

      m_findNextCommand = new ManualCommand(FindNext);
      m_replaceCommand = new ManualCommand(Replace);
      m_replaceAllCommand = new ManualCommand(ReplaceAll);
      m_settningsSchema = GetSettingsSchema(m_mainViewModel.SchemaManager);
      Settings = GetSettings();
    }

    private void SetHighLight(byte[] data, string[] exstensions, string description)
    {
      IHighlightingDefinition customHighlighting;
      using (Stream s = new MemoryStream(data))
      {
        using (XmlReader reader = new XmlTextReader(s))
        {
          customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
            HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
      // and register it in the HighlightingManager
      HighlightingManager.Instance.RegisterHighlighting(description, exstensions, customHighlighting);      
    }

    public string Name
    {
      get { return m_name; }
    }

    public IFileViewModel Settings
    {
      get { return m_settings; }
      protected set
      {
        if (Equals(value, m_settings)) return;
        if (m_settings != null)
          m_settings.Saving -= SettingsOnSaving;
        m_settings = value;
        if (m_settings != null)
        m_settings.Saving += SettingsOnSaving;
        UpdateSettings();
        OnPropertyChanged();
      }
    }

    public UIElement View
    {
      get
      {
        if (m_view == null)
        {
          m_view = new BaseTextEditorView
          {
            DataContext = this,
          };
          m_view.SetValue(FoldingMargin.FoldingMarkerBackgroundBrushProperty, new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20)));
          m_view.SetValue(FoldingMargin.FoldingMarkerBrushProperty, new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80)));
          m_view.SetValue(FoldingMargin.SelectedFoldingMarkerBrushProperty, new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0)));
          m_view.SetValue(FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20)));
          m_caret = m_view.TextEditor.TextArea.Caret;
          Caret.PositionChanged += CaretOnPositionChanged;
          if (m_file != null && m_file.Path != null)
            m_view.TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(m_file.Path));
          m_view.TextEditor.TextArea.TextView.BackgroundRenderers.Add(new CaretLineBackGroundRender(m_view.TextEditor));
          m_textMarkingBackGroundRender = new TextMarkingBackGroundRender(m_view.TextEditor);
          m_view.TextEditor.TextArea.TextView.BackgroundRenderers.Add(m_textMarkingBackGroundRender);
          m_view.TextEditor.Options.IndentationSize = m_tabSize;
          m_view.TextEditor.Options.ConvertTabsToSpaces = m_convertTabsToSpaces;
          m_view.TextEditor.Document = m_textDocument;
          ICollection<CommandBinding> editCommandBindings = m_view.TextEditor.TextArea.DefaultInputHandler.Editing.CommandBindings;
          editCommandBindings.Remove(editCommandBindings.First(binding => binding.Command == AvalonEditCommands.IndentSelection));
          InitEditor();
        }
        return m_view;
      }
    }

    protected virtual void  InitEditor() {}

    public IFileViewModel File
    {
      get { return m_file; }
      set
      {
        if (m_file != null)
        {
          m_file.ContentChanged -= FileOnPropertyChanged;
          m_file.SetTag("offset", m_offset);
          m_file.SetTag("scroll", m_view.TextEditor.TextArea.TextView.ScrollOffset);
        }
        m_file = value;
        if (m_file != null)
        {
          string text = m_file.GetContent<string>() ?? "";
          if (m_convertTabsToSpaces)
            text = text.Replace("\t", m_indentString);
          InitFromFile(text);
          m_file.ContentChanged += FileOnPropertyChanged;
          if (m_caret != null)
            m_caret.Offset = (int) (m_file.GetTag("offset") ?? 0);
          if (m_view != null)
          {
            Vector scrollOffset = (Vector) (m_file.GetTag("scroll") ?? new Vector(0, 0));
            m_view.TextEditor.ScrollToHorizontalOffset(scrollOffset.X);
            m_view.TextEditor.ScrollToVerticalOffset(scrollOffset.Y);
            m_view.TextEditor.IsReadOnly = m_file.IsReadonly;
          }
        }
        FocusTag = EditFocusTag.Editor;
        OnPropertyChanged();
      }
    }

    protected virtual void InitFromFile(string text)
    {
      m_textDocument.Text = text;
      m_textDocument.UndoStack.ClearAll();
      if (m_view != null && m_file != null && m_file.Path != null)
        m_view.TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(m_file.Path));
    }

    public IEnumerable<string> SupportedFiles
    {
      get { return m_supportedFiles; }
    }

    public TextDocument TextDocument
    {
      get { return m_textDocument; }
    }

    public int LineNumber
    {
      get { return m_lineNumber; }
      private set
      {
        if (value == m_lineNumber) return;
        m_lineNumber = value;
        OnPropertyChanged();
      }
    }

    public int CharNumber
    {
      get { return m_charNumber; }
      private set
      {
        if (value == m_charNumber) return;
        m_charNumber = value;
        OnPropertyChanged();
      }
    }

    public Caret Caret
    {
      get { return m_caret; }
    }

    public string SearchString
    {
      get { return m_searchString; }
      set
      {
        if (value == m_searchString) return;
        m_searchString = value;
        m_findIndex = -1;
        UpdateMarkedText();
        OnPropertyChanged();
      }
    }

    public bool IsFindVisible
    {
      get { return m_isFindVisible; }
      set
      {
        if (value.Equals(m_isFindVisible)) return;
        m_isFindVisible = value;
        if (!m_isFindVisible)
          FocusTag = EditFocusTag.Editor;
        UpdateMarkedText();
        OnPropertyChanged();
      }
    }

    public List<KeyBinding> KeyBindings
    {
      get { return m_keyBindings; }
      set
      {
        if (Equals(value, m_keyBindings)) return;
        m_keyBindings = value;
        OnPropertyChanged();
      }
    }

    public EditFocusTag FocusTag
    {
      get { return m_focusTag; }
      set
      {
        m_focusTag = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand FindNextCommand
    {
      get { return m_findNextCommand; }
    }

    public string ReplaceString
    {
      get { return m_replaceString; }
      set
      {
        if (value == m_replaceString) return;
        m_replaceString = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand ReplaceCommand
    {
      get { return m_replaceCommand; }
    }

    public ManualCommand ReplaceAllCommand
    {
      get { return m_replaceAllCommand; }
    }

    public virtual UIElement ExtraInfoElement
    {
      get { return null; }
    }

    private bool SettingsOnSaving()
    {
      JsonException jsonException;
      JsonObject jsonObject = JsonHelperFunctions.Parse(m_settings.GetContent<string>(), out jsonException);
      if (jsonException == null)
      {
        if (m_settningsSchema != null)
        {
          if (m_settningsSchema.Validate(jsonObject, Path.GetDirectoryName(m_settings.Path)))
          {
            UpdateSettings();
            return true;
          }
        }
        else
          return true;
      }
      m_mainViewModel.DialogHelper.ShowMessageBox("Settings save error",
                                                  "In order to save setting, there must be no json or validation errors!");
      return false;
    }

    private void ReplaceAll()
    {
      if (string.IsNullOrEmpty(m_searchString) || !m_isFindVisible)
        return;
      Regex scheamRegex = new Regex(m_searchString, RegexOptions.IgnoreCase);
      MatchCollection matches = scheamRegex.Matches(m_text);
      int offset = 0;
      m_textDocument.UndoStack.StartUndoGroup();
      foreach (Match match in matches)
      {
        int lengthdiff = match.Length - m_replaceString.Length;
        m_textDocument.Replace(match.Index + offset, match.Length, m_replaceString);
        offset -= lengthdiff;
      }
      m_textDocument.UndoStack.EndUndoGroup();
      UpdateMarkedText();
    }

    private void Replace()
    {
      if (m_currentMatch == null)
        return;
      m_textDocument.Replace(m_currentMatch.Index, m_currentMatch.Length, m_replaceString ?? "");
      UpdateMarkedText();
    }

    private void UpdateSettings()
    {
      JsonException jsonException;
      JsonNode settings = JsonHelperFunctions.Parse(Settings.GetContent<string>(), out jsonException) as JsonNode;
      if (jsonException != null || settings == null)
        return;
      JsonArray keybindings = settings.GetObjectOrDefault<object>("shortcuts", null) as JsonArray;
      KeyBindings = keybindings != null ? KeyboardBindingGenerator.GenerateKeyBinding(keybindings, this).ToList() : new List<KeyBinding>();
      m_tabSize = settings.GetObjectOrDefault("tabsize", 2);
      m_convertTabsToSpaces = settings.GetObjectOrDefault("convertTabsToSpaces", false);
      m_indentString = m_convertTabsToSpaces ? new string(' ', m_tabSize) : "\t";
      if (m_convertTabsToSpaces && m_text != null)
        m_textDocument.Text = m_text.Replace("\t", m_indentString);
      if (m_view != null)
      {
        m_view.TextEditor.Options.IndentationSize = m_tabSize;
        m_view.TextEditor.Options.ConvertTabsToSpaces = m_convertTabsToSpaces;
      }
    }

    private void TextDocumentOnTextChanged(object sender, EventArgs eventArgs)
    {
      m_text = m_textDocument.Text;
      if (m_file != null)
        m_file.SetContent(m_text);

      UpdateMarkedText();
      TextChanged();

    }

    protected virtual void TextChanged() {}

    protected virtual Schema GetSettingsSchema(SchemaManager schemaManager)
    {
      return new Schema(Properties.Resources.basetexteditorsettings_schema, schemaManager);
    }

    protected virtual IFileViewModel GetSettings()
    {
      if (!System.IO.File.Exists(Properties.Settings.Default.SettingPath + "\\basetesteditor.settings.json"))
        System.IO.File.WriteAllBytes(Properties.Settings.Default.SettingPath + "\\basetesteditor.settings.json", Properties.Resources.basetesteditor_settings);
      return new FileViewModel(Properties.Settings.Default.SettingPath + "\\basetesteditor.settings.json");
    }

    private void FindNext()
    {
      if (m_findIndex < 0)
        m_findIndex = m_offset;
      UpdateMarkedText();
    }

    private void CaretOnPositionChanged(object sender, EventArgs eventArgs)
    {
      m_offset = m_caret.Offset;
      LineNumber = Caret.Line;
      CharNumber = Caret.Column;
      OffsetChanged();
    }

    protected virtual void OffsetChanged() {}

    private void UpdateMarkedText()
    {
      List<TextBlock> textBlocks = new List<TextBlock>();
      if (!string.IsNullOrEmpty(m_searchString) && m_isFindVisible)
      {
        Regex scheamRegex;
        try
        {
          scheamRegex = new Regex(m_searchString, RegexOptions.IgnoreCase);
        }
        catch (Exception)
        {
         return;
        }
        MatchCollection matches = scheamRegex.Matches(m_text);
        m_currentMatch = null;
        if (m_findIndex > 0)
        {
          foreach (Match match in matches)
          {
            if (match.Index > m_findIndex)
            {
              m_currentMatch = match;
              break;
            }
          }
          if (m_currentMatch == null && matches.Count > 0)
            m_currentMatch = matches[0];

          if (m_currentMatch != null)
          {
            m_findIndex = m_currentMatch.Index;
            m_view.TextEditor.ScrollToLine(m_textDocument.GetLineByOffset(m_findIndex).LineNumber);
          }
        }
        foreach (Match match in matches)
          textBlocks.Add(new TextBlock(match.Index, match.Index + match.Length, m_currentMatch == match ? Colors.BurlyWood : Colors.Transparent, Colors.White));
      }
      if (m_view != null)
        m_textMarkingBackGroundRender.TextBlocks = textBlocks;
    }

    private void FileOnPropertyChanged(IFileViewModel file)
    {
        if (m_file.GetContent<string>() != m_text)
        {
          string text = m_file.GetContent<string>();
          if (m_convertTabsToSpaces)
            text = text.Replace("\t", m_indentString);
          FileChangedExternaley(text);
          m_textDocument.Text = text;
        }
    }

    protected virtual void FileChangedExternaley(string text) {}

    public void ShowFind()
    {
      IsFindVisible = true;
      FocusTag = EditFocusTag.Find;
    }

    public void HideFind()
    {
      IsFindVisible = false;
    }

    public virtual event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
      m_disposed = true;
    }
  }
}