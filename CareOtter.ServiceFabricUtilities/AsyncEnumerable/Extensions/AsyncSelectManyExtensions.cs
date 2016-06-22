using System;
using System.Collections.Generic;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions
{
    public static class AsyncSelectManyExtensions
    {
        /// <summary>
        /// Selects many elements from a collection inside of the value type
        /// </summary>
        /// <typeparam name="TValType">type of the enumerated collection</typeparam>
        /// <typeparam name="TCollectionType">collection type being selected in <see cref="TValType">TValType</see></typeparam>
        /// <typeparam name="TResultType">the result type being pulled from the collections</typeparam>
        /// <param name="enumerator">enumerator to select many from</param>
        /// <param name="selectFunc">the lamda to select with</param>
        /// <returns>a collection containing all the elements in the selected collection</returns>
        public static IAsyncEnumerator<TResultType> SelectMany<TValType, TCollectionType, TResultType>(
            this IAsyncEnumerator<TValType> enumerator, Func<TValType, TCollectionType> selectFunc) 
            where TCollectionType : IEnumerable<TResultType>
        {
            return new AsyncSelectManyEnumerator<TValType,TResultType,TCollectionType>(enumerator,selectFunc);
        }

        /// <summary>
        /// Selects many elements from a collection inside of the value type
        /// </summary>
        /// <typeparam name="TValType">type of the enumerated collection</typeparam>
        /// <typeparam name="TCollectionType">collection type being selected in <see cref="TValType">TValType</see></typeparam>
        /// <typeparam name="TResultType">the result type being pulled from the collections</typeparam>
        /// <param name="enumerator">enumerator to select many from</param>
        /// <param name="selectFunc">the lamda to select with</param>
        /// <returns>a collection containing all the elements in the selected collection</returns>
        public static IAsyncEnumerator<TResultType> SelectMany<TValType, TCollectionType, TResultType>(
            this IAsyncEnumerable<TValType> enumerator, Func<TValType, TCollectionType> selectFunc)
            where TCollectionType : IEnumerable<TResultType>
        {
            return enumerator.GetAsyncEnumerator().SelectMany<TValType, TCollectionType, TResultType>(selectFunc);
        }    
    }
}