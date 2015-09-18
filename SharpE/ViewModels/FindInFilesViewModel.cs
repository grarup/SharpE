using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Exstensions;
using SharpE.MvvmTools.Helpers;
using SharpE.Views;

namespace SharpE.ViewModels
{
  public class FindInFilesViewModel : BaseViewModel, IFileViewModel, IEditor
  {
    private readonly MainViewModel m_mainViewModel;
    private string m_searchString;
    private StringBuilder m_result;
    private ITreeNode m_treeNode;
    private TextEditor m_editor;
    private readonly ManualCommand m_findCommand;
    private readonly ManualCommand m_jumpCommand;
    private IFileViewModel m_settings;
    private IFileViewModel m_file;
    private readonly IEnumerable<string> m_supportedFiles = new[] { "searchresult" };
    private CancellationTokenSource m_cancellationTokenSource;
    private readonly List<Task> m_tasks = new List<Task>();
    private readonly List<string> m_extenisionToIgnore = new List<string> {".qb", ".png"};
    private string m_searchFile;

    public FindInFilesViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_findCommand = new ManualCommand(Find);
      m_jumpCommand = new ManualCommand(Jump);
    }

    private void Jump()
    {
      DocumentLine line = m_editor.Document.GetLineByOffset(m_editor.CaretOffset);
      while (!m_editor.Document.GetText(line).StartsWith("File") && line.LineNumber > 0)
        line = line.PreviousLine;
      m_mainViewModel.OpenFile(m_editor.Document.GetText(line).Substring(6));
    }

    protected override FrameworkElement GenerateView()
    {
      FindInFilesView findInFilesView = new FindInFilesView { DataContext = this };
      m_editor = findInFilesView.TextEditor;
      m_editor.TextArea.MouseDoubleClick += TextAreaOnMouseDoubleClick;
      return findInFilesView;
    }

    private void TextAreaOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      Jump();
    }

    public string SearchString
    {
      get { return m_searchString; }
      set
      {
        if (value == m_searchString) return;
        m_searchString = value;
        OnPropertyChanged();
      }
    }

    public ITreeNode TreeNode
    {
      get { return m_treeNode; }
      set
      {
        if (Equals(value, m_treeNode)) return;
        m_treeNode = value;
        OnPropertyChanged();
      }
    }

    public void Find()
    {
      if (m_cancellationTokenSource != null)
      {
        m_cancellationTokenSource.Cancel();
        while (m_cancellationTokenSource != null)
          Thread.Sleep(10);
      }
      UpdateText("");
      m_cancellationTokenSource = new CancellationTokenSource();
      m_result = new StringBuilder();
      Find(m_treeNode, m_searchString, m_cancellationTokenSource.Token);
      Task.Factory.StartNew(() =>
      {
        Task.WaitAll(m_tasks.ToArray());
        m_tasks.Clear();
        m_cancellationTokenSource = null;
        SearchFile = "";
        UpdateText(m_result.ToString());
      });
    }

    private void Find(ITreeNode root, string searchstring, CancellationToken token)
    {
      IFileViewModel fileViewModel = root as IFileViewModel;
      if (fileViewModel != null)
      {
        if (m_extenisionToIgnore.Contains(fileViewModel.Exstension))
          return;
        m_tasks.Add(Task.Factory.StartNew(() => FindInFile(fileViewModel, searchstring, token)));
        return;
      }
      foreach (ITreeNode treeNode in root.Children)
      {
        Find(treeNode, searchstring, token);
      }
    }

    private void FindInFile(IFileViewModel fileViewModel, string searchstring, CancellationToken token)
    {
      if (token.IsCancellationRequested)
        return;
      SearchFile = fileViewModel.Path;
      string text;
      try
      {
        text = fileViewModel.GetContent<string>();
        if (!fileViewModel.HasUnsavedChanges)
          fileViewModel.Reset();
      }
      catch (Exception)
      {
        return;
      }

      Regex scheamRegex;
      try
      {
        scheamRegex = new Regex(searchstring, RegexOptions.IgnoreCase);
      }
      catch (Exception)
      {
        return;
      }

      MatchCollection matches = scheamRegex.Matches(text);
      if (matches.Count == 0)
        return;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("File: ");
      stringBuilder.AppendLine(fileViewModel.Path);

      foreach (Match match in matches)
      {
        if (token.IsCancellationRequested)
          return;
        stringBuilder.Append("   ");
        stringBuilder.Append(text.LineNumber(match.Index));
        stringBuilder.Append(" : ");
        stringBuilder.AppendLine(text.Line(match.Index));
      }
      stringBuilder.AppendLine();
      lock (m_result)
      {
        m_result.Append(stringBuilder);
        UpdateText(m_result.ToString());
      }
    }

    private void UpdateText(string text)
    {
      if (!m_view.Dispatcher.CheckAccess())
      {
        m_view.Dispatcher.Invoke(new Action(() => UpdateText(text)));
        return;
      }

      m_editor.Document.Text = text;
    }

    public void Cancel()
    {
      m_cancellationTokenSource.Cancel();
    }

    public bool HasUnsavedChanges
    {
      get { return false; }
      set { }
    }

    public bool IsExpanded
    {
      get { return false; }
      set { }
    }

    public void Show()
    {

    }

    public string Name
    {
      get { return "Find in files"; }
      set { }
    }

    public IFileViewModel Settings
    {
      get { return m_settings; }
    }

    public UIElement View
    {
      get { return base.View; }
    }

    public IFileViewModel File
    {
      get { return m_file; }
      set { m_file = value; }
    }

    public IEnumerable<string> SupportedFiles
    {
      get { return m_supportedFiles; }
    }

    public string RenameString
    {
      get { return "Find in files"; }
      set { }
    }

    public string Path
    {
      get { return null; }
    }

    public bool IsRenaming
    {
      get { return false; }
      set { }
    }

    public ITreeNode Parent
    {
      get { return null; }
    }

    public IObservableCollection<ITreeNode> Children
    {
      get { return null; }
    }

    public IFileViewModel GetFile(string path)
    {
      return null;
    }

    public void Delete()
    {

    }

    public string GetParameter(string key)
    {
      return null;
    }

    public event Func<bool> Saving;
    public T GetContent<T>()
    {
      return default(T);
    }

    public void SetContent<T>(T content)
    {

    }

    public string Exstension
    {
      get { return "searchresult"; }
    }

    public bool IsReadonly
    {
      get { return true; }
    }

    public ValidationState ValidationState
    {
      get { return ValidationState.Undefined; }
      set { }
    }

    public ManualCommand FindCommand
    {
      get { return m_findCommand; }
    }

    public string SearchFile
    {
      get { return m_searchFile; }
      set
      {
        if (value == m_searchFile) return;
        m_searchFile = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand JumpCommand
    {
      get { return m_jumpCommand; }
    }

    public void Save()
    {
    }

    public void SetTag(string key, object tag)
    {

    }

    public object GetTag(string key)
    {
      return null;
    }

    public event Action<IFileViewModel> ContentChanged;
    public event Action<IFileViewModel> FileChangedOnDisk;
    public bool IsBasedOnCurrentFile()
    {
      return true;
    }

    public void Reset()
    {
      m_editor.Document.Text = "";
    }
  }
}
