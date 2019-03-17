using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Elucidon.Annotations;
using Silobreaker.Api.Framework;
using Silobreaker.Api.Framework.DataContracts;
using Silobreaker.Api.Framework.Security;
using Silobreaker.Api.MvcApplication.ActionResultBuilders;
using Silobreaker.Api.MvcApplication.Logging;
using Spring.Context.Support;

namespace Silobreaker.Api.MvcApplication.Controllers
{
    /// <summary>
    /// Controller that instanciates a separate component and forwards the parameters to one of its methods.
    /// </summary>   
    /// <remarks>The <see cref="DynamicController"/> uses the url parts of a request and additional
    /// parameters passed by the MVC framework to a method in a separate component (given by the IOC framework).</remarks>    
    public class DynamicController : AuthorizationBaseController
    {
        private readonly IActionResultBuilder _defaultActionResultBuilder;
        private readonly IDictionary<string, IActionResultBuilder> _actionResultBuilders;        
        private static readonly string[] _ignoreParameters = new[] { "componentName", "methodName", "controller", "action" };

        /// <summary>
        /// Instanciates a new instance of the <see cref="DynamicController"/> class.
        /// </summary>
        /// <param name="requestAuthenticator">Component that authenticates api-request by checking api-key and request digests. Cannot be null.</param>
        /// <param name="accessMatrix">Access matrix used to check access to content.</param>
        /// <param name="defaultActionResultBuilder">The default action result builder to use if no other can be found or is specified in <paramref name="actionResultBuilders"/>. Cannot be null.</param>
        /// <param name="actionResultBuilders">Dictionary with maps type names (as string) to action result builder (as IActionResultBuilder). Can be null.</param>
        /// <param name="requestLogger">The logger to use for logging requests. Can be null.</param>
        public DynamicController([NotNull]IRequestAuthenticator requestAuthenticator, [NotNull]IAccessMatrix accessMatrix, 
            [NotNull]IActionResultBuilder defaultActionResultBuilder, 
            [CanBeNull]IDictionary<string, IActionResultBuilder> actionResultBuilders, [CanBeNull] IRequestLogger requestLogger)
            : base(requestLogger, null, requestAuthenticator, accessMatrix)
        {
            
            if (defaultActionResultBuilder == null)
                throw new ArgumentNullException("defaultActionResultBuilder");
            
            _defaultActionResultBuilder = defaultActionResultBuilder;
            _actionResultBuilders = actionResultBuilders;
        }

        /// <summary>
        /// The generic metod used to invoke a separate components.
        /// </summary>
        /// <returns>An <see cref="IResultData"/> that has been serialized using a serializer
        /// given by the "type" request paramater.</returns>
        public virtual ActionResult InvokeMethod()
        {            
            var componentName = (string)RouteData.Values["componentName"];
            var methodName = (string)RouteData.Values["methodName"];
            var component = ContextRegistry.GetContext()[componentName];
            
            // Extract parameters from routedata, map them to the method and invoke
            var componentType = component.GetType();

            var parameters = GetParameters();
            
            TryToAddDebugData();

            // Replace "~" with "*" for parameter key "query"
            object queryObject;
            if(parameters.TryGetValue("query", out queryObject))
            {
                var query = queryObject as string;
                if(query != null && query == "~")
                {
                    parameters["query"] = "*";
                }
            }

            var orderedParameters = MapDictionaryToMethodParameters(parameters, componentType, methodName);
            object result;
            try
            {
                result = componentType.InvokeMember(methodName,
                                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy,
                                           null,
                                           component,
                                           orderedParameters);
                if (DebugInfo != null && result is ResultDataBase)
                    ((ResultDataBase)result).DebugInfo = DebugInfo;
            }
            catch (TargetInvocationException exp)
            {                
                throw exp.InnerException;
            }

            IActionResultBuilder actionResultBuilder = null;

            if (Request["type"] != null && _actionResultBuilders != null)
            {
                _actionResultBuilders.TryGetValue(Request["type"], out actionResultBuilder);
            }

            if (actionResultBuilder == null)
                actionResultBuilder = _defaultActionResultBuilder;

            ActionResult actionResult;
            try
            {
                actionResult = actionResultBuilder.Build(result);
            }
            catch(NotSupportedException nse)
            {
                throw new ParameterException("This operation is not supported by the desired response type " + Request["type"], nse);
            }

            return actionResult;
        }


        private Dictionary<string, object> GetParameters()
        {
            var parameters = new Dictionary<string, object>();
            foreach (var key in RouteData.Values.Keys.Where(x => !_ignoreParameters.Contains(x, StringComparer.InvariantCultureIgnoreCase)))
            {
                parameters.Add(key, RouteData.Values[key]);
            }
            return parameters;
        }

        /// <summary>
        /// Iterates over a dictionary and maps values to the parameters of a method signature.
        /// </summary>
        internal static object[] MapDictionaryToMethodParameters(IDictionary<string, Object> dictionary, Type componentType, string methodName)
        {
            var parameterTypes = dictionary.Values.Select(x => x.GetType()).ToArray();
            var methodInfo = componentType.GetMethod(methodName, parameterTypes);
            var parameterInfos = methodInfo.GetParameters();
            
            var orderedParameters = new object[parameterInfos.Length];
            for(int i = 0; i < orderedParameters.Length; i++)
            {
                orderedParameters[i] = dictionary[parameterInfos[i].Name];
            }
            return orderedParameters;
        }
    }
}
