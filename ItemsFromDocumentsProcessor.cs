using System;
using System.Linq;
using Elucidon.Annotations;
using Elucidon.Core;
using Elucidon.Queries;
using Silobreaker.Api.Framework;
using Silobreaker.Api.Framework.ResultBuilders;
using Silobreaker.Api.QueryBuilders;
using Silobreaker.Api.Utility;

namespace Silobreaker.Api.Processors
{
    /// <summary>
    /// Processor for creating TermsFromDocuments results.
    /// </summary>
    public class ItemsFromDocumentsProcessor<TItem> : ProcessorBase
        where TItem : class, IItem, ITypedItem
    {
        private readonly IQueryDecorator<ItemQuery<TItem>> _queryDecorator;
        private readonly IItemsFromDocumentsQueryBuilder<TItem> _itemsFromDocumentsQueryBuilder;
        private readonly IParameterResolver _parameterResolver;
        private readonly IItemResultBuilder<TItem> _resultBuilder;
        private readonly InFocusSettings _infocusSettings;

        /// <summary>
        /// Creates a new instance of a <see cref="ItemsFromDocumentsProcessor{TItem}"/>.
        /// </summary>
        /// <param name="queryExecutor">Resonsible for executing queries.</param>
        /// <param name="itemsFromDocumentsQueryBuilder">Reponsible for creating the termsFromDocuments query objects.</param>
        /// <param name="resultBuilder">Responsible for building the result object.</param>
        /// <param name="parameterResolver">Responsible for resolving parameters.</param>
        /// <param name="queryDecorator">Responsible for decorating the query.</param>
        public ItemsFromDocumentsProcessor(
            [NotNull]IQueryExecutor queryExecutor,
            [NotNull]IItemsFromDocumentsQueryBuilder<TItem> itemsFromDocumentsQueryBuilder,
            [NotNull]IParameterResolver parameterResolver,
            [NotNull]IItemResultBuilder<TItem> resultBuilder,
            [CanBeNull]IQueryDecorator<ItemQuery<TItem>> queryDecorator)
            : base(queryExecutor)
        {
            if (resultBuilder == null)
                throw new ArgumentNullException("resultBuilder");

            if (itemsFromDocumentsQueryBuilder == null)
                throw new ArgumentNullException("itemsFromDocumentsQueryBuilder");

            if (parameterResolver == null)
                throw new ArgumentNullException("parameterResolver");


            _infocusSettings = new InFocusSettings(parameterResolver, queryExecutor.Environment);
            _resultBuilder = resultBuilder;
            _itemsFromDocumentsQueryBuilder = itemsFromDocumentsQueryBuilder;
            _parameterResolver = parameterResolver;
            _queryDecorator = queryDecorator;
        }

        /// <summary>
        /// Performs an items from documents search.
        /// </summary>
        /// <returns>An <see cref="IResultData"/> containing the search result.</returns>
        public IResultData ItemsFromDocuments()
        {
            string query = null;
            string[] types = null;
            string optimization = null;

            if (_parameterResolver != null)
            {
                query = _parameterResolver.ResolveString(ParameterNames.Query);
                types = _infocusSettings.GetResultTypeDescriptions();
                optimization = _parameterResolver.ResolveString(ParameterNames.Optimization);
            }

            // If empty search then do a full search with sample optimization otherwise it won't be able to complete the query
            if (String.IsNullOrEmpty(query))
            {
                query = "*";
                optimization = "sample";
            }

            return ItemsFromDocuments(query, types, optimization);
        }

        /// <summary>
        /// Performs a terms from documents search.
        /// </summary>
        /// <param name="query">Search query string.</param>
        /// <returns>An <see cref="IResultData"/> containing the search result.</returns>
        /// TODO: Is this ever used?
        public IResultData ItemsFromDocuments([NotNull]string query)
        {
            return ItemsFromDocuments(query, null, null);
        }

        /// <summary>
        /// Performs an item from documents search.
        /// </summary>
        /// <param name="query">Search query string.</param>
        /// <param name="types">A comma separated list of type strings.</param>
        /// <param name="optimization">string carrying the optimization method </param>
        /// <returns>An <see cref="IResultData"/> containing the search result.</returns>
        public IResultData ItemsFromDocuments([NotNull]string query, [CanBeNull]string[] types, [CanBeNull]string optimization)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            var countPerType = _infocusSettings.GetCountPerType();
            var typeLimits = Enumerable.Repeat(countPerType, types.Length).ToArray();
            
            // Generate queries
            var itemQuery = _itemsFromDocumentsQueryBuilder.Build(query, types, typeLimits, optimization);
            var decoratedQuery = itemQuery;

            if (_queryDecorator != null)
            {
                _queryDecorator.ResolveParameters();
                decoratedQuery = _queryDecorator.Decorate(itemQuery);
            }

            var queryResult = QueryExecutor.Execute(decoratedQuery) as ItemQueryResult<TItem>;

            if (queryResult == null)
                throw new ApiException("queryResult was not of type ItemQueryResult<ITerm>");

            var result = _resultBuilder.Build(queryResult, itemQuery, types);
            return result;
        }

        public override IResultData Search()
        {
            return ItemsFromDocuments();
        }
    }
}