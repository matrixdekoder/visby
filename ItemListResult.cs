using System;
using System.Linq;

namespace Silobreaker.Api.Framework.DataContracts
{
    /// <summary>
    /// Contains a list of <see cref="IItemData"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IItemData"/> in the list.</typeparam>
    [Serializable]
    public class ItemListResult<T> : ResultList<T>, IDescription, IItemListResult<T> where T : class, IItemData
    {
        /// <inheritdoc/>
        IItemData[] IItemListResult.Items
        {
            get { return Items; }
            set
            {
                Items = value.Cast<T>().ToArray();
            }
        }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Use this property to override the title for RSS feed
        /// </summary>
        public string Title { get; set; }
    }
}