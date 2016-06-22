using System;
using System.Runtime.Serialization;

namespace CareOtter.ServiceFabricUtilities.Containers
{ 
    [DataContract]
    public class FabricServiceId
    {
        [DataMember]
        public Uri ServiceUri { get; private set; }
        [DataMember]
        public Uri ApplicationNameUri { get; private set; }

        /// <summary>
        /// Forms the Uri required to address this service instance
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="serviceType"></param>
        /// <param name="instanceId"></param>
        public FabricServiceId(string applicationName, string serviceType, string instanceId)
        {
            //scrub this to make it properlyformatted
            ServiceUri = new Uri(string.Format("{0}/{1}/{2}", applicationName, serviceType, instanceId.Replace('|', '/').Replace('.', '/')).TrimEnd('/'));
            ApplicationNameUri = new Uri(applicationName);
        }

        /// <summary>
        /// Forms the Uri required to address this service instance
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="serviceType"></param>
        /// <param name="instanceId"></param>
        public FabricServiceId(Uri applicationName, string serviceType, string instanceId) :
            this(applicationName.ToString(), serviceType, instanceId)
        {

        }

        /// <summary>
        /// Must include the application name, servicename, and then the instance id.
        /// </summary>
        /// <param name="id"></param>
        public FabricServiceId(Uri id)
        {
            ServiceUri = id;
        }
    }
}
