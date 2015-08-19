using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SharpE.Definitions.Collection;
using SharpE.Json.AutoComplet;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.MvvmTools.Properties;

namespace SharpE.Templats.ViewModels
{
  public class TemplateParameterViewModel : INotifyPropertyChanged
  {
    private readonly TemplateParameter m_templateParameter;
    private readonly Template m_template;
    private readonly AsyncObservableCollection<string> m_autoCompletValues = new AsyncObservableCollection<string>();
    private bool m_hasFocus;
    private Task m_autoCompleteTask;
    private bool m_updateAutoComplete;
    private int m_autoCompleteIndex;
    private readonly ICommand m_selectAutoCompleteCommand;
    private readonly ICommand m_nextAutoCompleteCommand;
    private readonly ICommand m_prevAutoCompleteCommand;
    private int m_caretIndex;
    private readonly IAutoCompleteCollectionManager m_autoCompleteCollectionManager;
    private bool m_showAutoComplete;

    public TemplateParameterViewModel(TemplateParameter templateParameter, Template template)
    {
      m_templateParameter = templateParameter;
      m_template = template;

      m_selectAutoCompleteCommand = new ManualCommand(SelectAutoComplete);
      m_nextAutoCompleteCommand = new ManualCommand(NextAutoComplete);
      m_prevAutoCompleteCommand = new ManualCommand(PrevAutoComplete);

      if (m_templateParameter.Key != null)
        m_autoCompleteCollectionManager = ServiceProvider.Resolve<IAutoCompleteCollectionManager>();
    }

    private void PrevAutoComplete()
    {
      if (m_autoCompletValues.Count == 0) return;
      if (m_autoCompleteIndex == 0)
        AutoCompleteIndex = m_autoCompletValues.Count - 1;
      else
        AutoCompleteIndex--;
    }

    private void NextAutoComplete()
    {
      if (m_autoCompletValues.Count == 0) return;
      if (m_autoCompleteIndex < m_autoCompletValues.Count - 1)
        AutoCompleteIndex++;
      else
        AutoCompleteIndex = 0;
    }

