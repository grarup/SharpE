using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class IsTypeValueConverter : MarkupExtension,  IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isType = value != null && parameter != null && parameter is Type && ((Type)parameter).IsAssignableFrom(value.GetType());
      if (targetType == typeof (Visibility))
        return isType ? Visibility.Visible : Visibility.Collapsed;
      if (targetType == typeof (FontWeight))
        return isType ? FontWeights.Bold : FontWeights.Normal;
      if (targetType == typeof (bool) || targetType == typeof(bool?))
        return isType;
      if (targetType == typeof (int?))
        return isType ? 1 : 0;
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
