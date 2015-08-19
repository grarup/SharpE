using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class BoolValueConverter : MarkupExtension,  IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        value = false;
      else if (value.GetType() != typeof (bool))
        value = true;
      bool comparewith = true;
      if (parameter != null)
      {
        if (parameter is bool)
          comparewith = (bool) parameter;
        else
        {
          string s = parameter as string;
          if (s != null)
          {
            comparewith = s.ToLower() == "true";
          }
        }
      }
      if (targetType == typeof (Visibility))
        return (bool) value == comparewith ? Visibility.Visible : Visibility.Collapsed;
      if (targetType == typeof (FontWeight))
        return (bool) value == comparewith ? FontWeights.Bold : FontWeights.Normal;
      if (targetType == typeof (bool) || targetType == typeof(bool?))
        return (bool)value == comparewith;
      if (targetType == typeof (int?))
        return ((int) value) != 0;
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null) return DependencyProperty.UnsetValue;
      bool comparewith = true;
      if (parameter != null)
      {
        if (parameter is bool)
          comparewith = (bool)parameter;
        else
        {
          string s = parameter as string;
          if (s != null)
          {
            comparewith = s == "true";
          }
        }
      }

      Type sourceType = value.GetType();
      if (sourceType == typeof (Visibility))
      {
        return ((Visibility) value) == Visibility.Visible;
      }
      if (sourceType == typeof(bool))
        return (bool)value == comparewith;
      return DependencyProperty.UnsetValue;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
