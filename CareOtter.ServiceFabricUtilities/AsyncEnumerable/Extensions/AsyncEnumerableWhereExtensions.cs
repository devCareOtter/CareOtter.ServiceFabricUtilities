using System;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncEnumerableWhereExtensions
    {
        /// <summary>
        /// Gets elements from the collection that satisfy a lambda
        /// </summary>
        /// <typeparam name="TValType">value type for the collection</typeparam>
        /// <param name="parentEnumerator">enumerator to get elements from</param>
        /// <param name="whereFunc">lambda to satisfy</param>
        /// <returns>enumerator with filtered elements</returns>
        public static IAsyncEnumerator<TValType> Where<TValType>(this IAsyncEnumerator<TValType> parentEnumerator,
            Func<TValType, bool> whereFunc)
        {
            return new AsyncWhereEnumerator<TValType>(parentEnumerator,whereFunc);
        }

        /// <summary>
        /// Gets elements from the collection that satisfy a lambda
        /// </summary>
        /// <typeparam name="TValueType">value type for the collection</typeparam>
        /// <param name="parentEnumerable">enumerator to get elements from</param>
        /// <param name="whereFunc">lambda to satisfy</param>
        /// <returns>enumerator with filtered elements</returns>
        public static IAsyncEnumerator<TValueType> Where<TValueType>(this IAsyncEnumerable<TValueType> parentEnumerable,
            Func<TValueType, bool> whereFunc)
        {
            return parentEnumerable.GetAsyncEnumerator().Where(whereFunc);
        } 
    }
}