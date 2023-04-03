using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

[HarmonyPatch]
internal static class LootTableManager
{
	private static Dictionary<string, List<LootTableEntry>> lootTableEntries = new Dictionary<string, List<LootTableEntry>>();
	private static List<int> processedLootTables = new();

	internal static void AddLootTableEntry(string lootTable, LootTableEntry entry)
	{
		string normalizedLootTableName = GetNormalizedLootTableName(lootTable);

		if (!lootTableEntries.ContainsKey(normalizedLootTableName))
		{
			lootTableEntries.Add(normalizedLootTableName, new List<LootTableEntry>());
		}

		lootTableEntries[normalizedLootTableName].Add(entry.Normalize());
	}

	internal static void ConfigureLootTableData(LootTableData lootTableData)
	{
		// empty loot table
		if (lootTableData == null)
		{
			return;
		}

		int instanceId = lootTableData.GetInstanceID();

		// already processed
		if (processedLootTables.Contains(instanceId))
		{
			return;
		}

		List<LootTableEntry> entries;
		if (lootTableEntries.TryGetValue(lootTableData.name.ToLowerInvariant(), out entries))
		{
			processedLootTables.Add(instanceId);

			Il2CppSystem.Collections.Generic.List<RandomTableDataEntry<AssetReferenceGearItem>> list = new();

//			MelonLoader.MelonLogger.Warning("found LootTableData " + lootTableData.name.ToLowerInvariant() + " " + lootTableData.m_BaseEntries.Count + " | " + entries.Count + " | " + lootTableData.GetInstanceID().ToString());

			List<string> has = new();
			foreach (RandomTableDataEntry<AssetReferenceGearItem> R in lootTableData.m_BaseEntries)
			{
				has.Add(R.m_Item.AssetGUID);
//				MelonLoader.MelonLogger.Warning(R.m_Item.AssetGUID + " => " + R.m_Weight + " | " + R.m_Item?.LoadAssetAsync()?.WaitForCompletion()?.name);
			}

			int added = 0;
			foreach (LootTableEntry entry in entries)
			{
				if (!has.Contains(entry.PrefabName))
				{
					RandomTableDataEntry<AssetReferenceGearItem> newEntry = new();
					newEntry.m_Item = new AssetReferenceGearItem(entry.PrefabName);
					newEntry.m_Weight = entry.Weight;

					lootTableData.m_BaseEntries.Add(newEntry);
					lootTableData.m_FilteredExtendedItems.Add(newEntry.m_Item);
					lootTableData.m_ExistingOperations.Add(new IKeyEvaluator(newEntry.m_Item.Pointer), newEntry.m_Item.LoadAsset());

//					MelonLoader.MelonLogger.Warning(entry.PrefabName + " => " + entry.Weight);

					added++;
				}
			}

//			MelonLoader.MelonLogger.Warning("patched LootTableData " + lootTableData.name.ToLowerInvariant() + " " + lootTableData.m_BaseEntries.Count + " | +" + added + " | " + lootTableData.GetInstanceID().ToString());
		}

	}

	private static string GetNormalizedLootTableName(string lootTable)
	{
		if (lootTable.StartsWith("Loot", System.StringComparison.InvariantCultureIgnoreCase))
		{
			return lootTable.ToLowerInvariant();
		}
		if (lootTable.StartsWith("Cargo", System.StringComparison.InvariantCultureIgnoreCase))
		{
			return "loot" + lootTable.ToLowerInvariant();
		}
		return "loottable" + lootTable.ToLowerInvariant();
	}


	[HarmonyPrefix]
	[HarmonyPatch(typeof(Container), nameof(Container.PopulateWithRandomGear))]
	private static void Container_PopulateWithRandomGear(Container __instance)
	{
		//MelonLoader.MelonLogger.Warning("Container_PopulateWithRandomGear | " + __instance.name);

		if (__instance.m_LootTable != null)
		{
			//			MelonLoader.MelonLogger.Warning("Container_PopulateWithRandomGear m_LootTable | " + __instance.name + " | " + __instance.m_LootTable.CanDrawFromTable.ToString());
			ConfigureLootTableData(__instance.m_LootTable);
		}
		if (__instance.m_LootTableData != null)
		{
			//			MelonLoader.MelonLogger.Warning("Container_PopulateWithRandomGear m_LootTableData | " + __instance.name + " | " + __instance.m_LootTableData.CanDrawFromTable.ToString());
			ConfigureLootTableData(__instance.m_LootTableData);
		}
		if (__instance.m_LockedLootTableData != null)
		{
			//			MelonLoader.MelonLogger.Warning("Container_PopulateWithRandomGear m_LockedLootTableData | " + __instance.name + " | " + __instance.m_LockedLootTableData.CanDrawFromTable.ToString());
			ConfigureLootTableData(__instance.m_LockedLootTableData);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData), new Type[] { typeof(string), typeof(string) })]
	private static void SaveGameSystem_LoadSceneData(SaveGameSystem __instance, string name, string sceneSaveName)
	{
		processedLootTables.Clear();
	}

}
