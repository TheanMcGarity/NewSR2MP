using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(InstantiationHelpers), nameof(InstantiateActor))]
    public class InstantiationHelpersInstantiateActor
    {

        public static void Postfix(GameObject __result, GameObject original, SceneGroup sceneGroup, Vector3 position,
            Quaternion rotation, bool nonActorOk = false,
            SlimeAppearance.AppearanceSaveSet appearance = SlimeAppearance.AppearanceSaveSet.NONE,
            SlimeAppearance.AppearanceSaveSet secondAppearance = SlimeAppearance.AppearanceSaveSet.NONE)
        {
            if (isJoiningAsClient) return;

            if (__result.GetComponent<NetworkActor>() == null)
            {
                try
                {
                    if (ClientActive())
                    {
                        Identifiable ident = null;

                        var isActor = __result.TryGetComponent<IdentifiableActor>(out var actor);
                        if (isActor) ident = actor;

                        var isGadget = __result.TryGetComponent<Gadget>(out var gadget);
                        if (isGadget) ident = gadget;

                        Vector3 vel = Vector3.zero;
                        if (__result.TryGetComponent<Rigidbody>(out var rb))
                            vel = rb.velocity;

                        var packet = new ActorSpawnClientMessage()
                        {
                            ident = GetIdentID(ident.identType),
                            position = __result.transform.position,
                            rotation = __result.transform.eulerAngles,
                            velocity = vel,
                            player = currentPlayerID,
                            scene = sceneGroupsReverse[systemContext.SceneLoader.CurrentSceneGroup.name]
                        };

                        MultiplayerManager.NetworkSend(packet);

                        DestroyActor(__result, "SR2MP.ClientActorSpawn", true);
                    }
                }
                catch
                {
                }
            }
            else if (ServerActive())
            {

                if (__result.GetComponent<NetworkActor>() == null)
                {
                    __result.AddComponent<NetworkActor>();
                    __result.AddComponent<TransformSmoother>();
                    __result.AddComponent<NetworkActorOwnerToggle>();
                }

                var ts = __result.GetComponent<TransformSmoother>();
                var id = __result.GetComponent<IdentifiableActor>().GetActorId().Value;
                if (actors.TryAdd(id, __result.GetComponent<NetworkActor>()))
                    actors[id] = __result.GetComponent<NetworkActor>();


                ts.interpolPeriod = 0.15f;
                ts.enabled = false;

                MultiplayerManager.NetworkSend(new ActorSpawnMessage
                {
                    rotation = rotation.ToEuler(),
                    position = position,
                    id = id,
                    ident = GetIdentID(__result.GetComponent<IdentifiableActor>().identType),
                    scene = sceneGroupsReverse[sceneGroup.name]
                });
            }

        }
    }
}