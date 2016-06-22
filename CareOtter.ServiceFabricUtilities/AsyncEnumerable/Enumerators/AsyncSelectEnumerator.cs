using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators
{
    public class AsyncSelectEnumerator<TValType,TResultType> : IAsyncEnumerator<TResultType>
    {
        private readonly IAsyncEnumerator<TValType> _parentEnumerator;
        private readonly Func<TValType, TResultType> _selectFunc;

        internal AsyncSelectEnumerator(IAsyncEnumerator<TValType> parentEnumerator, Func<TValType, TResultType> selectFunc)
        {
            _parentEnumerator = parentEnumerator;
            _selectFunc = selectFunc;
        } 

        public void Dispose()
        {
            _parentEnumerator.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return _parentEnumerator.MoveNextAsync(cancellationToken);
        }

        public void Reset()
        {
            _parentEnumerator.Reset();
        }

        public TResultType Current => _selectFunc(_parentEnumerator.Current);
    }
}