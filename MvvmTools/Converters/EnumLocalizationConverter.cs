using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  public class EnumLocalizationConverter : MarkupExtension, IValueConverter
  {
    private static ResourceManager s_baseResourceManager;
    private static readonly Dictionary<Type,ResourceManager> s_resourceManagers = new Dictionary<Type, ResourceManager>();
    private static EnumLocalizationConverter s_instance;

    public static void RegistreResourceManager(Type type, ResourceManager resourceManager)
    {
      if (s_resourceManagers.ContainsKey(type))
        s_resourceManagers[type] = resourceManager;
      else
        s_resourceManagers.Add(type, resourceManager);
    }

    public static ResourceManager BaseResourceManager
    {
      set
      {
        if (s_baseResourceManager != null)
          Debug.Assert(false, "BaseResourceManger already set");
        s_baseResourceManager = value;
      }
    }

    public static string Convert(object value, string adaptaion = null)
    {
      object retVal = Convert(value, typeof (string), adaptaion, null);
      if (retVal == DependencyProperty.UnsetValue)
        return "";
      return (string) retVal;
    }

    public static object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null) return null;
      Type type = value.GetType();
      if (!type.IsEnum)
      {
        string displayName = parameter as string;
        if (type.IsClass && displayName != null)
        {
          PropertyInfo propertyInfo = type.GetProperty(displayName);
          return propertyInfo.GetValue(value);
        }
        return value;
      }
      if (targetType == typeof (string) || targetType == typeof(object))
      {
        ResourceManager resourceManager;
        s_resourceManagers.TryGetValue(type, out resourceManager);
        if (resourceManager == null)
          resourceManager = s_baseResourceManager;
        if (resourceManager == null)
          return value.ToString();
        string lookup = GenerateLookup(parameter as string, type.FullName, Enum.GetName(type, value));
        string retVal = resourceManager.GetString(lookup);
        if (retVal == null)
        {
          Debug.Assert(false, "Localization of " + lookup + " could not be found");
          retVal = value.ToString();
        }
        return retVal;
      }
      Debug.Assert(false, "Unhandled targetType");

      return DependencyProperty.UnsetValue;
    }

    private static string GenerateLookup(string adaptaion, string typeName, string valueName)
    {
      return (string.IsNullOrEmpty(adaptaion) ? "" : adaptaion + "_") + typeName.Replace('.', '_') + "_" + valueName;
    }


    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert(value, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return s_instance ?? (s_instance = new EnumLocalizationConverter());
    }
  }
}
