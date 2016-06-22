using System;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncTakeExtensions
    {
        /// <summary>
        /// Takes elements from a collection while the lamda is satisfied
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to take from</param>
        /// <param name="takeFunc">lamda to determine if we should take</param>
        /// <returns>enumerator with the taken elements</returns>
        public static IAsyncEnumerator<TValType> TakeWhile<TValType>(this IAsyncEnumerator<TValType> enumerator,
            Func<TValType, bool> takeFunc)
        {
            return new AsyncTakeWhileEnumerator<TValType>(enumerator,takeFunc);
        }

        /// <summary>
        /// Takes elements from a collection while the lamda is satisfied
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to take from</param>
        /// <param name="takeFunc">lamda to determine if we should take</param>
        /// <returns>enumerator with the taken elements</returns>
        public static IAsyncEnumerator<TValType> TakeWhile<TValType>(this IAsyncEnumerable<TValType> enumerable,
            Func<TValType, bool> takeFunc)
        {
            return enumerable.GetAsyncEnumerator().TakeWhile(takeFunc);
        }

        /// <summary>
        /// Takes N elements from a collection 
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to take from</param>
        /// <param name="take">how many elements to take</param>
        /// <returns>enumerator with the taken elements</returns>
        public static IAsyncEnumerator<TValType> Take<TValType>(this IAsyncEnumerator<TValType> enumerator, int take)
        {
            var counter = 0;
            return enumerator.TakeWhile(x =>
            {
                ++counter;
                return counter <= take;
            });
        }

        /// <summary>
        /// Takes N elements from a collection 
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to take from</param>
        /// <param name="take">how many elements to take</param>
        /// <returns>enumerator with the taken elements</returns>
        public static IAsyncEnumerator<TValType> Take<TValType>(this IAsyncEnumerable<TValType> enumerable, int take)
        {
            return enumerable.GetAsyncEnumerator().Take(take);
        } 
    }
}