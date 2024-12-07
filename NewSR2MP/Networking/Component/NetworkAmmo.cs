using Il2CppMonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkAmmo : Ammo
    {
        public int GetSlotIDX(IdentifiableType id)
        {
            bool isSlotNull = false;
            // bool IsIdentAllowedForAmmo = false;
            bool isSlotEmptyOrSameType = false;
            bool isSlotFull = false;
            for (int j = 0; j < _ammoModel.slots.Count; j++)
            {
                isSlotNull = Slots[j] == null;

                isSlotEmptyOrSameType = isSlotNull || Slots[j]._id == id;

                // IsIdentAllowedForAmmo = slotPreds[j](id) && potentialAmmo.Contains(id);

                if (!isSlotNull)
                    isSlotFull = Slots[j].Count >= _ammoModel.GetSlotMaxCount(id, j);
                else
                    isSlotFull = false;

                if (isSlotEmptyOrSameType && isSlotFull) break;

                if (isSlotEmptyOrSameType)// && IsIdentAllowedForAmmo)
                {
                    return j;
                }
            }
            return -1;
        }
        public static Slot[] SRMPAmmoDataToSlots(Il2CppSystem.Collections.Generic.List<AmmoDataV01> ammo)
        {
            Slot[] array = new Slot[ammo.Count];
            for (int i = 0; i < ammo.Count; i++)
            {
                bool isSlotEmpty = ammo._items[i].ID == -1;
                if (isSlotEmpty)
                {
                    array[i] = null;
                    continue;
                }

                array[i]._count = ammo._items[i].Count;
                array[i]._id = GameContext.Instance.AutoSaveDirector.SavedGame.persistenceIdToIdentifiableType.GetIdentifiableType(ammo._items[i].ID);
                array[i].Emotions = new SlimeEmotionData();
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<SlimeEmotions.Emotion, float> emotionDatum in ammo._items[i].EmotionData.EmotionData)
                {
                    array[i].Emotions[emotionDatum.Key] = emotionDatum.Value;
                }
            }

            return array;
        }

        /// <summary>
        /// Site ID -> Ammo
        /// </summary>
        public static Dictionary<string, Ammo> all = new Dictionary<string, Ammo>();

        public string ammoId;
        public NetworkAmmo(string id, Il2CppReferenceArray<AmmoSlotDefinition> ammoSlotDefinitions) : base(ammoSlotDefinitions)
        {
            ammoId = id;
            all.Add(ammoId, this);
        }

        public NetworkAmmo() : base(DerivedConstructorPointer<NetworkAmmo>())
        {
        }
    }
}
