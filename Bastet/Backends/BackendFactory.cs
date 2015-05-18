using Bastet.Database.Model;
using System;
using System.Linq;
using System.Management.Instrumentation;

namespace Bastet.Backends
{
    public static class BackendFactory
    {
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
            Type type = Type.GetType(name ?? "", false, true);
            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(IBackend).IsAssignableFrom(t))
                    .SingleOrDefault(x => x.Name == name);
            }

            return type;
        }
    }
}
