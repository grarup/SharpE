using System;

namespace SharpE.QubicleViewer
{
  [Flags]
  public enum QubeSideMask
  {
    Invisble,
    Left = 0x02,
    Right = 0x04,
    Top = 0x08,
    Bottom = 0x10,
    Front = 0x20,
    Back = 0x40,
    All = 0x7E
  }

  internal static class QubeSideMaskExstension
  {
    public static bool Contains(this QubeSideMask qubeSideMask, QubeSideMask testMask)
    {
      return (qubeSideMask & testMask) == testMask;
    }
  }

}
