using System;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    [DataContract]
    public class PagedDataSenderPackage
    {
        [DataMember]
        public byte[] Data { get; set; }

        [DataMember]
        public int CurrentPageNumber { get; set; } = 0;
        [DataMember]
        public int BytesPerPage { get; set; }
        [DataMember]
        public int TotalNumberOfPages { get; set; }

        public PagedDataSenderPackage() { }
        
        public void Initialize<T>(T objectToSend, int bytesPerPage)
        {
            Data = PagingSerializer.Serialize<T>(objectToSend);
            BytesPerPage = bytesPerPage;
            TotalNumberOfPages = Data.Length / BytesPerPage;
        }

        public ConditionalValue<byte[]> GetNextPage()
        {
            var ret = GetDataAtPage(CurrentPageNumber);
            if(ret.HasValue)
                CurrentPageNumber++;
            return ret;
        }

        public ConditionalValue<byte[]> GetDataAtPage(int page)
        {
            if(page > TotalNumberOfPages)
                return new ConditionalValue<byte[]>(false, null);

            var offset = page*BytesPerPage;
            var copyLen = ( (offset + BytesPerPage) > Data.Length ) ? 
                Data.Length - offset : 
                BytesPerPage;
            var ret = new ConditionalValue<byte[]>(true,new byte[copyLen]);
            Buffer.BlockCopy(Data,offset,ret.Value,0,copyLen);
            return ret;
        }

        public int GetOriginDataLength()
        {
            return Data.Length;
        }
    }
}