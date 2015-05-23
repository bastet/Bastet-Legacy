using System.Collections.Concurrent;
using Bastet.Database.Model;
using System;
using System.Linq;
using System.Management.Instrumentation;

namespace Bastet.Backends
{
    public static class BackendFactory
    {
        private static readonly ConcurrentDictionary<Type, bool> _registeredBackends = new ConcurrentDictionary<Type, bool>(); 

        public static void Register<T>(Database.Database database) where T : IBackend
        {
            throw new NotImplementedException();
        }

        public static IBackend Backend(this Device device)
        {
            return Backend(device.Backend);
        }

        public static IBackend Backend(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Type type = BackendType(name);
            if (type == null)
                throw new InstanceNotFoundException();

            return (IBackend)Activator.CreateInstance(type);
        }

        public static Type BackendType(string name)
        {
            //Find the backend for the given type name
            Type type = Type.GetType(name ?? "", false, true);
            if (type == null)
            {
                type = _registeredBackends.Keys
                    .Where(t => typeof(IBackend).IsAssignableFrom(t))
                    .SingleOrDefault(x => x.Name == name);
            }

            //Check that this backend has been registered
            if (type != null && !_registeredBackends.ContainsKey(type))
                type = null;

            return type;
        }
    }
}
