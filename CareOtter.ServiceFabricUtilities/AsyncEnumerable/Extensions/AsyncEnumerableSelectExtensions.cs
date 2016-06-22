using System;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncEnumerableSelectExtensions
    {
        /// <summary>
        /// Selects values from an async enumerator
        /// </summary>
        /// <typeparam name="TValType">The source enumerators value type</typeparam>
        /// <typeparam name="TResultType">The result enumerators value type</typeparam>
        /// <param name="sourceEnumerator">The source enumerator to select from</param>
        /// <param name="selectFunc">the func used to select values</param>
        /// <returns>An enumerator that selects the values</returns>
        public static IAsyncEnumerator<TResultType> Select<TValType, TResultType>(
            this IAsyncEnumerator<TValType> sourceEnumerator, Func<TValType, TResultType> selectFunc)
        {
            return new AsyncSelectEnumerator<TValType,TResultType>(sourceEnumerator,selectFunc);
        }

        /// <summary>
        /// Selects values from an async enumerator
        /// </summary>
        /// <typeparam name="TValType">The source enumerators value type</typeparam>
        /// <typeparam name="TResultType">The result enumerators value type</typeparam>
        /// <param name="sourceEnumerable">The source enumerable to select from</param>
        /// <param name="selectFunc">the func used to select values</param>
        /// <returns>An enumerator that selects the values</returns>
        public static IAsyncEnumerator<TResultType> Select<TValType, TResultType>(
            this IAsyncEnumerable<TValType> sourceEnumerable, Func<TValType, TResultType> selectFunc)
        {
            return sourceEnumerable.GetAsyncEnumerator().Select(selectFunc);
        }   
    }
}