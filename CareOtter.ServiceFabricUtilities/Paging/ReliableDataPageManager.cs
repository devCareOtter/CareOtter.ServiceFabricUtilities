using System;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    public class ReliableDataPageManager : IDataPageManager
    {
        private readonly IReliableStateManager _stateManager;
        private readonly PagingIdManager _idManager = new PagingIdManager();
        private readonly object _thisLock = new object();
        private IReliableDictionary<PagingId, PagedDataRecieverPackage> _recievedPageDict;
        

        private const string PagedDataRecieverDictKey = "PagedDataRecieverDictKey";

        public ReliableDataPageManager(IReliableStateManager stateManager)
        {
            if(stateManager == null)
                throw new ArgumentNullException(nameof(stateManager));

            _stateManager = stateManager;
            
        }

        private async Task<IReliableDictionary<PagingId, PagedDataRecieverPackage>> GetRecieverDict()
        {
            if(_recievedPageDict == null)
                _recievedPageDict = await _stateManager.GetOrAddAsync<IReliableDictionary<PagingId, PagedDataRecieverPackage>>(
                    PagedDataRecieverDictKey);

            return _recievedPageDict;
        }

        public async Task<PagingId> BeginPagingData(int originDataLength)
        {
            var dict = await GetRecieverDict();
            var dataPackage = new PagedDataRecieverPackage(originDataLength);
            var id = new PagingId() {Id = _idManager.GetId()};
            using (var tx = _stateManager.CreateTransaction())
            {
                await dict.AddOrUpdateAsync(tx, id, newId => dataPackage, (oldId, package) => dataPackage);
                await tx.CommitAsync();
            }
            return id;
        }

        public async Task SendDataPage(PagingId id, byte[] page)
        {
            var dict = await GetRecieverDict();
            using (var tx = _stateManager.CreateTransaction())
            {
                if(!await dict.ContainsKeyAsync(tx,id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for SendDataPage");

                var oldPackage = await dict.TryGetValueAsync(tx, id);
                if(!oldPackage.HasValue)
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for SendDataPage");

                oldPackage.Value.AddData(page);

                await dict.SetAsync(tx, id, oldPackage.Value);
                await tx.CommitAsync();
            }
        }

        public async Task<T> GetDataAs<T>(PagingId id)
        {
            var dict = await GetRecieverDict();
            using (var tx = _stateManager.CreateTransaction())
            {
                if(!await dict.ContainsKeyAsync(tx,id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for GetDataAs");

                var data = await dict.TryGetValueAsync(tx, id);
                if(!data.HasValue)
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for GetDataAs");

                return PagingSerializer.Deserialize<T>(data.Value.Data);
            }
        }

        

        public async Task EndPagingData(PagingId id)
        {
            var dict = await GetRecieverDict();
            using (var tx = _stateManager.CreateTransaction())
            {
                if(!await dict.ContainsKeyAsync(tx,id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for EndPagingData");

                await dict.TryRemoveAsync(tx, id);
                _idManager.FreeId(id.Id);
            }
        }

        private IReliableDictionary<PagingId, PagedDataSenderPackage> _requestedPackageDictionary;
        private const string RequestedPackageDictionaryKey = "RequestedPackageDictionary";
        private async Task<IReliableDictionary<PagingId, PagedDataSenderPackage>> GetRequestedPackageDictionary()
        {
            if (_requestedPackageDictionary != null)
                return _requestedPackageDictionary;

            _requestedPackageDictionary =
                await _stateManager.GetOrAddAsync<IReliableDictionary<PagingId, PagedDataSenderPackage>>(
                    RequestedPackageDictionaryKey);

            return _requestedPackageDictionary;
        }
        public async Task<RequestedPagedDataSession> PrepareDataForPaging<T>(T payload)
        {
            var senderPackage = new PagedDataSenderPackage();
            senderPackage.Initialize<T>(payload,65000);
            var id = new PagingId() {Id = _idManager.GetId() };
            var dict = await GetRequestedPackageDictionary();
            using (var tx = _stateManager.CreateTransaction())
            {
                await dict.AddOrUpdateAsync(tx, id, key => senderPackage, (key, oldVal) => senderPackage);
                await tx.CommitAsync();
            }

            return new RequestedPagedDataSession()
            {
                OriginDataSize = senderPackage.GetOriginDataLength(),
                SessionId = id
            };
        }

        public async Task<ConditionalValue<byte[]>> GetPage(PagingId id,int pageNum)
        {
            var dict = await GetRequestedPackageDictionary();
            using (var tx = _stateManager.CreateTransaction())
            {
                var package = await dict.TryGetValueAsync(tx, id);
                if(!package.HasValue)
                    return new ConditionalValue<byte[]>(false, null);

                return package.Value.GetDataAtPage(pageNum);
            }
        }

        public async Task NotifyPagingSessionComplete(PagingId id)
        {
            var dict = await GetRequestedPackageDictionary();
            using (var tx = _stateManager.CreateTransaction())
            {
                if (!await dict.ContainsKeyAsync(tx, id))
                    return;

                await dict.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }
            _idManager.FreeId(id.Id);
        }
    }
}