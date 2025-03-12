using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using UnityEngine;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(InstantiationHelpers),nameof(InstantiationHelpers.InstantiateActor))]
    public class InstantiationHelpersInstantiateActor
    {

        public static void Postfix(GameObject __result, GameObject original, SceneGroup sceneGroup, Vector3 position, Quaternion rotation, bool nonActorOk = false, SlimeAppearance.AppearanceSaveSet appearance = SlimeAppearance.AppearanceSaveSet.NONE, SlimeAppearance.AppearanceSaveSet secondAppearance = SlimeAppearance.AppearanceSaveSet.NONE)
        {
            if (isJoiningAsClient) return;

            if (ClientActive() && !ServerActive() && !original.GetComponent<IdentifiableActor>().identType.IsPlayer && __result.GetComponent<NetworkActor>() == null)
            {
                if (__result.GetComponent<NetworkActor>() == null)
                {
                    try
                    {

                        __result.transform.GetChild(0).gameObject.SetActive(false);
                        __result.GetComponent<Collider>().isTrigger = true;
                        __result.gameObject.AddComponent<NetworkActorSpawn>();
                        return;
                    }
                    catch { }
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
