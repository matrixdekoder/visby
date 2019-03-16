using System;
using Silobreaker.Api.Framework;

namespace Silobreaker.Api.ParameterResolvers
{
    public class MultiParameterResolver : IParameterResolver
    {
        private readonly IParameterResolver[] _parameterResolvers;

        public MultiParameterResolver(IParameterResolver[] parameterResolvers)
        {
            _parameterResolvers = parameterResolvers;
        }

        public string ResolveString(string key)
        {
            foreach (var parameterResolver in _parameterResolvers)
            {
                var res = parameterResolver.ResolveString(key);
                if (!string.IsNullOrEmpty(res))
                    return res;
            }
            return null;
        }

        public bool? ResolveBool(string key)
        {
            foreach (var parameterResolver in _parameterResolvers)
            {
                var res = parameterResolver.ResolveBool(key);
                if (res.HasValue)
                    return res;
            }
            return null;
        }

        public DateTime? ResolveDateTime(string key)
        {
            foreach (var parameterResolver in _parameterResolvers)
            {
                var res = parameterResolver.ResolveDateTime(key);
                if (res.HasValue)
                    return res;
            }
            return null;
        }

        public int? ResolveInt(string key)
        {
            foreach (var parameterResolver in _parameterResolvers)
            {
                var res = parameterResolver.ResolveInt(key);
                if (res.HasValue)
                    return res;
            }
            return null;
        }

        public double? ResolveDouble(string key)
        {
            foreach (var parameterResolver in _parameterResolvers)
            {
                var res = parameterResolver.ResolveDouble(key);
                if (res.HasValue)
                    return res;
            }
            return null;
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
            catch (ApiException)
            {
                result = default(T);
                return false;
            }
        }
    }
}
