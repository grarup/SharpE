using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class KiloValueConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double doubleValue = value is int ? (int) value : (double) value;
      string formatingString = parameter as string ?? (doubleValue < 1000 ? "{0:0}" : "{0:0.#}k");
      if (doubleValue < 1000)
        return String.Format(formatingString, doubleValue);
      return String.Format(formatingString, doubleValue / 1000);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
