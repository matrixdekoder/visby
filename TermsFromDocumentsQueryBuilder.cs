using System;
using System.Linq;
using Elucidon.Annotations;
using Elucidon.Core;
using Elucidon.Queries;
using Silobreaker.Api.Framework;
using Silobreaker.Api.Logic;

namespace Silobreaker.Api.QueryBuilders
{
    /// <summary>
    /// Query builder that creates a TermsFromDocument query.
    /// </summary>
    public class TermsFromDocumentsQueryBuilder : ItemsFromDocumentsQueryBuilderBase<ITerm>
    {
        private readonly IDocumentQueryOptimizer _queryOptimizer;
        
        /// <summary>
        /// Creates a new instance of a <see cref="TermsFromDocumentsQueryBuilder"/>.
        /// </summary>
        /// <param name="queryExecutor">Responsible for executing queries. Used when limiting entities based on type.</param>
        /// <param name="documentQueryBuilder">Responsible for creating a document query that is the source of the query.</param>
        /// <param name="queryOptimizer">Query sampler to improve performance. Used to limit the set of items to analyze. </param>
        public TermsFromDocumentsQueryBuilder([NotNull] IQueryExecutor queryExecutor, [NotNull] IItemSearchQueryBuilder<IDocument> documentQueryBuilder, [NotNull] IDocumentQueryOptimizer queryOptimizer)
            : base(queryExecutor, documentQueryBuilder)
        {
            _queryOptimizer = queryOptimizer;
        }

        /// <inheritdoc/>
        protected override ItemQuery<ITerm> GetRelatedItemsQueryLimitedByTypes( [NotNull] ItemQuery<IDocument> documentQuery, [NotNull] string[] entityTypeList, [NotNull] int[] entityTypeLimits, [CanBeNull] string optimization)
        {
            if (documentQuery == null)
                throw new ArgumentNullException("documentQuery");

            if (entityTypeList == null)
                throw new ArgumentNullException("entityTypeList");

            if (entityTypeLimits == null)
                throw new ArgumentNullException("entityTypeLimits");

            if (entityTypeLimits.Length != entityTypeList.Length)
                throw new ArgumentException("The number of entity types and the number of entity type limits does not match.");

            int maxTermTypeLimit = entityTypeLimits.Max();

            ItemQuery<ITerm> termsOfTypeQuery = new QItems_ByTypes<ITerm>(entityTypeList);
            ItemQuery<ITerm> limitedQuery;
            if (optimization == "sample")
            {
                ItemQuery<IDocument> sampledDocumentQuery = _queryOptimizer.Optimize(documentQuery);
                limitedQuery = new QItems_PartitionByType<ITerm>(new QTerms_ByDocuments(sampledDocumentQuery)*termsOfTypeQuery, maxTermTypeLimit);
            }
            else
            {
                limitedQuery = new QItems_PartitionByType<ITerm>(new QTerms_ByDocuments(documentQuery)*termsOfTypeQuery, maxTermTypeLimit);
            }

            var queryResult = QueryExecutor.Execute(limitedQuery) as ItemQueryResult<ITerm>;
            ItemQuery<ITerm> result = LimitResultByTypes(queryResult, entityTypeList, entityTypeLimits);
            return result;
        }

        /// <inheritdoc/>
        protected override ItemQuery<ITerm> GetRelatedItemsQuery([NotNull] ItemQuery<IDocument> documentQuery, [CanBeNull] string[] entityTypeList, [CanBeNull] string optimization)
        {
            if (documentQuery == null)
                throw new ArgumentNullException("documentQuery");
            ItemQuery<ITerm> termQuery;

            if (optimization == "sample")
            {
                ItemQuery<IDocument> sampledDocumentQuery = _queryOptimizer.Optimize(documentQuery);
                termQuery = new QTerms_ByDocuments(sampledDocumentQuery);
            }
            else
                termQuery = new QTerms_ByDocuments(documentQuery);

            // Either limit the types based on parameters
            if (entityTypeList != null && entityTypeList.Length > 0)
                termQuery *= new QItems_ByTypes<ITerm>(entityTypeList);
                // ...or force it to identified
            else
                termQuery *= new QTerms_Identified();

            return termQuery;
        }
    }
}