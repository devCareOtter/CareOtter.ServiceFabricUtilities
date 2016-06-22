using System;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncEnumerableSkipExtensions
    {
        /// <summary>
        /// Skips over elements in the collection while the lamda returns true
        /// </summary>
        /// <typeparam name="TResultVal">type of the collection</typeparam>
        /// <param name="enumerator">enumerator to skip</param>
        /// <param name="skipFunc">function to determine if we should skip</param>
        /// <returns>enumerator with requested elements skipped</returns>
        public static IAsyncEnumerator<TResultVal> SkipWhile<TResultVal>(this IAsyncEnumerator<TResultVal> enumerator,
            Func<TResultVal, bool> skipFunc)
        {
            return new AsyncSkipWhileEnumerator<TResultVal>(enumerator,skipFunc);
        }

        /// <summary>
        /// Skips over elements in the collection while the lamda returns true
        /// </summary>
        /// <typeparam name="TResultVal">type of the collection</typeparam>
        /// <param name="enumerable">collection to skip over</param>
        /// <param name="skipFunc">function to determine if we should skip</param>
        /// <returns>enumerator with requested elements skipped</returns>
        public static IAsyncEnumerator<TResultVal> SkipWhile<TResultVal>(this IAsyncEnumerable<TResultVal> enumerable,
            Func<TResultVal, bool> skipFunc)
        {
            return enumerable.GetAsyncEnumerator().SkipWhile(skipFunc);
        }

        /// <summary>
        /// Skips over N elements in the collection 
        /// </summary>
        /// <typeparam name="TResultVal">type of the collection</typeparam>
        /// <param name="enumerator">enumerator to skip</param>
        /// <param name="skip">number of elements to skip</param>
        /// <returns>enumerator with requested elements skipped</returns>
        public static IAsyncEnumerator<TResultVal> Skip<TResultVal>(this IAsyncEnumerator<TResultVal> enumerator,
            int skip)
        {
            int counter = 0;
            return enumerator.SkipWhile(x =>
            {
                ++counter;
                return counter <= skip;
            });
        }

        /// <summary>
        /// Skips over N elements in the collection 
        /// </summary>
        /// <typeparam name="TResultVal">type of the collection</typeparam>
        /// <param name="enumerable">enumerator to skip</param>
        /// <param name="skip">number of elements to skip</param>
        /// <returns>enumerator with requested elements skipped</returns>
        public static IAsyncEnumerator<TResultVal> Skip<TResultVal>(this IAsyncEnumerable<TResultVal> enumerable,
            int skip)
        {
            return enumerable.GetAsyncEnumerator().Skip(skip);
        } 
    }
}