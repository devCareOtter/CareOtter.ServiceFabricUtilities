using System;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Services;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    public class PagingRequestSession<TResultType> : IPagingRequestSession<TResultType> 
        where TResultType : class //paging with value types is dumb, just send that crap the normal way
    {
        private readonly IDataPageSender _parentSender;
        private readonly byte[] _byteBuffer;
        private int _bufferPos = 0;
        private readonly PagingId _currentSessionId;
       

        public PagingRequestSession(IDataPageSender parentSender, RequestedPagedDataSession session)
        {
            _parentSender = parentSender;
            //TODO: Get the byte buffer from a memory pool
            _byteBuffer = new byte[session.OriginDataSize];
            _currentSessionId = session.SessionId;
        }

        public PagingRequestSession(RequestedPagedDataSession session, ICareOtterServiceProxyFactory proxyFactory) 
            :this(proxyFactory.Create<IDataPageSender>(session.OriginUri),session)
        {}

        public async Task<TResultType> ExecuteAsync(CancellationToken ct)
        {
            var curPage = 0;
            var page = await _parentSender.GetPageAsync(_currentSessionId, curPage);
            while (page.Success)
            {
                for (var i = 0; i < page.ResultValue.Length; i++)
                    _byteBuffer[_bufferPos + i] = page.ResultValue[i];

                _bufferPos += page.ResultValue.Length;
                page = await _parentSender.GetPageAsync(_currentSessionId, ++curPage);
            }
            await _parentSender.NotifyPagingComplete(_currentSessionId);
            return PagingSerializer.Deserialize<TResultType>(_byteBuffer);
        }
    }
}