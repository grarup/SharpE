using System.Collections.Generic;

namespace SharpE.MvvmTools.Helpers
{
  public class MouseActionCollection
  {
    private readonly List<MouseAction> m_actions = new List<MouseAction>();

    public List<MouseAction> Actions
    {
      get { return m_actions; }
    }
  }
}
