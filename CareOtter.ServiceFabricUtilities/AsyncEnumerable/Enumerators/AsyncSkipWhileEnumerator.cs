using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators
{
    public class AsyncSkipWhileEnumerator<TValType> : IAsyncEnumerator<TValType>
    {

        private readonly IAsyncEnumerator<TValType> _parentEnumerator;
        private readonly Func<TValType, bool> _skipFunc;

        public AsyncSkipWhileEnumerator(IAsyncEnumerator<TValType> parentEnumerator, Func<TValType, bool> skipFunc)
        {
            _parentEnumerator = parentEnumerator;
            _skipFunc = skipFunc;
        } 
          
        public void Dispose()
        {
            _parentEnumerator.Dispose();
        }

        private bool hasSkipped = false;
        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (!hasSkipped) // if this hasn't skipped yet skip until the skipFunc is happy
            {
                var lastSkipResult = await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false);
                while (lastSkipResult && _skipFunc(_parentEnumerator.Current))
                {
                    lastSkipResult = await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false);
                }
                if (!lastSkipResult) //if we broke out of the loop because we couldn't skip any more then return false
                {
                    return false;
                }
            }

            if (hasSkipped) //if this isn't our first iter then move on and return that result
            {
                return await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false);
            }

            hasSkipped = true;
            return true;

        }

        public void Reset()
        {
            _parentEnumerator.Reset();
        }

        public TValType Current => _parentEnumerator.Current;
    }
}