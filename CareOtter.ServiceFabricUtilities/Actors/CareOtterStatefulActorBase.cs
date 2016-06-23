using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CareOtter.ServiceFabricUtilities.Actors
{
    public class CareOtterStatefulActorBase : Actor
    {
        
        public bool TryGetReminder(string reminderName, out IActorReminder reminder)
        {
            try
            {
                reminder = GetReminder(reminderName);
                return true;
            }
            catch (FabricException)
            {
                reminder = null;
                return false;
            }
        }
    }
}