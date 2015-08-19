using System;
using System.Windows;
using System.Windows.Media;

namespace SharpE.MvvmTools.Controls
{
  public interface IItemRender
  {
    double ItemHeight { get; }
    void Render(DrawingContext drawingContext, Point position, Object item);
  }
}
