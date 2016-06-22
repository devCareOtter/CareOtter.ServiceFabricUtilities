using System;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace CareOtter.ServiceFabricUtilities.Actors
{
    public interface IActorFactory<T> where T : IActor
    {
        T GetActor(ActorId actorId);

        T GetActor(Guid actorId);

        T GetActor(string actorId);
    }

    public class ActorFactory<T> : IActorFactory<T> where T: IActor
    {
        private readonly Uri _uri;

        public ActorFactory(Uri uri)
        {
            _uri = uri;
        }

        public T GetActor(ActorId actorId)
        {
            return ActorProxy.Create<T>(actorId, _uri);
        }

        public T GetActor(Guid actorId)
        {
            return ActorProxy.Create<T>(new ActorId(actorId), _uri);
        }

        public T GetActor(string actorId)
        {
            return ActorProxy.Create<T>(new ActorId(actorId), _uri);
        }
    }
}
