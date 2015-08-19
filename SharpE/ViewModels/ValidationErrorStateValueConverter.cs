using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using SharpE.Definitions.Editor;

namespace SharpE.ViewModels
{
  class ValidationStateValueConverter : MarkupExtension, IValueConverter
  {
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is ValidationState))
        return DependencyProperty.UnsetValue;
      if (targetType == typeof(Brush))
      {
        switch ((ValidationState)value)
        {
          case ValidationState.Undefined:
            return Brushes.Black;
          case ValidationState.Good:
            return Brushes.Green;
          case ValidationState.Errors:
            return Brushes.Red;
          case ValidationState.Warnings:
            return Brushes.Orange;
          default:
            throw new ArgumentOutOfRangeException("value");
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
