using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.Views.Layout;

namespace SharpE.ViewModels.Layout
{
  public class LayoutManager : BaseViewModel
  {
    private LayoutType m_selectedLayoutType = LayoutType.ThreeColumns;
    private IObservableCollection<LayoutElementViewModel> m_layoutElements = new ObservableCollection<LayoutElementViewModel>();
    private LayoutElementViewModel m_activeLayoutElement;
    private readonly MainViewModel m_mainViewModel;
    private readonly List<LayoutType> m_layoutTypes = new List<LayoutType> { LayoutType.Single, LayoutType.TwoColumns, LayoutType.ThreeColumns, LayoutType.TwoRows, LayoutType.ThreeRows, LayoutType.Grid };
    private readonly GenericManualCommand<LayoutType> m_selectLayoutTypeCommand;

    public LayoutManager(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_selectLayoutTypeCommand = new GenericManualCommand<LayoutType>(layoutType => SelectedLayoutType = layoutType);
      CrateView();
    }

    private void CrateView()
    {
      switch (m_selectedLayoutType)
      {
        case LayoutType.Single:
          m_view = new SingleLayoutView {DataContext = this};
          break;
        case LayoutType.TwoColumns:
          m_view = new TwoColumnsView {DataContext = this};
          break;
        case LayoutType.ThreeColumns:
          m_view = new ThreeColumnsView { DataContext = this };
          break;
        case LayoutType.TwoRows:
          m_view = new TwoRowsView { DataContext = this };
          break;
        case LayoutType.ThreeRows:
          m_view = new ThreeRowsView { DataContext = this };
          break;
        case LayoutType.Grid:
          m_view = new GridView { DataContext = this };
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      while (m_layoutElements.Count > m_selectedLayoutType.ElementCount())
      {
        foreach (IFileViewModel fileViewModel in m_layoutElements.Last().OpenFiles)
        {
          m_layoutElements[m_selectedLayoutType.ElementCount() - 1].OpenFiles.Add(fileViewModel);
          m_layoutElements[m_selectedLayoutType.ElementCount() - 1].FileUseOrder.Add(fileViewModel);
        }
        m_layoutElements.Remove(m_layoutElements.Last());
      }
      
      int index = m_layoutElements.Count;
      while (m_layoutElements.Count < m_selectedLayoutType.ElementCount())
        m_layoutElements.Add(new LayoutElementViewModel(m_mainViewModel, index++));
      
      if (m_activeLayoutElement == null || !m_layoutElements.Contains(m_activeLayoutElement))
        m_activeLayoutElement = m_layoutElements.FirstOrDefault();

      OnPropertyChanged("View");
    }

    public LayoutType SelectedLayoutType
    {
      get { return m_selectedLayoutType; }
      set
      {
        if (value == m_selectedLayoutType) return;
        m_selectedLayoutType = value;
        CrateView();
        OnPropertyChanged();
      }
    }

    public IObservableCollection<LayoutElementViewModel> LayoutElements
    {
      get { return m_layoutElements; }
      set
      {
        if (Equals(value, m_layoutElements)) return;
        m_layoutElements = value;
        OnPropertyChanged();
      }
    }

    public LayoutElementViewModel ActiveLayoutElement
    {
      get { return m_activeLayoutElement; }
      set
      {
        if (Equals(value, m_activeLayoutElement)) return;
        m_activeLayoutElement = value;
        OnPropertyChanged();
      }
    }

    public Action<object> CurrentFocusTag
    {
      get { return Action; }
    }

    public List<LayoutType> LayoutTypes
    {
      get { return m_layoutTypes; }
    }

    public GenericManualCommand<LayoutType> SelectLayoutTypeCommand
    {
      get { return m_selectLayoutTypeCommand; }
    }

    private void Action(object o)
    {
      int index = int.Parse((string) o);
      if (index > 0 && index < m_layoutElements.Count)
        ActiveLayoutElement = m_layoutElements[index];
    }

    public void SaveSetup()
    {
      JsonNode root = new JsonNode {{"layouttype", m_selectedLayoutType}};
      JsonArray layouts = new JsonArray();
      foreach (LayoutElementViewModel editorLayoutViewModel in m_layoutElements)
      {
        JsonNode layout = new JsonNode();
        JsonArray openFiles = new JsonArray();
        foreach (IFileViewModel fileViewModel in editorLayoutViewModel.OpenFiles.Where(n => n.Path != null))
          openFiles.Add(fileViewModel.Path);
        layout.Add(new JsonElement("openfiles", openFiles));
        if (editorLayoutViewModel.SelectedFile != null && editorLayoutViewModel.SelectedFile.Path != null)
          layout.Add(new JsonElement("selectedFile", editorLayoutViewModel.SelectedFile.Path));
        layout.Add("isactive", editorLayoutViewModel == m_activeLayoutElement);
        layouts.Add(layout);
      }
      root.Add("layouts", layouts);
      StreamWriter streamWriter = File.CreateText(Properties.Settings.Default.SettingPath + "\\openFiles.json");
      streamWriter.Write(root.ToString());
      streamWriter.Close();
    }

    public void LoadSetup()
    {
      if (File.Exists(Properties.Settings.Default.SettingPath + "\\openFiles.json"))
      {
        StreamReader streamReader = File.OpenText(Properties.Settings.Default.SettingPath + "\\openFiles.json");
        JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(streamReader.ReadToEnd());
        streamReader.Close();
        SelectedLayoutType = jsonNode.GetObjectOrDefault("layouttype", LayoutType.Single);
        JsonArray layouts = jsonNode.GetObjectOrDefault<JsonArray>("layouts", null);
        int index = 0;
        foreach (JsonNode layout in layouts)
        {
          LayoutElementViewModel layoutElement = m_layoutElements[index];
          JsonArray openFiles = layout.GetObjectOrDefault<JsonArray>("openfiles", null);
          string selectedFilePath = layout.GetObjectOrDefault<string>("selectedFile", null);
          foreach (JsonValue jsonValue in openFiles)
          {
            IFileViewModel fileViewModel = m_mainViewModel.OpenFile((string)jsonValue.Value, layoutElement,
              (string)jsonValue.Value == selectedFilePath);
            if (!layoutElement.FileUseOrder.Contains(fileViewModel))
              layoutElement.FileUseOrder.Add(fileViewModel);
          }
          if (layoutElement.SelectedFile == null && layoutElement.OpenFiles.Count > 0)
            layoutElement.SelectedFile = layoutElement.OpenFiles.First();
          if (layout.GetObjectOrDefault("isactive", false))
            m_activeLayoutElement = layoutElement;
          index++;
        }
      }
    }
  }
}
