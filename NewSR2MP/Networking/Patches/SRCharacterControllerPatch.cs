using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;

namespace NewSR2MP.Networking.Packet;

[HarmonyPatch(typeof(SRCharacterController),nameof(SRCharacterController.Awake))]
public class SRCharacterControllerAwake
{
    static void Postfix(SRCharacterController __instance)
    {
        if (latestSaveJoined == null)
            return;
        
        __instance.Position = (Vector3)latestSaveJoined?.localPlayerSave.pos;
        __instance.Rotation = Quaternion.Euler((Vector3)latestSaveJoined?.localPlayerSave.rot);
    }
}