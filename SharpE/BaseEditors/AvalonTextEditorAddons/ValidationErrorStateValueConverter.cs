using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using SharpE.Json.Schemas;

namespace SharpE.BaseEditors.AvalonTextEditorAddons
{
  class ValidationErrorStateValueConverter : MarkupExtension, IValueConverter
  {
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is ValidationErrorState))
        return DependencyProperty.UnsetValue;
      if (targetType == typeof(Brush))
      {
        switch ((ValidationErrorState)value)
        {
          case ValidationErrorState.Good:
            return Brushes.Green;
          case ValidationErrorState.NotInSchema:
            return Brushes.Purple;
          case ValidationErrorState.WrongData:
            return Brushes.DeepPink;
          case ValidationErrorState.NotCorrectJson:
            return Brushes.Red;
          case ValidationErrorState.Unknown:
            return Brushes.LightBlue;
          case ValidationErrorState.ToMany:
            return Brushes.MediumBlue;
          case ValidationErrorState.MissingChild:
            return Brushes.Orange;
          default:
            throw new ArgumentOutOfRangeException("targetType");
        }
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
