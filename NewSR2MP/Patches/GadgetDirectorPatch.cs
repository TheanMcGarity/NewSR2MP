using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.World;

namespace NewSR2MP.Patches;
[HarmonyPatch(typeof(GadgetDirector), nameof(GadgetDirector.InstantiateGadget))]
public class GadgetDirectorInstantiateGadget
{
    static void Postfix(GadgetDirector __instance, GameObject __result, GameObject original, SceneGroup sceneGroup, Vector3 position, Quaternion rotation, bool spawnImmediate)
    {
        if (!handlingPacket)
        {
            if (ClientActive())
            {
                var isGadget = __result.TryGetComponent<Gadget>(out var gadget);

                var packet = new ActorSpawnClientMessage()
                {
                    ident = GetIdentID(gadget.identType),
                    position = position,
                    rotation = __result.transform.eulerAngles,
                    velocity = Vector3.zero,
                    player = currentPlayerID,
                    scene = sceneGroupsReverse[systemContext.SceneLoader.CurrentSceneGroup.name]
                };
                MultiplayerManager.NetworkSend(packet);
                SRMP.Debug($"Client Spawn - {gadget.identType.name}");
                handlingPacket = true; 
                if (isGadget)
                    DestroyGadget(__result, "SR2MP.SpawnGadgetClient");
                handlingPacket = false;
            }
            else if (ServerActive())
            {
                var id = __result.GetComponent<Gadget>().GetActorId().Value;

                gadgets.TryAdd(id, __result.GetComponent<Gadget>());
                
                MultiplayerManager.NetworkSend(new ActorSpawnMessage
                {
                    rotation = rotation.ToEuler(),
                    position = position,
                    id = id,
                    ident = GetIdentID(__result.GetComponent<Gadget>().identType),
                    scene = sceneGroupsReverse[sceneGroup.name]
                });
            }
        }
    }
}