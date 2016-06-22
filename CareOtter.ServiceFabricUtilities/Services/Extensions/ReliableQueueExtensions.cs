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
    public static class ReliableQueueExtensions
    {
        /// <summary>
        /// returns a readonly snapshot enumerable for a reliable queue. May be invalidated
        /// </summary>
        /// <typeparam name="T">Val type for the queue</typeparam>
        /// <param name="tx">Transaction from statemanager</param>
        /// <returns>Enumerable for the collection</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        public static async Task<IEnumerable<T>> GetReadOnlyReliableQueueEnumerableAsync<T>(this IReliableQueue<T> collection, ITransaction tx)
        {
            var result = new List<T>();

            var query = await collection.CreateEnumerableAsync(tx);
            using (IAsyncEnumerator<T> e = query.GetAsyncEnumerator())
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
