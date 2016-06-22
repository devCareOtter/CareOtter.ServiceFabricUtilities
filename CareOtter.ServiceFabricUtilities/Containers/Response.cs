using System.Runtime.Serialization;

namespace CareOtter.ServiceFabricUtilities.Containers
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public bool Success { get; set; }
    }

    [DataContract]
    public class Response<TValueType>
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public TValueType ResultValue { get; set; }
    }
}