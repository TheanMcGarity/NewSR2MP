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
                }
            }
            catch { }
            return true;
        }
    }


    // this is just IdentifiableDestroy patch but with Destroyer.DestroyActor
    [HarmonyPatch(typeof(Destroyer),nameof(Destroyer.DestroyActor))]
    public class DestroyerDestroyNetworkActor
    {
        public static void Prefix(GameObject actorObj, string source, bool okIfNonActor)
        {
            if (isJoiningAsClient) return;

            if ((ServerActive() || ClientActive()) && !actorObj.GetComponent<HandledDummy>())
            {
                var packet = new ActorDestroyGlobalMessage()
                {
                    id = actorObj.GetComponent<IdentifiableActor>().GetActorId().Value,
                };
                MultiplayerManager.NetworkSend(packet);
            }
        }
    }
}
