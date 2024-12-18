using Il2CppMonomiPark.SlimeRancher.Map;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(MapDirector), nameof(MapDirector.SetPlayerNavigationMarker))]
    internal class MapDirectorSetPlayerNavigationMarker
    {
        public static void Postfix(MapDirector __instance, Vector3 position, MapDefinition onMap, float minimumDistanceToPlace)
        {
            if (SceneContext.Instance.GetComponent<HandledDummy>()) return;
            
            MapType packetMapType;
            switch (onMap.name)
            {
                case "LabyrinthMap":
                    packetMapType = MapType.Labyrinth;
                    break;
                default:
                    packetMapType = MapType.RainbowIsland;
                    break;
            }

            var packet = new PlaceNavMarkerNessage()
            {
                map = packetMapType,
                position = position,
            };
            
            MultiplayerManager.NetworkSend(packet);
        }
    }  
    
    [HarmonyPatch(typeof(MapDirector), nameof(MapDirector.ClearPlayerNavigationMarker))]
    internal class MapDirectorClearPlayerNavigationMarker
    {
        public static void Postfix(MapDirector __instance)
        {
            if (SceneContext.Instance.GetComponent<HandledDummy>()) return;

            var packet = new RemoveNavMarkerNessage();
            
            MultiplayerManager.NetworkSend(packet);
        }
    }
}
