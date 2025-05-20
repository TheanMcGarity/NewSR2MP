using HarmonyLib;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(Ammo), nameof(Ammo.MaybeAddToSpecificSlot), typeof(IdentifiableType), typeof(Identifiable), typeof(int), typeof(int), typeof(bool))]
    public class AmmoMaybeAddToSpecificSlot
    {
        public static void Postfix(Ammo __instance, ref bool __result,IdentifiableType id, Identifiable identifiable, int slotIdx, int count, bool overflow)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            if (__result)
            {
                
                var packet = new AmmoEditSlotMessage()
                {
                    ident = GetIdentID(id),
                    slot = slotIdx,
                    count = count,
                    id = __instance.GetPlotID()
                };
                if (packet.id == null) return;
                
                MultiplayerManager.NetworkSend(packet);
            }
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.MaybeAddToSlot), typeof(IdentifiableType), typeof(Identifiable),
        typeof(SlimeAppearance.AppearanceSaveSet))]
    public class AmmoMaybeAddToSlot
    {

        public static void Postfix(Ammo __instance, ref bool __result, IdentifiableType id, Identifiable identifiable,
            SlimeAppearance.AppearanceSaveSet appearance)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            var slotIDX = __instance.GetSlotIDX(id);
            
            if (slotIDX == -1) return;
            
            if (__result)
            {
                var packet = new AmmoEditSlotMessage
                {
                    ident = GetIdentID(id),
                    slot = slotIDX,
                    count = 1,
                    id = __instance.GetPlotID()
                };
                
                if (packet.id == null) return;

                MultiplayerManager.NetworkSend(packet);
            }
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.Decrement), typeof(int), typeof(int))]
    public class AmmoDecrement
    {
        public static void Postfix(Ammo __instance, int index, int count)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            if (__instance.Slots[index]._count <= 0) __instance.Slots[index]._id = null;

            var packet = new AmmoRemoveMessage()
            {
                index = index,
                count = count,
                id = __instance.GetPlotID()
            };        
            
            if (packet.id == null) return;
            
            MultiplayerManager.NetworkSend(packet);
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.DecrementSelectedAmmo), typeof(int))]
    public class AmmoDecrementSelectedAmmo
    {
        public static void Postfix(Ammo __instance, int amount)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            var packet = new AmmoRemoveMessage()
            {
                index = __instance._selectedAmmoIdx,
                count = amount,
                id = __instance.GetPlotID()
            };          
            
            if (packet.id == null) return;

            MultiplayerManager.NetworkSend(packet);
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.SetAmmoSlot), typeof(int))]
    public class AmmoSetAmmoSlot
    {
        public static void Postfix(Ammo __instance, int idx)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            var packet = new AmmoRemoveMessage()
            {
                index = idx,
                id = __instance.GetPlotID()
            };          
            
            if (packet.id == null) return;

            MultiplayerManager.NetworkSend(packet);
        }
    }

    [HarmonyPatch(typeof(Ammo), nameof(Ammo.NextAmmoSlot))]
    public class AmmoNextAmmoSlot
    {
        public static void Postfix(Ammo __instance)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            var packet = new AmmoSelectMessage()
            {
                index = __instance._selectedAmmoIdx + 1,
                id = __instance.GetPlotID()
            };          
            
            if (packet.id == null) return;

            MultiplayerManager.NetworkSend(packet);
        }
    }
    [HarmonyPatch(typeof(Ammo), nameof(Ammo.PrevAmmoSlot))]
    public class AmmoPrevAmmoSlot
    {
        public static void Postfix(Ammo __instance)
        {
            if (!(ClientActive() || ServerActive()) || handlingPacket)
                return;
            
            var packet = new AmmoSelectMessage()
            {
                index = __instance._selectedAmmoIdx - 1,
                id = __instance.GetPlotID()
            };   
            if (packet.id == null) return;

            MultiplayerManager.NetworkSend(packet);
        }
    }
}
