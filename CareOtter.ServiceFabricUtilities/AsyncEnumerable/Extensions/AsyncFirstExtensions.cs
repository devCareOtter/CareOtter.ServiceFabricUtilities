using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncFirstExtensions
    {
        //first
        //func
        //enumerator
        /// <summary>
        /// gets the first element that satisfies the lambda
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to get the first elem from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <param name="firstFunc">func to check for first satisfied elem</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static async Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            CancellationToken ct, Func<TValueType, bool> firstFunc)
        {
            while (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                if (firstFunc(enumerator.Current))
                    return enumerator.Current;
            }
            throw new ArgumentException("Element not found in collection");
        }

        /// <summary>
        /// gets the first element that satisfies the lambda
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to get the first elem from</param>
        /// <param name="firstFunc">func to check for first satisfied elem</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            Func<TValueType, bool> firstFunc)
        {
            return enumerator.FirstAsync(CancellationToken.None, firstFunc);
        }
        //enumerable
        /// <summary>
        /// gets the first element that satisfies the lambda
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to get the first elem from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <param name="firstFunc">func to check for first satisfied elem</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            CancellationToken ct, Func<TValueType, bool> firstFunc)
        {
            return enumerable.GetAsyncEnumerator().FirstAsync(ct, firstFunc);
        }

        /// <summary>
        /// gets the first element that satisfies the lambda
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to get the first elem from</param>
        /// <param name="firstFunc">func to check for first satisfied elem</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            Func<TValueType, bool> firstFunc)
        {
            return enumerable.GetAsyncEnumerator().FirstAsync(firstFunc);
        }

        //no func
        /// <summary>
        /// gets the first element in the collection
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to get the first elem from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            CancellationToken ct)
        {
            return enumerator.FirstAsync(ct, x => true);
        }

        /// <summary>
        /// gets the first element in the collection
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to get the first elem from</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator)
        {
            return enumerator.FirstAsync(CancellationToken.None);
        }
        //enumerable
        /// <summary>
        /// gets the first element in the collection
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to get the first elem from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            CancellationToken ct)
        {
            return enumerable.GetAsyncEnumerator().FirstAsync(ct);
        }

        /// <summary>
        /// gets the first element in the collection
        /// </summary>
        /// <typeparam name="TValueType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to get the first elem from</param>
        /// <returns>the first elem that satisfied the lambda</returns>
        /// <exception cref="ArgumentException">If no elements were found</exception>
        public static Task<TValueType> FirstAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable)
        {
            return enumerable.GetAsyncEnumerator().FirstAsync();
        }


        //First or default
        //func
        //enuerator
        /// <summary>
        /// Gets the first element satisfying the lambda or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerator">enumerator to fech from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <param name="firstFunc">func to determine first element desired</param>
        /// <returns>first element or default if not found</returns>
        public static async Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            CancellationToken ct, Func<TValueType, bool> firstFunc)
        {
            while (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                if (firstFunc(enumerator.Current))
                    return enumerator.Current;
            }
            return default(TValueType);
        }

        /// <summary>
        /// Gets the first element satisfying the lambda or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerator">enumerator to fech from</param>
        /// <param name="firstFunc">func to determine first element desired</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            Func<TValueType, bool> firstFunc)
        {
            return enumerator.FirstOrDefaultAsync(CancellationToken.None, firstFunc);
        }
        //enumerable
        /// <summary>
        /// Gets the first element satisfying the lambda or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerable">enumerator to fech from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <param name="firstFunc">func to determine first element desired</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            CancellationToken ct, Func<TValueType, bool> firstFunc)
        {
            return enumerable.GetAsyncEnumerator().FirstOrDefaultAsync(ct, firstFunc);
        }

        /// <summary>
        /// Gets the first element satisfying the lambda or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerable">enumerator to fech from</param>
        /// <param name="firstFunc">func to determine first element desired</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            Func<TValueType, bool> firstFunc)
        {
            return enumerable.GetAsyncEnumerator().FirstOrDefaultAsync(firstFunc);
        }

        //No func
        //enumerator
        /// <summary>
        /// Gets the first element in the collection or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerator">enumerator to fech from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator,
            CancellationToken ct)
        {
            return enumerator.FirstOrDefaultAsync(ct, x => true);
        }

        /// <summary>
        /// Gets the first element in the collection or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerator">enumerator to fech from</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerator<TValueType> enumerator)
        {
            return enumerator.FirstOrDefaultAsync(CancellationToken.None);
        }
        //enumerable
        /// <summary>
        /// Gets the first element in the collection or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerable">enumerator to fech from</param>
        /// <param name="ct">cancellation token for async operations</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable,
            CancellationToken ct)
        {
            return enumerable.GetAsyncEnumerator().FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Gets the first element in the collection or returns default(TValueType)
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="enumerable">enumerator to fech from</param>
        /// <returns>first element or default if not found</returns>
        public static Task<TValueType> FirstOrDefaultAsync<TValueType>(this IAsyncEnumerable<TValueType> enumerable)
        {
            return enumerable.GetAsyncEnumerator().FirstOrDefaultAsync();
        }

    }
}