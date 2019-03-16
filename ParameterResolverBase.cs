using System;
using System.Globalization;
using Elucidon.Annotations;
using Silobreaker.Api.Framework;

namespace Silobreaker.Api.ParameterResolvers
{
    /// <summary>
    /// Base class for the <see cref="IParameterResolver"/> interface.
    /// </summary>
    public abstract class ParameterResolverBase : IParameterResolver
    {
        /// <inheritdoc />
        public abstract string ResolveString(string key);

        /// <inheritdoc />
        public virtual bool? ResolveBool([NotNull]string key)
        {
            if (key == null) 
                throw new ArgumentNullException("key");

            bool resolvedValue;
            var s = ResolveString(key);
            
            if(string.IsNullOrEmpty(s))
                return null;

            if(bool.TryParse(s, out resolvedValue))
                return resolvedValue;

            throw new ParameterException(
                string.Format("Unable to resolve parameter \"{0}\" as a boolean. Contents of parameter was {1}.", key, s));                
        }

        /// <inheritdoc/>
        public DateTime? ResolveDateTime(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var s = ResolveString(key);

            if(string.IsNullOrEmpty(s))
                return null;

            DateTime resolvedValue;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out resolvedValue))
                return resolvedValue;
            
            throw new ParameterException(
                string.Format("Unable to resolve parameter \"{0}\" as an datetime. Contents of parameter was {1}.", key, s));
        }

        /// <inheritdoc />
        public virtual int? ResolveInt([NotNull]string key)
        {
            if (key == null) 
                throw new ArgumentNullException("key");

            int resolvedValue;
            var s = ResolveString(key);
            
            if(string.IsNullOrEmpty(s))
                return null;

            if(int.TryParse(s, out resolvedValue))
                return resolvedValue;

            throw new ParameterException(
                string.Format("Unable to resolve parameter \"{0}\" as an integer. Contents of parameter was {1}.", key, s));
        }

        /// <inheritdoc/>
        public virtual double? ResolveDouble([NotNull] string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            double resolvedValue;
            var s = ResolveString(key);

            if (string.IsNullOrEmpty(s))
                return null;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out resolvedValue))
                return resolvedValue;

            throw new ParameterException(
                string.Format("Unable to resolve parameter \"{0}\" as a double. Contents of parameter was {1}.", key, s));
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
            catch (ApiException)
            {
                result = default(T);
                return false;
            }
        }
    }
}