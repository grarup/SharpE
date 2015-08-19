using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using SharpE.Json.Data;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Properties;

namespace SharpE.BaseEditors.AvalonTextEditorAddons
{
  public class ErrorMargin : AbstractMargin, IWeakEventListener, INotifyPropertyChanged
  {
    #region decleration
    private Dictionary<int, List<ValidationError>> m_errors;
    private JsonException m_jsonException;
    private int m_tooltipLine;
    private bool m_hasSchema;
    int m_maxLineNumberLength = 1;

    #endregion

    #region constructor
    static ErrorMargin()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorMargin),
                                               new FrameworkPropertyMetadata(typeof(ErrorMargin)));
    }
    #endregion

    #region public properties
    public Dictionary<int, List<ValidationError>> Errors
    {
      get { return m_errors; }
      set
      {
        m_errors = value;
        if (Dispatcher.CheckAccess())
          InvalidateVisual();
        else
          Dispatcher.Invoke(InvalidateVisual);
      }
    }

    public JsonException JsonException
    {
      get { return m_jsonException; }
      set
      {
        m_jsonException = value;

        if (Dispatcher.CheckAccess())
          InvalidateVisual();
        else
          Dispatcher.Invoke(InvalidateVisual);
      }
    }

    public int TooltipLine
    {
      get { return m_tooltipLine; }
      set
      {
        if (value == m_tooltipLine) return;
        m_tooltipLine = value;
        OnPropertyChanged();
      }
    }

    public bool HasSchema
    {
      get { return m_hasSchema; }
      set
      {
        m_hasSchema = value;
      }
    }
    #endregion

    #region overrides
    protected override Size MeasureOverride(Size availableSize)
    {
      return new Size(12, 0);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      TextView textView = TextView;

      if (textView != null && textView.VisualLinesValid)
      {
        foreach (VisualLine line in textView.VisualLines)
        {
          int lineNumber = line.FirstDocumentLine.LineNumber - 1;
          List<Brush> brushes = new List<Brush>();
          if (m_errors != null && m_errors.ContainsKey(lineNumber))
          {
            brushes.AddRange(Errors[lineNumber].Select(validationError => GetBrush(validationError.ErrorState)));
          }
          else
          {
            if (m_hasSchema && m_errors != null && (m_jsonException == null || lineNumber < m_jsonException.LineIndex))
              brushes.Add(GetBrush(ValidationErrorState.Good));
            else
            {
              brushes.Add(GetBrush(ValidationErrorState.Unknown));
            }
          }
          double startY = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
          double stopY = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextBottom);
          double width = 8.0 / brushes.Count;
          int index = 0;
          foreach (Brush brush in brushes)
          {
            drawingContext.DrawRectangle(brush, null,
                                         new Rect(2 + (index * width), startY - textView.VerticalOffset, width,
                                                  stopY - startY));
            index++;
          }
        }
      }
    }

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
      if (oldTextView != null)
      {
        oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
      }
      base.OnTextViewChanged(oldTextView, newTextView);
      if (newTextView != null)
      {
        newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
      }
      InvalidateVisual();
    }

    protected override void OnDocumentChanged(TextDocument oldDocument, TextDocument newDocument)
    {
      if (oldDocument != null)
      {
        PropertyChangedEventManager.RemoveListener(oldDocument, this, "LineCount");
      }
      base.OnDocumentChanged(oldDocument, newDocument);
      if (newDocument != null)
      {
        PropertyChangedEventManager.AddListener(newDocument, this, "LineCount");
      }
      OnDocumentLineCountChanged();
    }

    protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
      if (managerType == typeof(PropertyChangedEventManager))
      {
        OnDocumentLineCountChanged();
        return true;
      }
      return false;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      UpdateLineNumber(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      UpdateLineNumber(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      base.OnMouseLeave(e);
      UpdateLineNumber(e, true);
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
      return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }
    #endregion

    #region private methods
    private void UpdateLineNumber(MouseEventArgs e, bool leaveing = false)
    {
      int newLineNumber = -1;
      if (!leaveing)
      {
        Point pos = e.GetPosition(TextView);
        pos.X = 0;
        pos.Y += TextView.VerticalOffset;
        VisualLine vl = TextView.GetVisualLineFromVisualTop(pos.Y);
        if (vl != null)
          newLineNumber = vl.FirstDocumentLine.LineNumber;
      }
      TooltipLine = newLineNumber - 1;
    }

    private Brush GetBrush(ValidationErrorState errorState)
    {
      //TODO move colors to settings
      switch (errorState)
      {
        case ValidationErrorState.Good:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0x00, 0x80, 0x00));
        case ValidationErrorState.NotInSchema:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0x80, 0x00, 0x80));
        case ValidationErrorState.WrongData:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0xFF, 0x14, 0x93));
        case ValidationErrorState.NotCorrectJson:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0xFF, 0x00, 0x00));
        case ValidationErrorState.Unknown:
          return new SolidColorBrush(Color.FromArgb(0x55, 0xAD, 0xD8, 0xE6));
        case ValidationErrorState.ToMany:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0x00, 0x00, 0xCD));
        case ValidationErrorState.MissingChild:
          return new SolidColorBrush(Color.FromArgb(0xA5, 0xFF, 0xA5, 0x00));
        default:
          throw new ArgumentOutOfRangeException("errorState");
      }
    }

    private void OnDocumentLineCountChanged()
    {
      int documentLineCount = Document != null ? Document.LineCount : 1;
      int newLength = documentLineCount.ToString(CultureInfo.CurrentCulture).Length;

      if (newLength < 2)
        newLength = 2;

      if (newLength != m_maxLineNumberLength)
      {
        m_maxLineNumberLength = newLength;
        InvalidateMeasure();
      }
    }

    private void TextViewVisualLinesChanged(object sender, EventArgs e)
    {
      InvalidateVisual();
    }
    #endregion

    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
      return ReceiveWeakEvent(managerType, sender, e);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
