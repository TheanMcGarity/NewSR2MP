using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(Destroyer), nameof(Destroyer.DestroyActor), typeof(GameObject), typeof(string), typeof(bool))]
    public class DestroyerDestroyActor
    {
        public static bool Prefix(GameObject actorObj, string source, bool okIfNonActor)
        {
            if (isJoiningAsClient) return true;
            try
            {
                if (ServerActive() || ClientActive())
                {
                    if (source.Equals("ResourceCycle.RegistryUpdate#1"))
                    {
                        return false;
                    }
                    else if (source.Equals("SlimeFeral.Awake"))
                    {
                        return false;
                    }
                }
            }
            catch { }

            // Moved here because it would spam testers' melonloader logs and lag the game because it didnt destroy (^^^^) but it sent the packet anyways.

            if ((ServerActive() || ClientActive()) && !handlingPacket && actorObj)
            {
                var packet = new ActorDestroyGlobalMessage()
                {
                    id = actorObj.GetComponent<IdentifiableActor>().GetActorId().Value,
                };
                MultiplayerManager.NetworkSend(packet);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Destroyer), nameof(DestroyGadget), typeof(GameObject), typeof(string))]
    public class DestroyerDestroy
    {
        public static void Prefix(GameObject gadgetObj, string source)
        {
            if (isJoiningAsClient) return;
            
            if ((ServerActive() || ClientActive()) && !handlingPacket && gadgetObj) 
            {
                SRMP.Debug("Destroyed Gadget!");
                var packet = new ActorDestroyGlobalMessage()
                {
                    id = gadgetObj.GetComponent<Gadget>().GetActorId().Value,
                };
                MultiplayerManager.NetworkSend(packet);
            }
        }
    }
}
