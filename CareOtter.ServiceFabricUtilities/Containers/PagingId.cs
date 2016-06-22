using System;
using System.Runtime.Serialization;
using CareOtter.ServiceFabricUtilities.Paging;

namespace CareOtter.ServiceFabricUtilities.Containers
{
    //encapsulates ID functionality for paging requests across services
    [DataContract]
    public struct PagingId : IEquatable<PagingId>, IComparable<PagingId>
    {
        [DataMember]
        public ushort Id { get; set; }

        public bool Equals(PagingId other)
        {
            return other.Id == Id;
        }

        public int CompareTo(PagingId other)
        {
            return Id.CompareTo(other.Id);
        }

        public override string ToString()
        {
            return $"Page-{Id}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
    }
}