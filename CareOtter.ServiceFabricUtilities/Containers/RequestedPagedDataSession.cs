using System;
using System.Runtime.Serialization;

namespace CareOtter.ServiceFabricUtilities.Containers
{
    [DataContract]
    public class RequestedPagedDataSession
    {
        [DataMember]
        public PagingId SessionId { get; set; }
        [DataMember]
        public int OriginDataSize { get; set; }
        [DataMember]
        public Uri OriginUri { get; set; }
    }
}