using System;

namespace SharpE.ViewModels.Layout
{
  public enum LayoutType
  {
    Undefined,
    Single,
    TwoColumns,
    ThreeColumns,
    TwoRows,
    ThreeRows,
    Grid,
  }

  public static class LayoutTypeExstensions
  {
    public static int ElementCount(this LayoutType layoutType)
    {
      switch (layoutType)
      {
        case LayoutType.Single:
          return 1;
        case LayoutType.TwoColumns:
        case LayoutType.TwoRows:
          return 2;
        case LayoutType.ThreeColumns:
        case LayoutType.ThreeRows:
          return 3;
        case LayoutType.Grid:
          return 4;
        default:
          throw new ArgumentOutOfRangeException("layoutType");
      }
    }
  }
}
