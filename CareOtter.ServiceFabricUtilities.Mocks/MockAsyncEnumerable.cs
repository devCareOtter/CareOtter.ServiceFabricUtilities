using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.UnitTest.Mocks
{
    public class MockAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IList<T> _sourceList;
        private readonly Action _moveNextAction;
        public MockAsyncEnumerable(IList<T> sourceList, Action moveNextAction = null )
        {
            _sourceList = sourceList;
            _moveNextAction = moveNextAction;
        } 
        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new MockAsyncEnumerator<T>(_sourceList,_moveNextAction);
        }
    }

    public class MockAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IList<T> _list;
        private readonly Action _moveNextAction;
        public MockAsyncEnumerator(IList<T> sourceList, Action moveNextAction)
        {
            _list = sourceList;
            _moveNextAction = moveNextAction;
        } 

        public void Dispose()
        {
            //do nothing
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            _moveNextAction?.Invoke();
            curIdx++;
            if (curIdx >= _list.Count)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public void Reset()
        {
            curIdx = -1;
        }

        private int curIdx = -1;
        public T Current => _list[curIdx];
    }
}