using HarmonyLib;
using NewSR2MP.Component;
using NewSR2MP.Packet;
namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(GardenCatcher),nameof(GardenCatcher.Plant))]
    public class GardenCatcherPlant
    {

        public static void Postfix(GardenCatcher __instance, IdentifiableType cropId, bool isReplacement)
        {
            // Check if it is being planted by a network handler.
            if (!handlingPacket)
            {
                SRMP.Log("Garden Debug");
                // Get landplot ID.
                string id = __instance.GetComponentInParent<LandPlotLocation>()._id;

                var msg = new GardenPlantMessage()
                {
                    ident = GetIdentID(cropId),
                    replace = isReplacement,
                    id = id,
                };
                MultiplayerManager.NetworkSend(msg);
            }
        }
    }
}
