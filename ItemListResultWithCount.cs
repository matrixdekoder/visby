using System;
using System.Runtime.Serialization;

namespace Silobreaker.Api.Framework.DataContracts
{
    /// <summary>
    /// Data result class for lists of items containing a total count.
    /// </summary>
    /// <typeparam name="T">Type of the item.</typeparam>
    [DataContract]
    [Serializable]
    public class ItemListResultWithCount<T> : ItemListResult<T>, IItemListResultWithCount where T : class, IItemData
    {
        /// <inheritdoc />
        [DataMember]
        public int TotalCount { get; set; }

    }
}