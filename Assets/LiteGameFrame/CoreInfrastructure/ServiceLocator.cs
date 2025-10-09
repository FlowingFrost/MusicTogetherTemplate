using System;
using System.Collections.Generic;

namespace LiteGameFrame.CoreInfrastructure
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        public static void Register<T>(T service)
        {
            _services.Add(typeof(T), service);
        }

        public static T GetService<T>()
        {
            return (T)_services[typeof(T)];
        }
        
    }
}