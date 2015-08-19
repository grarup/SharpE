using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Properties;

namespace SharpE.BaseEditors.Json.ViewModels
{
  class LineDescriptionViewModel : INotifyPropertyChanged
  {
    private readonly int m_index;
    private string m_text;
    private bool m_isCurrentLine;
    private ValidationErrorState m_lineState;
    private Brush m_brush = Brushes.Green;
    private string m_errorMessage;
    private int m_errorIndex;
    private bool m_isVisiable = true;

    public event PropertyChangedEventHandler PropertyChanged;

    public LineDescriptionViewModel(int index)
    {
      m_index = index;
    }

    public string Text
    {
      get { return m_text; }
      set
      {
        if (value == m_text) return;
        m_text = value;
        OnPropertyChanged();
      }
    }

    public int Index
    {
      get { return m_index; }
    }

    public bool IsCurrentLine
    {
      get { return m_isCurrentLine; }
      set
      {
        if (value.Equals(m_isCurrentLine)) return;
        m_isCurrentLine = value;
        OnPropertyChanged();
      }
    }

    public ValidationErrorState LineState
    {
      get { return m_lineState; }
      set
      {
        if (value == m_lineState) return;
        m_lineState = value;
        if ((m_lineState & ValidationErrorState.NotCorrectJson) != ValidationErrorState.Good)
          Brush = Brushes.Red;
        else if ((m_lineState & ValidationErrorState.NotInSchema) != ValidationErrorState.Good)
        {
          Brush = Brushes.Purple;
          if (ErrorMessage == null) 
            ErrorMessage = "Not in templet.";
        }
        else if ((m_lineState & ValidationErrorState.WrongData) != ValidationErrorState.Good)
        {
          Brush = Brushes.DarkOrange;
          if (ErrorMessage == null) 
            ErrorMessage = "Datatype is wrong.";
        }
        else if ((m_lineState & ValidationErrorState.ToMany) != ValidationErrorState.Good)
        {
          Brush = Brushes.Fuchsia;
          if (ErrorMessage == null) 
            ErrorMessage = "Max count for this element is excided.";
        }
        else if ((m_lineState & ValidationErrorState.MissingChild) != ValidationErrorState.Good)
        {
          Brush = Brushes.Goldenrod;
          if (ErrorMessage == null) 
            ErrorMessage = "Missing a child.";
        }
        else if ((m_lineState & ValidationErrorState.Unknown) != ValidationErrorState.Good)
        {
          Brush = Brushes.DodgerBlue;
          if (ErrorMessage == null) 
            ErrorMessage = "Status of this line is unknown do to error earlier.";
        }
        else
        {
          Brush = Brushes.Green;
          ErrorIndex = -1;
          ErrorMessage = null;
        }
        OnPropertyChanged();
      }
    }

    public Brush Brush
    {
      get { return m_brush; }
      private set
      {
        if (Equals(value, m_brush)) return;
        m_brush = value;
        OnPropertyChanged();
      }
    }

    public string ErrorMessage
    {
      get { return m_errorMessage; }
      set
      {
        if (value == m_errorMessage) return;
        m_errorMessage = value;
        OnPropertyChanged();
      }
    }

    public int ErrorIndex
    {
      get { return m_errorIndex; }
      set
      {
        if (value == m_errorIndex) return;
        m_errorIndex = value;
        OnPropertyChanged();
      }
    }

    public bool IsVisiable
    {
      get { return m_isVisiable; }
      set
      {
        if (value.Equals(m_isVisiable)) return;
        m_isVisiable = value;
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
