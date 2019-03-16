using System;
using Elucidon.Annotations;
using Silobreaker.Api.Framework;

namespace Silobreaker.Api.ParameterResolvers
{
    /// <summary>
    /// Resolves parameters using the current HttpContextFactory.
    /// Parameters will be checked against GET, POST, Form, ServerVariables and Cookies for parameter match.
    /// </summary>
    public class HttpRequestParameterResolver : ParameterResolverBase
    {
        /// <inheritdoc />
        public override string ResolveString([NotNull]string key)
        {
            if (key == null) 
                throw new ArgumentNullException("key");

            if(HttpContextFactory.Current == null)
                throw new ApiException(String.Format("No HttpContextFactory available when resolving parameter {0}", key));

            if (HttpContextFactory.Current.Request == null)
                throw new ApiException(String.Format("No Http Request available when resolving parameter {0}", key));

            return HttpContextFactory.Current.Request.Params[key];
        }
    }
}