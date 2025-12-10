#if MONO
using ScheduleOne.Building;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Storage;
using ScheduleOne.Persistence.Datas;
#elif IL2CPP
using Il2CppScheduleOne.Building;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppInterop.Runtime;
#endif
using HarmonyLib;
using MetalStorage.Features;
using System;
using System.Collections.Generic;

namespace MetalStorage.Integrations
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        private static readonly HashSet<int> _expandedStorages = new HashSet<int>();

        [HarmonyPatch(typeof(BuildManager), nameof(BuildManager.CreateGridItem))]
        [HarmonyPostfix]
        public static void CreateGridItem_Postfix(GridItem __result, ItemInstance item)
        {
            if (__result == null || item?.Definition == null) return;

            string itemId = item.Definition.ID;
            if (!MetalStorageRackCreator.MetalItemIds.Contains(itemId)) return;

            MetalStorageRackCreator.MetallizeMaterials(__result.gameObject);

#if IL2CPP
            var placeableStorage = __result.TryCast<PlaceableStorageEntity>();
            if (placeableStorage?.StorageEntity != null)
                ExpandStorageSlotsEarly(placeableStorage, itemId);
#endif
        }

        [HarmonyPatch(typeof(BuildManager), nameof(BuildManager.CreateSurfaceItem))]
        [HarmonyPostfix]
        public static void CreateSurfaceItem_Postfix(SurfaceItem __result, ItemInstance item)
        {
            if (__result == null || item?.Definition == null) return;

            if (MetalStorageRackCreator.MetalItemIds.Contains(item.Definition.ID))
                MetalStorageRackCreator.MetallizeMaterials(__result.gameObject);
        }

        [HarmonyPatch(typeof(BuildableItem), nameof(BuildableItem.InitializeBuildableItem))]
        [HarmonyPostfix]
        public static void InitializeBuildableItem_Postfix(BuildableItem __instance, ItemInstance instance)
        {
            if (instance?.Definition == null) return;

            if (MetalStorageRackCreator.MetalItemIds.Contains(instance.Definition.ID))
                MetalStorageRackCreator.MetallizeMaterials(__instance.gameObject);
        }

        [HarmonyPatch(typeof(PlaceableStorageEntity), "Start")]
        [HarmonyPostfix]
        public static void PlaceableStorageEntity_Start_Postfix(PlaceableStorageEntity __instance)
        {
            if (__instance?.ItemInstance?.Definition == null) return;
            if (__instance.StorageEntity == null) return;

            string itemId = __instance.ItemInstance.Definition.ID;
            if (!MetalStorageRackCreator.MetalItemIds.Contains(itemId)) return;

            ExpandStorageSlots(__instance, itemId);
        }

#if MONO
        [HarmonyPatch(typeof(ItemSet), nameof(ItemSet.LoadTo), new Type[] { typeof(List<ItemSlot>) })]
        [HarmonyPrefix]
#endif
        public static void ItemSet_LoadTo_Prefix(ItemSet __instance, List<ItemSlot> slots)
        {
            if (__instance.Items == null || slots == null) return;
            if (__instance.Items.Length <= slots.Count) return;
            if (slots.Count == 0) return;

            var owner = slots[0].SlotOwner;
            if (owner == null) return;

#if IL2CPP
            var storageEntity = owner.TryCast<StorageEntity>();
#else
            var storageEntity = owner as StorageEntity;
#endif
            if (storageEntity == null) return;

            var placeableStorage = storageEntity.GetComponentInParent<PlaceableStorageEntity>();
            if (placeableStorage?.ItemInstance?.Definition == null) return;

            string itemId = placeableStorage.ItemInstance.Definition.ID;
            if (!MetalStorageRackCreator.MetalItemIds.Contains(itemId)) return;

            int slotsToAdd = Math.Min(__instance.Items.Length - slots.Count, 20 - slots.Count);
            if (slotsToAdd <= 0) return;

            for (int i = 0; i < slotsToAdd; i++)
            {
                var slot = new ItemSlot(storageEntity.SlotsAreFilterable);
#if IL2CPP
                slot.SetSlotOwner(storageEntity.Cast<IItemSlotOwner>());
#else
                slot.SetSlotOwner(storageEntity);
#endif
            }

            storageEntity.SlotCount = slots.Count;
            if (slots.Count + slotsToAdd > 6) storageEntity.DisplayRowCount = 2;
        }

        private static void ExpandStorageSlots(PlaceableStorageEntity placeableStorage, string itemId)
        {
            int instanceId = placeableStorage.GetInstanceID();
            if (_expandedStorages.Contains(instanceId)) return;
            _expandedStorages.Add(instanceId);

            int extraSlots = Core.GetExtraSlots(itemId);
            if (extraSlots <= 0) return;

            var storage = placeableStorage.StorageEntity;
            int currentSlotCount = storage.ItemSlots.Count;
            int baseSlots = storage.SlotCount;
            int targetSlots = Math.Min(baseSlots + extraSlots, 20);
            int slotsToAdd = targetSlots - currentSlotCount;

            if (slotsToAdd <= 0) return;

            for (int i = 0; i < slotsToAdd; i++)
            {
                var slot = new ItemSlot(storage.SlotsAreFilterable);
#if IL2CPP
                slot.SetSlotOwner(storage.Cast<IItemSlotOwner>());
#else
                slot.SetSlotOwner(storage);
#endif
            }

            storage.SlotCount = targetSlots;

            int newSlotCount = storage.ItemSlots.Count;
            for (int i = currentSlotCount; i < newSlotCount; i++)
            {
                placeableStorage.InputSlots.Add(storage.ItemSlots[i]);
                placeableStorage.OutputSlots.Add(storage.ItemSlots[i]);
            }

            if (targetSlots > 6) storage.DisplayRowCount = 2;
        }

#if IL2CPP
        private static void ExpandStorageSlotsEarly(PlaceableStorageEntity placeableStorage, string itemId)
        {
            int instanceId = placeableStorage.GetInstanceID();
            if (_expandedStorages.Contains(instanceId)) return;
            _expandedStorages.Add(instanceId);

            int extraSlots = Core.GetExtraSlots(itemId);
            if (extraSlots <= 0) return;

            var storage = placeableStorage.StorageEntity;
            int currentSlotCount = storage.ItemSlots.Count;
            int baseSlots = storage.SlotCount;
            int targetSlots = Math.Min(baseSlots + extraSlots, 20);
            int slotsToAdd = targetSlots - currentSlotCount;

            if (slotsToAdd <= 0) return;

            for (int i = 0; i < slotsToAdd; i++)
            {
                var slot = new ItemSlot(storage.SlotsAreFilterable);
                slot.SetSlotOwner(storage.Cast<IItemSlotOwner>());
            }

            storage.SlotCount = targetSlots;
            if (targetSlots > 6) storage.DisplayRowCount = 2;
        }
#endif
    }
}
