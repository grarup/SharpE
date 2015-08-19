using SharpE.Json.Data;

namespace SharpE.Json.Schemas
{
  public class ValidationError
  {
    private readonly ValidationErrorState m_errorState;
    private readonly int m_lineIndex;
    private readonly int m_charIndex;
    private readonly string m_message;

    public ValidationError(ValidationErrorState errorState, IDocPosition docPosition, string message)
    {
      m_errorState = errorState;
      if (docPosition != null)
      {
        m_lineIndex = docPosition.LineIndex;
        m_charIndex = docPosition.CharIndex;
      }
      m_message = message;
    }

    public ValidationError(ValidationErrorState errorState, int lineIndex, int charIndex, string message)
    {
      m_errorState = errorState;
      m_lineIndex = lineIndex;
      m_charIndex = charIndex;
      m_message = message;
    }

    public ValidationErrorState ErrorState
    {
      get { return m_errorState; }
    }

    public int LineIndex
    {
      get { return m_lineIndex; }
    }

    public int CharIndex
    {
      get { return m_charIndex; }
    }

    public string Message
    {
      get { return m_message; }
    }
  }
}