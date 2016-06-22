using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncAnyExtensions
    {
        /// <summary>
        /// Checks that any of the elements in a collection conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <param name="ct">cancellation token for the async methods</param>
        /// <param name="anyFunc">the function to check conformity for</param>
        /// <returns>if any elements conform to the lambda</returns>
        public static async Task<bool> AnyAsync<TValType>(this IAsyncEnumerator<TValType> enumerator,
            CancellationToken ct,Func<TValType, bool> anyFunc)
        {
            while (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                if (anyFunc(enumerator.Current))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks that any of the elements in a collection conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <param name="anyFunc">the function to check conformity for</param>
        /// <returns>if any elements conform to the lambda</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerator<TValType> enumerator,
            Func<TValType, bool> anyFunc)
        {
            return enumerator.AnyAsync(CancellationToken.None, anyFunc);
        }

        /// <summary>
        /// Checks if there are any elements in a collection
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <param name="ct">cancellation token for the async methods</param>
        /// <returns>if there are any elements in the collection</returns>
        public static async Task<bool> AnyAsync<TValType>(this IAsyncEnumerator<TValType> enumerator,
            CancellationToken ct)
        {
            return await enumerator.MoveNextAsync(ct); //check that there's at least one in the collection
        }

        /// <summary>
        /// Checks if there are any elements in a collection
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerator">the enumerator to check</param>
        /// <returns>if there are any elements in the collection</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerator<TValType> enumerator)
        {
            return enumerator.AnyAsync(CancellationToken.None);
        }


        /// <summary>
        /// Checks that any of the elements in a collection conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <param name="ct">cancellation token for the async methods</param>
        /// <param name="anyFunc">the function to check conformity for</param>
        /// <returns>if any elements conform to the lambda</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerable<TValType> enumerable, CancellationToken ct,
            Func<TValType, bool> anyFunc)
        {
            return enumerable.GetAsyncEnumerator().AnyAsync(ct, anyFunc);
        }


        /// <summary>
        /// Checks that any of the elements in a collection conform to a lambda
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <param name="anyFunc">the function to check conformity for</param>
        /// <returns>if any elements conform to the lambda</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerable<TValType> enumerable,
            Func<TValType, bool> anyFunc)
        {
            return enumerable.GetAsyncEnumerator().AnyAsync(anyFunc);
        }

        /// <summary>
        /// Checks if there are any elements in a collection
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <param name="ct">cancellation token for the async methods</param>
        /// <returns>if there are any elements in the collection</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerable<TValType> enumerable, CancellationToken ct)
        {
            return enumerable.GetAsyncEnumerator().AnyAsync(ct);
        }

        /// <summary>
        /// Checks if there are any elements in a collection
        /// </summary>
        /// <typeparam name="TValType">the type of the collection</typeparam>
        /// <param name="enumerable">the enumerator to check</param>
        /// <returns>if there are any elements in the collection</returns>
        public static Task<bool> AnyAsync<TValType>(this IAsyncEnumerable<TValType> enumerable)
        {
            return enumerable.GetAsyncEnumerator().AnyAsync();
        }
        
    }
}