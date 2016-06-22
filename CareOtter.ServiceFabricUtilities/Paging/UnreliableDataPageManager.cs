using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    public class UnreliableDataPageManager : IDataPageManager
    {
        private ConcurrentDictionary<PagingId, PagedDataRecieverPackage> _dataPackages 
            = new ConcurrentDictionary<PagingId, PagedDataRecieverPackage>();
        private readonly PagingIdManager _idManager = new PagingIdManager();
        private readonly object _thisLock = new object();

        public UnreliableDataPageManager() { }


        public Task<PagingId> BeginPagingData(int originDataLength)
        {
            lock (_thisLock)
            {
                var dataPackage = new PagedDataRecieverPackage(originDataLength);
                var id = new PagingId() {Id = _idManager.GetId()};
                _dataPackages.TryAdd(id, dataPackage);
                return Task.FromResult(id);
            }
        }

        public Task SendDataPage(PagingId id, byte[] page)
        {
            lock (_thisLock)
            {
                if (!_dataPackages.ContainsKey(id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for SendDataPage");

                _dataPackages[id].AddData(page);
                return Task.FromResult(true);
            }
        }

        public Task<T> GetDataAs<T>(PagingId id)
        {
            lock (_thisLock)
            {
                if (!_dataPackages.ContainsKey(id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for GetDataAs");


                return Task.FromResult(PagingSerializer.Deserialize<T>(_dataPackages[id].Data));
            }
        }

        public Task EndPagingData(PagingId id)
        {
            lock (_thisLock)
            {
                if (!_dataPackages.ContainsKey(id))
                    throw new ArgumentException($"Invalid id provided to DataPageManager ({id}) for EndPagingData");

                PagedDataRecieverPackage outRecieverPackage;
                _dataPackages.TryRemove(id, out outRecieverPackage);
                _idManager.FreeId(id.Id);
                return Task.FromResult(true);
            }
        }


        private readonly ConcurrentDictionary<PagingId, PagedDataSenderPackage> _requestedPackageDictionary =
            new ConcurrentDictionary<PagingId, PagedDataSenderPackage>();
        public Task<RequestedPagedDataSession> PrepareDataForPaging<T>(T payload)
        {
            var id = new PagingId() {Id = _idManager.GetId()};
            var senderPackage = new PagedDataSenderPackage();
            senderPackage.Initialize<T>(payload,65000);

            _requestedPackageDictionary.AddOrUpdate(id, key => senderPackage, (key, value) => senderPackage);

            return Task.FromResult(new RequestedPagedDataSession()
            {
                OriginDataSize = senderPackage.GetOriginDataLength(),
                SessionId = id
            });
        }

        public Task<ConditionalValue<byte[]>> GetPage(PagingId id, int pageNum)
        {
            PagedDataSenderPackage outSenderPackage;
            var success = _requestedPackageDictionary.TryGetValue(id, out outSenderPackage);
            if (!success)
                return Task.FromResult(new ConditionalValue<byte[]>(false, null));
            return Task.FromResult(outSenderPackage.GetDataAtPage(pageNum));
        }

        public Task NotifyPagingSessionComplete(PagingId id)
        {
            _idManager.FreeId(id.Id);
            PagedDataSenderPackage outVal;
            _requestedPackageDictionary.TryRemove(id, out outVal);
            return Task.FromResult(true);
        }
    }
}