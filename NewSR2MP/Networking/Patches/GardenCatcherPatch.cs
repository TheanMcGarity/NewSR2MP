using HarmonyLib;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(GardenCatcher),nameof(GardenCatcher.Plant))]
    public class GardenCatcherPlant
    {

        public static void Postfix(GardenCatcher __instance, Identifiable.Id cropId, bool isReplacement)
        {
            // Check if it is being planted by a network handler.
            if (!__instance.IsHandling())
            {
                SRMP.Log("Garden Debug");
                // Get landplot ID.
                string id = __instance.GetComponentInParent<LandPlotLocation>().id;

                var msg = new GardenPlantMessage()
                {
                    ident = cropId,
                    replace = isReplacement,
                    id = id,
                };
                SRNetworkManager.NetworkSend(msg);
            }
        }
    }
}
