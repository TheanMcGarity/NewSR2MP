using System.Collections;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace NewSR2MP.Networking.Patches
{
    internal static class ActorSpawnHelper
    {
        internal static IEnumerator Spawn(GameObject __result, GameObject original, SceneGroup sceneGroup,
            Vector3 position, Quaternion rotation, bool nonActorOk = false,
            SlimeAppearance.AppearanceSaveSet appearance = SlimeAppearance.AppearanceSaveSet.NONE,
            SlimeAppearance.AppearanceSaveSet secondAppearance = SlimeAppearance.AppearanceSaveSet.NONE)
        {
            yield return null;
            
            if (!__result) yield break;
            
            if (isJoiningAsClient) yield break;

            if (!handlingPacket)
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
                            position = position,
                            rotation = rotation.eulerAngles,
                            velocity = vel,
                            player = currentPlayerID,
                            scene = sceneGroupsReverse[systemContext.SceneLoader.CurrentSceneGroup.name]
                        };

                        MultiplayerManager.NetworkSend(packet);

                        DestroyActor(__result, "SR2MP.ClientActorSpawn", true);
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
                
                        if (!actors.TryAdd(id, __result.GetComponent<NetworkActor>()))
                            actors[id] = __result.GetComponent<NetworkActor>();


                        ts.interpolPeriod = 0.245f;
                        ts.enabled = false;

                        Vector3 vel = Vector3.zero;
                        if (__result.TryGetComponent<Rigidbody>(out var rb))
                            vel = rb.velocity;
                
                        MultiplayerManager.NetworkSend(new ActorSpawnMessage
                        {
                            rotation = rotation.ToEuler(),
                            position = position,
                            velocity = vel,
                            id = id,
                            ident = GetIdentID(__result.GetComponent<IdentifiableActor>().identType),
                            scene = sceneGroupsReverse[sceneGroup.name]
                        });
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Error in loading actor: {e}");
                }
            }
        }
    }
    [HarmonyPatch(typeof(InstantiationHelpers), nameof(InstantiateActor))]
    public class InstantiationHelpersInstantiateActor
    {
        public static void Postfix(GameObject __result, GameObject original, SceneGroup sceneGroup, Vector3 position,
            Quaternion rotation, bool nonActorOk = false,
            SlimeAppearance.AppearanceSaveSet appearance = SlimeAppearance.AppearanceSaveSet.NONE,
            SlimeAppearance.AppearanceSaveSet secondAppearance = SlimeAppearance.AppearanceSaveSet.NONE)
        {
            MelonCoroutines.Start(ActorSpawnHelper.Spawn(__result, original, sceneGroup, position, rotation, nonActorOk, appearance, secondAppearance));
        }
    }
}