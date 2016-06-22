//Keeping this around for potential future usage, aut as of right now these will enumerate the entire collection
//rx sucks.


//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.ServiceFabric.Data;

//namespace CareOtter.ServiceFabricUtilities.Extensions
//{
//    public static class AsyncEnumberableObservableExtensions
//    {
//        /// <summary>
//        /// pushes the elem to the observer if it passes the func
//        /// </summary>
//        /// <typeparam name="TEnumVal">type of the collection</typeparam>
//        /// <typeparam name="TResultVal">type of the resulting collection</typeparam>
//        /// <param name="enumerable">enumerable to query</param>
//        /// <param name="func">func to select from</param>
//        /// <returns></returns>
//        public static IObservable<TResultVal> Select<TEnumVal,TResultVal>(
//            this IAsyncEnumerator<TEnumVal> enumerable, Func<TEnumVal, TResultVal> func)
//        {
//            return System.Reactive.Linq.Observable.Create<TResultVal>(async (observer, token) =>
//            {
//                while ((await enumerable.MoveNextAsync(token)))
//                {
//                    observer.OnNext(func(enumerable.Current));
//                }
//            });
//        }

//        /// <summary>
//        /// Helper to select from the enumerator
//        /// </summary>
//        /// <typeparam name="TEnumVal">collection type</typeparam>
//        /// <typeparam name="TResultVal">result type</typeparam>
//        /// <param name="enumerable">enumerable to select from</param>
//        /// <param name="func">func to select from</param>
//        /// <returns></returns>
//        public static IObservable<TResultVal> Select<TEnumVal,TResultVal>(
//            this IAsyncEnumerable<TEnumVal> enumerable, Func<TEnumVal,TResultVal> func)
//        {
//            return enumerable.GetAsyncEnumerator().Select(func);
//        }

//        /// <summary>
//        /// Helper to add a where clause to the enumerator
//        /// </summary>
//        /// <typeparam name="TValType">val type of the origin collection</typeparam>
//        /// <param name="enumerable">enumerator to pull from</param>
//        /// <param name="func">func to determine if we should select</param>
//        /// <returns>observable to return the vals</returns>
//        public static IObservable<TValType> Where<TValType>(this IAsyncEnumerator<TValType> enumerable,
//            Func<TValType, bool> func)
//        {
//            return System.Reactive.Linq.Observable.Create<TValType>(async (observer, token) =>
//            {
//                while (await enumerable.MoveNextAsync(token))
//                {
//                    if (func(enumerable.Current))
//                    {
//                        observer.OnNext(enumerable.Current);
//                    }
//                }
//            });
//        }

//        /// <summary>
//        /// helper to add a where clause to the enumerator
//        /// </summary>
//        /// <typeparam name="TValType">val type of origin collection</typeparam>
//        /// <param name="enumerable">enumerable to pull from</param>
//        /// <param name="func">func to determine if we should select</param>
//        /// <returns>observable to return the vals</returns>
//        public static IObservable<TValType> Where<TValType>(this IAsyncEnumerable<TValType> enumerable,
//            Func<TValType, bool> func)
//        {
//            return enumerable.GetAsyncEnumerator().Where(func);
//        }

//        /// <summary>
//        /// returns an observable wrapping this enumerator
//        /// </summary>
//        /// <typeparam name="TValType">val type of the enumerator</typeparam>
//        /// <param name="enumerator">enumerator to fetch from</param>
//        /// <returns>an observable wrapping the enumerator</returns>
//        public static IObservable<TValType> ToObservable<TValType>(this IAsyncEnumerator<TValType> enumerator)
//        {
//            return System.Reactive.Linq.Observable.Create<TValType>(async (observer, token) =>
//            {
//                while (await enumerator.MoveNextAsync(token))
//                {
//                    observer.OnNext(enumerator.Current);
//                }
//            });
//        }

//        /// <summary>
//        /// returns an observable wrapping this enumerator
//        /// </summary>
//        /// <typeparam name="TValType">val type of the observable</typeparam>
//        /// <param name="enumerable">enumerable to fetch from</param>
//        /// <returns>an observable wrapping the enumerator</returns>
//        public static IObservable<TValType> ToObservable<TValType>(this IAsyncEnumerable<TValType> enumerable)
//        {
//            return enumerable.GetAsyncEnumerator().ToObservable();
//        } 
//    }
//}