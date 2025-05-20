using SR2E;
using SR2E.Buttons;

namespace NewSR2MP.Patches;

//[HarmonyPatch(typeof(GameContext), nameof(GameContext.Start))]
public class GameContextStart
{
    public static void Postfix(GameContext __instance)
    {
        var label = AddTranslationFromSR2E("button.multiplayer", "b.multiplayer", "UI");
        
        new CustomPauseMenuButton(label, 1, () => { });
        new CustomMainMenuButton(label, SR2EUtils.ConvertToSprite(LoadImage("MultiplayerButton")), 1, () => { });
    }
}