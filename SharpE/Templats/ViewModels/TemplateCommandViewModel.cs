using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Properties;

namespace SharpE.Templats.ViewModels
{
  public class TemplateCommandViewModel : INotifyPropertyChanged
  {
    private readonly TemplateCommand m_templateCommand;
    private string m_commandString;
    private readonly IObservableCollection<string> m_errors = new AsyncObservableCollection<string>();
    private bool m_isSelectedToRun = true;
    private bool m_hasRun;
    private Brush m_statusBrush;

    public TemplateCommandViewModel(TemplateCommand templateCommand, IEnumerable<TemplateParameterViewModel> templateParameterViewModels)
    {
      m_templateCommand = templateCommand;
      m_templateCommand.CheckCommand();
      foreach (TemplateParameterViewModel templateParameterViewModel in m_templateCommand.UsedParameters.Select(n => templateParameterViewModels.FirstOrDefault(m => m.Name == n.Name)))
      {
        templateParameterViewModel.PropertyChanged += TemplateParameterViewModelOnPropertyChanged;
      }
      m_commandString = m_templateCommand.ToString();
      foreach (string error in m_templateCommand.Errors)
        Errors.Add(error);
      UpdateStatusBrush();
    }

    private void TemplateParameterViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      Update();
    }

    private void Update()
    {
      Errors.Clear();
      m_templateCommand.CheckCommand();
      foreach (string error in m_templateCommand.Errors)
        Errors.Add(error);
      CommandString = m_templateCommand.ToString();
      HasRun = false;
      UpdateStatusBrush();
    }

    private void UpdateStatusBrush()
    {
      StatusBrush = m_templateCommand.HasErrors
        ? (m_hasRun ? Brushes.Red : Brushes.Orange)
        : (m_hasRun ? Brushes.Green : Brushes.DimGray);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string CommandString
    {
      get { return m_commandString; }
      set
      {
        if (value == m_commandString) return;
        m_commandString = value;
        OnPropertyChanged();
      }
    }

    public void Run()
    {
      Errors.Clear();
      if (!m_templateCommand.Execute())
      {
        foreach (string error in m_templateCommand.Errors)
          Errors.Add(error);
      }
      HasRun = true;
    }

    public bool IsSelectedToRun
    {
      get { return m_isSelectedToRun; }
      set
      {
        if (value.Equals(m_isSelectedToRun)) return;
        m_isSelectedToRun = value;
        OnPropertyChanged();
      }
    }

    public bool HasRun
    {
      get { return m_hasRun; }
      set
      {
        if (value.Equals(m_hasRun)) return;
        m_hasRun = value;
        UpdateStatusBrush();
        OnPropertyChanged();
      }
    }

    public IObservableCollection<string> Errors
    {
      get { return m_errors; }
    }

    public Brush StatusBrush
    {
      get { return m_statusBrush; }
      set
      {
        if (Equals(value, m_statusBrush)) return;
        m_statusBrush = value;
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