    private void SelectAutoComplete()
    {
      if (m_autoCompleteIndex < 0 && m_autoCompletValues != null && m_autoCompletValues.Count > 0)
        AutoCompleteIndex = 0;
      if (m_autoCompletValues == null || m_autoCompleteIndex >= m_autoCompletValues.Count) return;
      switch (m_templateParameter.Type)
      {
        case TemplateParameterType.Undefined:
          break; 
        case TemplateParameterType.String:
          Value = m_autoCompletValues[m_autoCompleteIndex];
          CaretIndex = Value.Length;
          break;
        case TemplateParameterType.File:
          string value = (Value ?? "").Replace(m_templateParameter.PathSeparator, '\\');
          if (m_autoCompletValues[m_autoCompleteIndex] == "..")
          {
            int index = value.LastIndexOf('\\');
            if (index == -1)
              index = 0;
            else
            {
              index = value.LastIndexOf('\\', index - 1);
              if (index == -1)
                index = 0;
              else
                index++;
            }
            Value = value.Substring(0, index).Replace('\\', m_templateParameter.PathSeparator);
          }
          else
          {
            int index = value.LastIndexOf('\\');
            if (index == -1)
              index = 0;
            else
              index++;
            Value = (value.Substring(0, index) + m_autoCompletValues[m_autoCompleteIndex]).Replace('\\', m_templateParameter.PathSeparator); ;
          }
          CaretIndex = Value.Length;
          break;
        case TemplateParameterType.Folder:
          break;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
      get { return m_templateParameter.Name; }
    }

    public bool IsEditable { get { return m_templateParameter.IsEditable; } }

    public string Value
    {
      get { return m_templateParameter.Value; }
      set
      {
        if (value == m_templateParameter.Value) return;
        m_templateParameter.Value = value;
        m_updateAutoComplete = true;
        OnPropertyChanged();
      }
    }

    public string Description
    {
      get { return m_templateParameter.Description; }
    }

    public TemplateParameterType Type
    {
      get { return m_templateParameter.Type; }
    }

    public bool HasFocus
    {
      get { return m_hasFocus; }
      set
      {
        if (value.Equals(m_hasFocus)) return;
        m_hasFocus = value;
        ShowAutoComplete &= m_hasFocus;
        if (m_hasFocus && (Type != TemplateParameterType.Undefined))
        {
          m_updateAutoComplete = true;
          m_autoCompleteTask = new Task(UpdateAutoComplet);
          m_autoCompleteTask.Start();
        }
        OnPropertyChanged();
      }
    }

    private void UpdateAutoComplet()
    {
      while (m_hasFocus)
      {
        if (m_updateAutoComplete)
        {
          m_updateAutoComplete = false;
          switch (Type)
          {
            case TemplateParameterType.Undefined:
              break;
            case TemplateParameterType.String:
            {
              if (m_templateParameter.Key == null)
                m_autoCompletValues.Clear();
              else
              {
                m_autoCompletValues.Clear();
                m_autoCompletValues.AddRange(
                  m_autoCompleteCollectionManager.GetAutoCompleteValues(m_templateParameter.Key)
                    .Where(n => Value == null || n.StartsWith(Value)));
              }
            }
              break;
            case TemplateParameterType.File:
            {
              List<string> autocompletList = new List<string>();
              string value = (Value ?? "").Replace(m_templateParameter.PathSeparator, '\\');
              int index = value.LastIndexOf('\\');
              if (index == -1)
                index = 0;
              else
                index++;
              value = value.Substring(0, index);
              string folderpath = value;
              if (Directory.Exists(folderpath))
              {
                autocompletList.Add("..");
                foreach (string directory in Directory.GetDirectories(folderpath))
                  autocompletList.Add(Path.GetFileName(directory) + "\\");
                foreach (string file in Directory.GetFiles(folderpath))
                {
                  if (file.StartsWith(value))
                    autocompletList.Add(Path.GetFileName(file));
                }
              }
              m_autoCompletValues.Clear();
              m_autoCompletValues.AddRange(autocompletList.Select(n => n.Replace('\\', m_templateParameter.PathSeparator)));
            }
              break;
            case TemplateParameterType.Folder:
              m_autoCompletValues.Clear();
              break;
            default:
              m_autoCompletValues.Clear();
              break;
          }
          ShowAutoComplete = m_autoCompletValues.Count > 0 &&
                             !(m_autoCompletValues.Count == 1 && m_autoCompletValues[0] == Value);
        }
        else
        {
          Thread.Sleep(10);
        }
      }
    }

    public IObservableCollection<string> AutoCompletValues
    {
      get { return m_autoCompletValues; }
    }

    public int AutoCompleteIndex
    {
      get { return m_autoCompleteIndex; }
      set
      {
        if (value == m_autoCompleteIndex) return;
        m_autoCompleteIndex = value;
        OnPropertyChanged();
      }
    }

    public ICommand SelectAutoCompleteCommand
    {
      get { return m_selectAutoCompleteCommand; }
    }

    public ICommand NextAutoCompleteCommand
    {
      get { return m_nextAutoCompleteCommand; }
    }

    public ICommand PrevAutoCompleteCommand
    {
      get { return m_prevAutoCompleteCommand; }
    }

    public Action<bool> HasFocusAction
    {
      get { return hasFocus => HasFocus = hasFocus; }
    }

    public int CaretIndex
    {
      get { return m_caretIndex; }
      set
      {
        if (value == m_caretIndex) return;
        m_caretIndex = value;
        OnPropertyChanged();
      }
    }

    public bool ShowAutoComplete
    {
      get { return m_showAutoComplete; }
      set
      {
        if (value.Equals(m_showAutoComplete)) return;
        m_showAutoComplete = value;
        OnPropertyChanged();
      }
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
