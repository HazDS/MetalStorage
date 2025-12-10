#if MONO
using ScheduleOne.Persistence;
#elif IL2CPP
using Il2CppScheduleOne.Persistence;
#endif
using MelonLoader;
using MetalStorage.Features;
using MetalStorage.Utils;
using System.Collections;
using UnityEngine.Events;

[assembly: MelonInfo(typeof(MetalStorage.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]

namespace MetalStorage
{
    public class Core : MelonMod
    {
        private bool _itemsInitialized;
        private bool _shopsInitialized;
        private bool _preLoadHooked;

        private static MelonPreferences_Category _prefsCategory;
        private static MelonPreferences_Entry<int> _smallExtraSlots;
        private static MelonPreferences_Entry<int> _mediumExtraSlots;
        private static MelonPreferences_Entry<int> _largeExtraSlots;
        private static MelonPreferences_Entry<int> _smallPrice;
        private static MelonPreferences_Entry<int> _mediumPrice;
        private static MelonPreferences_Entry<int> _largePrice;

        public static int GetExtraSlots(string itemId)
        {
            return itemId switch
            {
                Constants.ItemIds.SMALL => _smallExtraSlots?.Value ?? 2,
                Constants.ItemIds.MEDIUM => _mediumExtraSlots?.Value ?? 2,
                Constants.ItemIds.LARGE => _largeExtraSlots?.Value ?? 2,
                _ => 0
            };
        }

        public static int GetPrice(string itemId)
        {
            return itemId switch
            {
                Constants.ItemIds.SMALL => _smallPrice?.Value ?? 72,
                Constants.ItemIds.MEDIUM => _mediumPrice?.Value ?? 104,
                Constants.ItemIds.LARGE => _largePrice?.Value ?? 144,
                _ => 0
            };
        }

        public override void OnInitializeMelon()
        {
            _prefsCategory = MelonPreferences.CreateCategory(Constants.PREFERENCES_CATEGORY, "Metal Storage Settings");

            _smallExtraSlots = _prefsCategory.CreateEntry("SmallExtraSlots", 2, "Small Rack Extra Slots",
                "Extra slots for Small Metal Storage Rack (base: 4, max extra: 16)");
            _mediumExtraSlots = _prefsCategory.CreateEntry("MediumExtraSlots", 2, "Medium Rack Extra Slots",
                "Extra slots for Medium Metal Storage Rack (base: 6, max extra: 14)");
            _largeExtraSlots = _prefsCategory.CreateEntry("LargeExtraSlots", 2, "Large Rack Extra Slots",
                "Extra slots for Large Metal Storage Rack (base: 8, max extra: 12)");

            _smallPrice = _prefsCategory.CreateEntry("SmallRackPrice", 72, "Small Rack Price",
                "Purchase price for Small Metal Storage Rack");
            _mediumPrice = _prefsCategory.CreateEntry("MediumRackPrice", 104, "Medium Rack Price",
                "Purchase price for Medium Metal Storage Rack");
            _largePrice = _prefsCategory.CreateEntry("LargeRackPrice", 144, "Large Rack Price",
                "Purchase price for Large Metal Storage Rack");

            ClampEntry(_smallExtraSlots, 0, 16);
            ClampEntry(_mediumExtraSlots, 0, 14);
            ClampEntry(_largeExtraSlots, 0, 12);
            ClampEntry(_smallPrice, 1, 10000);
            ClampEntry(_mediumPrice, 1, 10000);
            ClampEntry(_largePrice, 1, 10000);
        }

        private static void ClampEntry(MelonPreferences_Entry<int> entry, int min, int max)
        {
            if (entry.Value < min) entry.Value = min;
            if (entry.Value > max) entry.Value = max;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            TryHookPreLoad();

            if (sceneName == "Main")
            {
                if (!_itemsInitialized)
                {
                    MetalStorageRackCreator.CreateAllMetalRacks();
                    _itemsInitialized = true;
                }

                if (!_shopsInitialized)
                {
                    MelonCoroutines.Start(AddToShopsDelayed());
                }
            }
            else if (sceneName == "Menu")
            {
                _itemsInitialized = false;
                _shopsInitialized = false;
            }
        }

        private void TryHookPreLoad()
        {
            if (_preLoadHooked) return;

            var loadManager = LoadManager.Instance;
            if (loadManager == null) return;

#if IL2CPP
            loadManager.onPreLoad.AddListener((UnityAction)OnPreLoad);
#else
            loadManager.onPreLoad.AddListener(new UnityAction(OnPreLoad));
#endif
            _preLoadHooked = true;
        }

        private void OnPreLoad()
        {
            if (_itemsInitialized) return;

            MetalStorageRackCreator.CreateAllMetalRacks();
            _itemsInitialized = true;
        }

        private IEnumerator AddToShopsDelayed()
        {
            yield return new UnityEngine.WaitForSeconds(2f);

            if (_shopsInitialized) yield break;

            MetalStorageRackCreator.AddToShops();
            _shopsInitialized = true;
        }
    }
}
