using System;
using System.Collections.Generic;
using System.Globalization;

namespace Supremacy.Xna
{
    public class ServiceContainer : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService(Type type, object provider)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "The service type cannot be null.");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider", "The service provider instance cannot be null.");
            }

            if (_services.ContainsKey(type))
            {
                throw new ArgumentException("Container already contains a service of this type.", "type");
            }

            if (!type.IsAssignableFrom(provider.GetType()))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentUICulture,
                        "Service provider object of type {0} must be assignable to service type {1}.",
                        new object[] { provider.GetType().FullName, type.GetType().FullName }));
            }

            _services.Add(type, provider);
        }

        public object GetService(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "The service type cannot be null.");
            }


            return _services.TryGetValue(type, out object service) ? service : null;
        }

        public void RemoveService(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "The service type cannot be null.");
            }

            _ = _services.Remove(type);
        }
    }
}