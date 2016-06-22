using System.Net.PeerToPeer.Collaboration;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Wrapper for data being observed
    /// </summary>
    [DataContract]
    public class MessageWrapper
    {
        /// <summary>
        /// Creates a message wrapper to be sent to observers
        /// </summary>
        /// <param name="payload">object to send to the observers</param>
        /// <param name="messageSource">where the message is coming from</param>
        /// <returns>new message wrapper</returns>
        public static MessageWrapper Create(object payload, string messageSource)
        {
            return new MessageWrapper()
            {
                MessageSource = messageSource,
                MessageType = payload.GetType().FullName,
                Payload = JsonConvert.SerializeObject(payload)
            };
        }

        /// <summary>
        /// attempts to deserialize the wrapper 
        /// </summary>
        /// <typeparam name="T">type to deserialize as</typeparam>
        /// <param name="wrapper">the wrapper to deserialize</param>
        /// <returns>deserialized object</returns>
        public static T Deserialize<T>(MessageWrapper wrapper)
        {
            return JsonConvert.DeserializeObject<T>(wrapper.Payload);
        }

        [DataMember]
        public string MessageSource { get; set; }

        [DataMember]
        public string MessageType { get; set; }

        [DataMember]
        public string Payload { get; set; }
    }
}