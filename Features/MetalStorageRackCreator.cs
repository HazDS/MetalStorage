#if MONO
using ScheduleOne;
using ScheduleOne.ItemFramework;
using ScheduleOne.EntityFramework;
using ScheduleOne.UI.Shop;
#elif IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.UI.Shop;
#endif
using MelonLoader;
using MetalStorage.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MetalStorage.Features
{
    public static class MetalStorageRackCreator
    {
        private static readonly Color MetalColor = new Color(0.5f, 0.5f, 0.55f, 1f);

        public static readonly HashSet<string> MetalItemIds = new HashSet<string>();
        private static readonly Dictionary<string, Sprite> _loadedIcons = new Dictionary<string, Sprite>();
        private static readonly List<StorableItemDefinition> _createdDefinitions = new List<StorableItemDefinition>();

        public static void CreateAllMetalRacks()
        {
            LoadIcons();
            CreateMetalStorageRack(Constants.StorageRacks.SMALL, "Small Metal Storage Rack", Constants.ItemIds.SMALL, "MetalStorageRack_Small-Icon");
            CreateMetalStorageRack(Constants.StorageRacks.MEDIUM, "Medium Metal Storage Rack", Constants.ItemIds.MEDIUM, "MetalStorageRack_1.5x0.5-Icon");
            CreateMetalStorageRack(Constants.StorageRacks.LARGE, "Large Metal Storage Rack", Constants.ItemIds.LARGE, "MetalStorageRack_Large-Icon");
        }

        private static void LoadIcons()
        {
            string[] iconNames = {
                "MetalStorageRack_Small-Icon",
                "MetalStorageRack_1.5x0.5-Icon",
                "MetalStorageRack_Large-Icon"
            };

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var iconName in iconNames)
            {
                string resourceName = $"MetalStorage.Assets.{iconName}.png";
                try
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) continue;

                        byte[] imageData = new byte[stream.Length];
                        stream.Read(imageData, 0, imageData.Length);

                        var texture = new Texture2D(2, 2);
                        if (texture.LoadImage(imageData))
                        {
                            texture.filterMode = FilterMode.Bilinear;
                            _loadedIcons[iconName] = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f),
                                100f
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"Failed to load icon {iconName}: {ex.Message}");
                }
            }
        }

        private static void CreateMetalStorageRack(string originalId, string newName, string newId, string iconName)
        {
            var originalDef = Registry.GetItem<BuildableItemDefinition>(originalId);
            if (originalDef == null)
            {
                MelonLogger.Error($"Could not find original storage rack: {originalId}");
                return;
            }

            var metalDef = ScriptableObject.CreateInstance<BuildableItemDefinition>();

            metalDef.ID = newId;
            metalDef.Name = newName;
            metalDef.Description = $"A metal version of the {originalDef.Name.ToLower()}. More industrial looking.";
            metalDef.Category = originalDef.Category;
            metalDef.StackLimit = originalDef.StackLimit;
            metalDef.Keywords = originalDef.Keywords;
            metalDef.AvailableInDemo = originalDef.AvailableInDemo;
            metalDef.UsableInFilters = originalDef.UsableInFilters;
            metalDef.LabelDisplayColor = Color.white;
            metalDef.Icon = _loadedIcons.TryGetValue(iconName, out var icon) ? icon : originalDef.Icon;

            metalDef.BasePurchasePrice = Core.GetPrice(newId);
            metalDef.ResellMultiplier = originalDef.ResellMultiplier;
            metalDef.ShopCategories = originalDef.ShopCategories;
            metalDef.RequiresLevelToPurchase = originalDef.RequiresLevelToPurchase;
            metalDef.RequiredRank = originalDef.RequiredRank;
            metalDef.legalStatus = originalDef.legalStatus;
            metalDef.PickpocketDifficultyMultiplier = originalDef.PickpocketDifficultyMultiplier;
            metalDef.CombatUtilityForNPCs = originalDef.CombatUtilityForNPCs;

            metalDef.BuildSoundType = BuildableItemDefinition.EBuildSoundType.Metal;
            metalDef.BuiltItem = originalDef.BuiltItem;
            metalDef.StoredItem = originalDef.StoredItem;
            metalDef.Equippable = originalDef.Equippable;

            MetalItemIds.Add(newId);
            _createdDefinitions.Add(metalDef);
            Registry.Instance.AddToRegistry(metalDef);
        }

        public static void AddToShops()
        {
            foreach (var shop in ShopInterface.AllShops)
            {
                if (!ShopHasStorageRacks(shop)) continue;

                foreach (var metalDef in _createdDefinitions)
                {
                    if (ShopHasItem(shop, metalDef.ID)) continue;

                    var listing = new ShopListing { Item = metalDef, name = metalDef.Name };
                    shop.Listings.Add(listing);
                    listing.Initialize(shop);
                    CreateListingUI(shop, listing);
                }
            }
        }

        private static bool ShopHasStorageRacks(ShopInterface shop)
        {
            foreach (var listing in shop.Listings)
            {
                if (listing?.Item == null) continue;
                if (listing.Item.ID == Constants.StorageRacks.SMALL ||
                    listing.Item.ID == Constants.StorageRacks.MEDIUM ||
                    listing.Item.ID == Constants.StorageRacks.LARGE)
                    return true;
            }
            return false;
        }

        private static bool ShopHasItem(ShopInterface shop, string itemId)
        {
            foreach (var listing in shop.Listings)
            {
                if (listing.Item?.ID == itemId) return true;
            }
            return false;
        }

        private static void CreateListingUI(ShopInterface shop, ShopListing listing)
        {
            if (shop.ListingUIPrefab == null || shop.ListingContainer == null) return;

            var uiObject = UnityEngine.Object.Instantiate(shop.ListingUIPrefab.gameObject, shop.ListingContainer);
            var listingUI = uiObject.GetComponent<ListingUI>();
            if (listingUI == null) return;

            listingUI.Initialize(listing);

#if IL2CPP
            listingUI.onClicked = (Il2CppSystem.Action)(() => shop.ListingClicked(listingUI));
            listingUI.onDropdownClicked = (Il2CppSystem.Action)(() => shop.DropdownClicked(listingUI));
            listingUI.hoverStart = (Il2CppSystem.Action)(() => shop.EntryHovered(listingUI));
            listingUI.hoverEnd = (Il2CppSystem.Action)(() => shop.EntryUnhovered());
#else
            var listingClickedMethod = typeof(ShopInterface).GetMethod("ListingClicked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var dropdownClickedMethod = typeof(ShopInterface).GetMethod("DropdownClicked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var entryHoveredMethod = typeof(ShopInterface).GetMethod("EntryHovered", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var entryUnhoveredMethod = typeof(ShopInterface).GetMethod("EntryUnhovered", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (listingClickedMethod != null)
                listingUI.onClicked = (Action)Delegate.Combine(listingUI.onClicked, (Action)(() => listingClickedMethod.Invoke(shop, new object[] { listingUI })));
            if (dropdownClickedMethod != null)
                listingUI.onDropdownClicked = (Action)Delegate.Combine(listingUI.onDropdownClicked, (Action)(() => dropdownClickedMethod.Invoke(shop, new object[] { listingUI })));
            if (entryHoveredMethod != null)
                listingUI.hoverStart = (Action)Delegate.Combine(listingUI.hoverStart, (Action)(() => entryHoveredMethod.Invoke(shop, new object[] { listingUI })));
            if (entryUnhoveredMethod != null)
                listingUI.hoverEnd = (Action)Delegate.Combine(listingUI.hoverEnd, (Action)(() => entryUnhoveredMethod.Invoke(shop, null)));
#endif

            var listingUIField = typeof(ShopInterface).GetField("listingUI", BindingFlags.NonPublic | BindingFlags.Instance);
            var listingUIList = listingUIField?.GetValue(shop) as List<ListingUI>;
            listingUIList?.Add(listingUI);
        }

        public static void MetallizeMaterials(GameObject obj)
        {
            if (obj == null) return;

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;

                var materials = renderer.materials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    if (mat == null || !mat.name.ToLower().Contains("brownwood")) continue;

                    var newMat = new Material(mat) { name = mat.name + "_metal" };

                    var shader = mat.shader;
                    for (int p = 0; p < shader.GetPropertyCount(); p++)
                    {
                        if (shader.GetPropertyType(p) == ShaderPropertyType.Texture)
                            newMat.SetTexture(shader.GetPropertyName(p), null);
                    }
                    newMat.mainTexture = null;

                    if (newMat.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", MetalColor);
                    if (newMat.HasProperty("_Color")) newMat.SetColor("_Color", MetalColor);
                    if (newMat.HasProperty("_Metallic")) newMat.SetFloat("_Metallic", 0.8f);
                    if (newMat.HasProperty("_Smoothness")) newMat.SetFloat("_Smoothness", 0.5f);
                    if (newMat.HasProperty("_Glossiness")) newMat.SetFloat("_Glossiness", 0.5f);

                    materials[i] = newMat;
                    changed = true;
                }

                if (changed) renderer.materials = materials;
            }
        }
    }
}
