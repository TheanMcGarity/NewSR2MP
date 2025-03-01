namespace NewSR2MP.Networking.Patches;

[HarmonyPatch(typeof(PlayerDeathHandler),nameof(PlayerDeathHandler.OnDeath))]
public class PlayerDeathHandlerOnDeath
{
    public static float timer = -1f;
    public const float TIMER_LENGTH = .8f;
    public static bool Prefix()
    {
        if (isJoiningAsClient)
            if (timer <= Time.time)
                return false;
        
        return true;
    }
}