using System;
using System.Globalization;
using Elucidon.Annotations;
using Silobreaker.Api.Framework;

namespace Silobreaker.Api.ParameterResolvers
{
    /// <summary>
    /// Represents a parameter resolver that retrieves parameters from the HttpContextFactory's Item collection.
    /// </summary>
    public class HttpContextParameterResolver : IParameterResolver
    {
        /// <inheritdoc/>
        public string ResolveString([NotNull]string key)
        {
            if (key == null) 
                throw new ArgumentNullException("key");

            if(HttpContextFactory.Current == null)
                throw new ApiException("Tried to resolve a http context parameter but HttpContextFactory.Current was not set.");

            return HttpContextFactory.Current.Items[key] as string;
        }

        /// <inheritdoc/>
        public bool? ResolveBool([NotNull]string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (HttpContextFactory.Current == null)
                throw new ApiException("Tried to resolve a http context parameter but HttpContextFactory.Current was not set.");

            return HttpContextFactory.Current.Items[key] as bool?;
        }

        /// <inheritdoc/>
        public DateTime? ResolveDateTime(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (HttpContextFactory.Current == null)
                throw new ApiException("Tried to resolve a http context parameter but HttpContextFactory.Current was not set.");

            var dateTimeString = HttpContextFactory.Current.Items[key] as string;
            if(string.IsNullOrEmpty(dateTimeString))
                return null;

            DateTime result;
            if(DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }

        /// <inheritdoc/>
        public int? ResolveInt([NotNull]string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (HttpContextFactory.Current == null)
                throw new ApiException("Tried to resolve a http context parameter but HttpContextFactory.Current was not set.");

            return HttpContextFactory.Current.Items[key] as int?;
        }

        /// <inheritdoc/>
        public double? ResolveDouble([NotNull]string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (HttpContextFactory.Current == null)
                throw new ApiException("Tried to resolve a http context parameter but HttpContextFactory.Current was not set.");

            return HttpContextFactory.Current.Items[key] as double?;
        }

        public T ResolveParameter<T>(string key)
        {
            object res = null;

            if (typeof(T) == typeof(int))
                res = ResolveInt(key);
            else if (typeof(T) == typeof(string))
                res = ResolveString(key);
            else if (typeof(T) == typeof(bool))
                res = ResolveBool(key);
            else if (typeof(T) == typeof(DateTime))
                res = ResolveDateTime(key);
            else if (typeof(T) == typeof(double))
                res = ResolveDouble(key);
            if (res == null)
                throw new ApiException(key + " Parameter missing");

            return (T)res;
        }

        public bool TryResolveParameter<T>(string key, out T result)
        {
            try
            {
                result = ResolveParameter<T>(key);
                return true;
            }
            catch(ApiException)
            {
                result = default(T);
                return false;
            }
        }
    }
}
