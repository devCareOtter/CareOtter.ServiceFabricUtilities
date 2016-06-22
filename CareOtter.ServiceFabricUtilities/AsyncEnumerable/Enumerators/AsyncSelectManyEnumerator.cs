using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators
{
    public class AsyncSelectManyEnumerator<TValType,TResultType, TCollectionType> : IAsyncEnumerator<TResultType> 
        where TCollectionType : IEnumerable<TResultType> 
    {
        private readonly IAsyncEnumerator<TValType> _parentEnumerator;
        private readonly Func<TValType, TCollectionType> _selectFunc;  
        public AsyncSelectManyEnumerator(IAsyncEnumerator<TValType> parentEnumerator,
            Func<TValType, TCollectionType> selectFunc)
        {
            _parentEnumerator = parentEnumerator;
            _selectFunc = selectFunc;
        }  
        public void Dispose()
        {
            _parentEnumerator.Dispose();
        }

        //current child collection elem we're recursing over
        //null is a flag that we need to select one again from the parent collection
        private IEnumerator<TResultType> _curCollection = null;

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            long maxAttempts = 0;
            while (maxAttempts < long.MaxValue) //this is a hell of a lot of iterations, should be more than enough to prevent an infinite loop.
            {
                if (_curCollection == null) //if cur collection is null, get the next one from the selector
                {
                    if (!(await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false)))
                    {
                        return false;
                    }
                    _curCollection = _selectFunc(_parentEnumerator.Current).GetEnumerator();
                }

                if (!_curCollection.MoveNext()) //if we can't select another from cur collection, set it to null & try this fxn again
                {
                    _curCollection = null;
                    ++maxAttempts;
                    continue;
                }
                return true; //we could successfully move next so return true
            }
            throw new ArgumentOutOfRangeException("AsyncSelectManyEnumerator couldn't find a valid enumerator after Int.Max retries.");
        }

        public void Reset()
        {
            _parentEnumerator.Reset();
        }

        public TResultType Current => _curCollection.Current;
    }
}