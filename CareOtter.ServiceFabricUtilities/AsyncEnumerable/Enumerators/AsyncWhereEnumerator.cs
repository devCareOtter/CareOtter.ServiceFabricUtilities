using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators
{
    public class AsyncWhereEnumerator<TValType> : IAsyncEnumerator<TValType>
    {
        private readonly IAsyncEnumerator<TValType> _parentEnumerator;
        private readonly Func<TValType, bool> _whereFunc;

        internal AsyncWhereEnumerator(IAsyncEnumerator<TValType> parentEnumerator, Func<TValType, bool> whereFunc)
        {
            _parentEnumerator = parentEnumerator;
            _whereFunc = whereFunc;
        } 

          
        public void Dispose()
        {
            _parentEnumerator.Dispose();
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            await _parentEnumerator.MoveNextAsync(cancellationToken);
            while (!_whereFunc(_parentEnumerator.Current)) //while the where func doesn't pass keep moving on
            {
                if (!(await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))) //if the parent runs out, fail
                {
                    return false;
                }
            }
            return true;
        }

        public void Reset()
        {
            _parentEnumerator.Reset();
        }

        public TValType Current => _parentEnumerator.Current;
    }
}