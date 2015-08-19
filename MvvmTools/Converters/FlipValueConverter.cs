using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class FlipValueConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value != null && value.Equals(parameter) ? -1 : 1;
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
