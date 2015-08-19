using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class EnumBoolConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      bool result = parameter.Equals(value);
      if (targetType == typeof(Visibility))
      {
        return result ? Visibility.Visible : Visibility.Collapsed;
      }
      return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return parameter;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}

