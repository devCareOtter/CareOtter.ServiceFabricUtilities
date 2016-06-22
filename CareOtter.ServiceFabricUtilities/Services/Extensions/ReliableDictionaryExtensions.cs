using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CareOtter.ServiceFabricUtilities.Services.Extensions
{
    public static class ReliableDictionaryExtensions
    {
        /// <summary>
        /// returns a readonly snapshot enumerable for the dictionary. May be invalidated
        /// </summary>
        /// <typeparam name="TKey">Key type for the dictionary</typeparam>
        /// <typeparam name="TVal">Value type for the dictionary</typeparam>
        /// <param name="tx">Transaction from statemanager</param>
        /// <returns>enumerable for the dict</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        public static async Task<IEnumerable<KeyValuePair<TKey, TVal>>> GetReadOnlyReliableDictionaryEnumerableAsync
            <TKey, TVal>(this IReliableDictionary<TKey, TVal> dict, ITransaction tx) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var result = new List<KeyValuePair<TKey, TVal>>();
            var query = await dict.CreateEnumerableAsync(tx);
            using (IAsyncEnumerator<KeyValuePair<TKey, TVal>> e = query.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    result.Add(e.Current);
                }
            }
            return result;
        }
    }
}
