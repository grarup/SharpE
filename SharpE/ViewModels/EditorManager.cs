using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpE.BaseEditors.BaseTextEditor;
using SharpE.BaseEditors.Image;
using SharpE.BaseEditors.Json.ViewModels;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Collections;

namespace SharpE.ViewModels
{
  public class EditorManager
  {
    private readonly IFileViewModel m_setting;
    private readonly IObservableCollection<IEditor> m_baseEditors = new ObservableCollection<IEditor>();
    private readonly IObservableCollection<IEditor> m_editorsWithSettings;
    private readonly IObservableCollection<IEditor> m_usedEditors = new ObservableCollection<IEditor>();
    private readonly List<IEditor> m_allEditors = new List<IEditor>();
    private readonly IEditor m_simpleEditor;
    private readonly JsonEditorViewModel m_jsonEditorViewModel;
    private readonly ImageViewerViewModel m_imageViewerViewModel;
    private readonly FindInFilesViewModel m_findInFilesViewModel;

    public EditorManager(IFileViewModel setting, MainViewModel mainViewModel)
    {
      m_editorsWithSettings = new FilteredObservableCollection<IEditor>(m_baseEditors, editor => editor.Settings != null);
      m_setting = setting;
      m_imageViewerViewModel = new ImageViewerViewModel();
      m_jsonEditorViewModel = new JsonEditorViewModel(mainViewModel);
      m_simpleEditor = new BaseTextEditorViewModel(mainViewModel);
      m_findInFilesViewModel = new FindInFilesViewModel(mainViewModel);

      UpdateSettings();
      m_setting.ContentChanged += SettingOnContentChanged;
    }

    private void SettingOnContentChanged(IFileViewModel fileViewModel)
    {
      UpdateSettings();
    }

    private void UpdateSettings()
    {
      m_baseEditors.Clear();
      m_baseEditors.Add(m_imageViewerViewModel);
      m_baseEditors.Add(m_jsonEditorViewModel);
      m_baseEditors.Add(m_findInFilesViewModel);
      m_baseEditors.Add(m_simpleEditor);

      JsonException jsonException;
      JsonNode jsonNode = (JsonNode) JsonHelperFunctions.Parse(m_setting.GetContent<string>(), out jsonException);
      if (jsonNode == null || jsonException != null)
        return;
      JsonArray jsonArray = jsonNode.GetObjectOrDefault<JsonArray>("editors", null);
      if (jsonArray == null) return;
      foreach (JsonValue path in jsonArray)
      {
        IEditor editor = LoadEditor((string) path.Value);
        if (editor != null)
          m_baseEditors.Add(editor);
      }
    }

    public IObservableCollection<IEditor> BaseEditors
    {
      get { return m_baseEditors; }
    }

    public FindInFilesViewModel FindInFilesViewModel
    {
      get { return m_findInFilesViewModel; }
    }

    public IObservableCollection<IEditor> EditorsWithSettings
    {
      get { return m_editorsWithSettings; }
    }

    public IEditor GetEditor(string fileExstension)
    {
      return m_baseEditors.FirstOrDefault(n => n.SupportedFiles.Contains(fileExstension)) ?? m_simpleEditor;
    }

    private IEditor LoadEditor(string path)
    {
      try
      {
        Assembly assembly = Assembly.LoadFile(Environment.CurrentDirectory + "\\" + path);
        Type interfaceType = typeof (IEditorCreator);
        Type type = assembly.GetTypes().FirstOrDefault(interfaceType.IsAssignableFrom);
        if (type == null)
          return null;
        IEditorCreator editorCreator = Activator.CreateInstance(type) as IEditorCreator;
        if (editorCreator != null)
          return editorCreator.CreateEditor();
      }
// ReSharper disable once EmptyGeneralCatchClause (dont load editors that doesn't work)
      catch (Exception)
      {

      }
      return null;
    }

  }
}
