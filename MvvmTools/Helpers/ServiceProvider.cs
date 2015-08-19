using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpE.MvvmTools.Helpers
{
  public static class ServiceProvider
  {
    private static readonly Dictionary<Type, ServiceHolder> s_services = new Dictionary<Type, ServiceHolder>();

    public static void Registre<TInterface, TImplementation>(bool isSingleton) where TImplementation : TInterface
    {
      s_services.Add(typeof(TInterface), new ServiceHolder(isSingleton, typeof(TImplementation)));
    }

    public static void Registre<TInterface>(object implementation)
    {
      s_services.Add(typeof(TInterface), new ServiceHolder(true, null, implementation));
    }

    public static T Resolve<T>()
    {
      if (!s_services.ContainsKey(typeof(T)))
        throw new ArgumentException(string.Format("No service for {0} is registret", typeof(T)));
      return (T) s_services[typeof (T)].Implementation;
    }

    private class ServiceHolder
    {
      private readonly bool m_isSinglton;
      private readonly Type m_type;
      private object m_implementation;

      public ServiceHolder(bool isSinglton, Type type, object implementation = null)
      {
        m_isSinglton = isSinglton;
        m_type = type;
        m_implementation = implementation;
      }

      public object Implementation
      {
        get
        {
          if (m_isSinglton)
          {
            return m_implementation ?? (m_implementation = CreateImplementation(m_type));
          }
          return CreateImplementation(m_type);
        }
      }

      private static object CreateImplementation(Type type)
      {
        if (s_services.ContainsKey(type))
          return s_services[type].Implementation;

        ConstructorInfo constructorInfo = type.GetConstructors().First();

        List<object> parameters = constructorInfo.GetParameters().Select(parameterInfo => CreateImplementation(parameterInfo.ParameterType)).ToList();

        return parameters.Count > 0 ? Activator.CreateInstance(type, parameters) : Activator.CreateInstance(type);
      }
    }
  }
}
