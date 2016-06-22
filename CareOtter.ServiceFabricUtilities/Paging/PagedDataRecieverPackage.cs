using System;
using System.Runtime.Serialization;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    [DataContract]
    public class PagedDataRecieverPackage
    {
        [DataMember]
        public byte[] _data { get; set; }
        [DataMember]
        public int _dataPos { get; set; }

        public byte[] Data => _data;

        public PagedDataRecieverPackage(int originDataLength)
        {
            _dataPos = 0;
            _data = new byte[originDataLength];
        }

        public void AddData(byte[] data)
        {
            var dataLen = ((data.Length + _dataPos) > _data.Length)
                ? _data.Length - _dataPos
                : data.Length;

            Buffer.BlockCopy(data,0,_data,_dataPos,dataLen);
            _dataPos += dataLen;
        }
    }
}