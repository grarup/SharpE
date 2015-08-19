using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class StringValueConveter : MarkupExtension, IValueConverter
  {
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value == null ? null : value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value.GetType() != typeof (string))
        return null;
      if (targetType == typeof (string))
        return value;
      if (targetType == typeof (int) || targetType == typeof(int?))
      {
        int result;
        if (int.TryParse((string) value, out result))
          return result;
        return null;
      }
      if (targetType == typeof(double) || targetType == typeof(double?))
      {
        double result;
        if (double.TryParse((string)value, out result))
          return result;
        return null;
      }
      return null;
    }
  }
}
