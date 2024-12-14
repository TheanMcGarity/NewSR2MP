using HarmonyLib;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(Ammo), nameof(Ammo.MaybeAddToSpecificSlot), typeof(IdentifiableType), typeof(Identifiable), typeof(int), typeof(int), typeof(bool))]
    public class AmmoMaybeAddToSpecificSlot
    {
        public static void Postfix(Ammo __instance, ref bool __result,IdentifiableType id, Identifiable identifiable, int slotIdx, int count, bool overflow)
        {
            if (!(ClientActive() || ServerActive()))
                return;
            
            if (__result)
            {
                if (__instance is NetworkAmmo netAmmo)
                {
                    var packet = new AmmoEditSlotMessage()
                    {
                        ident = id.name,
                        slot = slotIdx,
                        count = count,
                        id = netAmmo.ammoId
                    };
                    MultiplayerManager.NetworkSend(packet);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.MaybeAddToSlot), typeof(IdentifiableType), typeof(Identifiable), typeof(SlimeAppearance.AppearanceSaveSet))]
    public class AmmoMaybeAddToSlot
    {

        public static bool Prefix(Ammo __instance, ref bool __result, IdentifiableType id, Identifiable identifiable, SlimeAppearance.AppearanceSaveSet appearance)
        {
            if (!(ClientActive() || ServerActive()))
                return true;

            if (!(__instance is NetworkAmmo)) return true;

            var slotIDX = (__instance as NetworkAmmo).GetSlotIDX(id);
            if (slotIDX == -1) return true;

            __instance.MaybeAddToSpecificSlot(id, identifiable, slotIDX);

            return false;
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.Decrement), typeof(int), typeof(int))]
    public class AmmoDecrement
    {
        public static void Postfix(Ammo __instance, int index, int count)
        {
            if (!(ClientActive() || ServerActive()))
                return;
            if (__instance is NetworkAmmo netAmmo)
            {
                if (__instance.Slots[index]._count <= 0) __instance.Slots[index]._id = null;

                var packet = new AmmoRemoveMessage()
                {
                    index = index,
                    count = count,
                    id = netAmmo.ammoId
                };
                MultiplayerManager.NetworkSend(packet);
            }
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.DecrementSelectedAmmo), typeof(int))]
    public class AmmoDecrementSelectedAmmo
    {
        public static void Postfix(Ammo __instance, int amount)
        {
            if (!(ClientActive() || ServerActive()))
                return;

            if (__instance is NetworkAmmo netAmmo)
            {
                
                if (__instance.Slots[netAmmo._selectedAmmoIdx] != null && __instance.Slots[netAmmo._selectedAmmoIdx]._count <= 0) __instance.Slots[netAmmo._selectedAmmoIdx] = null;

                var packet = new AmmoRemoveMessage()
                {
                    index = netAmmo._selectedAmmoIdx,
                    count = amount,
                    id = netAmmo.ammoId
                };
                MultiplayerManager.NetworkSend(packet);
            }
        }
    }
}
