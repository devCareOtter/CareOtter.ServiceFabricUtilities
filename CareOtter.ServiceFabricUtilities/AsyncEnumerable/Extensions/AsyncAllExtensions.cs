using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncAllExtensions
    {
        /// <summary>
        /// Ensures all elements conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <param name="ct">cancellation token for the operation</param>
        /// <param name="allFunc">lambda to check conformity</param>
        /// <returns>if all elements conform</returns>
        public static async Task<bool> AllAsync<TValType>(this IAsyncEnumerator<TValType> enumerator, CancellationToken ct,
            Func<TValType, bool> allFunc)
        {
            while (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                if (!allFunc(enumerator.Current))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Ensures all elements conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <param name="allFunc">lambda to check conformity</param>
        /// <returns>if all elements conform</returns>
        public static Task<bool> AllAsync<TValType>(this IAsyncEnumerator<TValType> enumerator,
            Func<TValType, bool> allFunc)
        {
            return enumerator.AllAsync(CancellationToken.None, allFunc);
        }


        /// <summary>
        /// Ensures all elements conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <param name="ct">cancellation token for the operation</param>
        /// <param name="allFunc">lambda to check conformity</param>
        /// <returns>if all elements conform</returns>
        public static Task<bool> AllAsync<TValType>(this IAsyncEnumerable<TValType> enumerable, CancellationToken ct,
            Func<TValType, bool> allFunc)
        {
            return enumerable.GetAsyncEnumerator().AllAsync(ct, allFunc);
        }


        /// <summary>
        /// Ensures all elements conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <param name="allFunc">lambda to check conformity</param>
        /// <returns>if all elements conform</returns>
        public static Task<bool> AllAsync<TValType>(this IAsyncEnumerable<TValType> enumerable,
            Func<TValType, bool> allFunc)
        {
            return enumerable.AllAsync(CancellationToken.None, allFunc);
        }
    }
}