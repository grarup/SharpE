using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using SharpE.Json.Data;
using SharpE.MvvmTools.Commands;

namespace SharpE.ViewModels
{
  public class KeyboardBindingGenerator
  {
    public static IEnumerable<KeyBinding> GenerateKeyBinding(JsonArray array, object obj)
    {
      List<KeyBinding> keyBindings = new List<KeyBinding>();
      KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
      
      foreach (JsonNode jsonNode in array)
      {
        KeyGesture keyGesture =
          keyGestureConverter.ConvertFromString(jsonNode.GetObjectOrDefault("gesture", "")) as KeyGesture;
        if (keyGesture == null)
          continue;
        MethodInfo methodInfo = obj.GetType().GetMethod(jsonNode.GetObjectOrDefault("command", ""), new Type[0]);
        if (methodInfo == null)
          continue;
        keyBindings.Add(new KeyBinding(new ManualCommand(() => methodInfo.Invoke(obj, null)), keyGesture ));
      }

      return keyBindings;
    }
  }
}
