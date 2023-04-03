using HarmonyLib;
using Il2Cpp;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

[HarmonyPatch]
internal static class GearSpawnManager
{
	private static readonly Dictionary<string, List<GearSpawnInfo>> gearSpawnInfos = new();

	internal static void AddGearSpawnInfo(string sceneName, GearSpawnInfo gearSpawnInfo)
	{
		string normalizedSceneName = GetNormalizedSceneName(sceneName);
		if (!gearSpawnInfos.TryGetValue(normalizedSceneName, out List<GearSpawnInfo>? sceneGearSpawnInfos))
		{
			sceneGearSpawnInfos = new();
			gearSpawnInfos.Add(normalizedSceneName, sceneGearSpawnInfos);
		}

		sceneGearSpawnInfos.Add(gearSpawnInfo.NormalizePrefabName());
	}

	private static string GetNormalizedGearName(string gearName)
	{
		if (!gearName.StartsWith("GEAR_"))
		{
			return "GEAR_" + gearName;
		}
		else
		{
			return gearName;
		}
	}

	private static string GetNormalizedSceneName(string sceneName) => sceneName.ToLowerInvariant();

	private static IEnumerable<GearSpawnInfo>? GetSpawnInfos(string sceneName)
	{
		if (gearSpawnInfos.TryGetValue(sceneName, out List<GearSpawnInfo>? result))
		{
			GearSpawnerMod.Logger.Msg($"Found {result.Count} spawn entries for '{sceneName}'");
			return result;
		}
		else
		{
			GearSpawnerMod.Logger.Msg($"Could not find any spawn entries for '{sceneName}'");
			return null;
		}
	}

	internal static void PrepareScene()
	{
		if (IsNonGameScene())
		{
			return;
		}

		string sceneName = GameManager.m_ActiveScene;

		SpawnManager.InvokeStartSpawningEvent(sceneName);

		GearSpawnerMod.Logger.Msg($"Spawning items for scene '{sceneName}' ...");
		Stopwatch stopwatch = Stopwatch.StartNew();

		IReadOnlyList<GearItem> spawnedItems = SpawnGearForScene(GetNormalizedSceneName(sceneName));

		stopwatch.Stop();
		GearSpawnerMod.Logger.Msg($"Spawned '{GameModes.GetDifficultyLevel()}' items for scene '{sceneName}' in {stopwatch.ElapsedMilliseconds} ms");

		SpawnManager.InvokeFinishSpawningEvent(spawnedItems);
	}

	internal static bool IsNonGameScene()
	{
		return GameManager.m_ActiveScene is null or "" or "MainMenu" or "Boot" or "Empty";
	}

	/// <summary>
	/// Spawns the items into the scene. However, this can be overwritten by deserialization
	/// </summary>
	/// <param name="sceneName"></param>
	private static IReadOnlyList<GearItem> SpawnGearForScene(string sceneName)
	{
		IEnumerable<GearSpawnInfo>? sceneGearSpawnInfos = GetSpawnInfos(sceneName);
		if (sceneGearSpawnInfos == null)
		{
			return Array.Empty<GearItem>();
		}

		DifficultyLevel difficultyLevel = GameModes.GetDifficultyLevel();
		FirearmAvailability firearmAvailability = GameModes.GetFirearmAvailability();

		List<GearItem> spawnedItems = new();

		foreach (GearSpawnInfo gearSpawnInfo in sceneGearSpawnInfos)
		{
			if (ShouldSpawn(difficultyLevel, firearmAvailability, gearSpawnInfo))
			{
				GameObject? gear = SpawnGear(sceneName, gearSpawnInfo);
				if (gear != null)
				{
					spawnedItems.Add(gear.GetComponent<GearItem>());
				}
			}
		}
		return spawnedItems;
	}

	private static bool ShouldSpawn(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		return Settings.Instance.alwaysSpawnItems
			|| SpawnTagManager.GetHandler(gearSpawnInfo.Tag).ShouldSpawn(difficultyLevel, firearmAvailability, gearSpawnInfo);
	}

	private static GameObject? SpawnGear(string sceneName, GearSpawnInfo gearSpawnInfo)
	{
		GameObject? prefab = Addressables.LoadAssetAsync<GameObject>(gearSpawnInfo.PrefabName).WaitForCompletion();

		if (prefab == null)
		{
			GearSpawnerMod.Logger.Warning($"Could not find prefab '{gearSpawnInfo.PrefabName}' to spawn in scene '{sceneName}'.");
			return null;
		}

		GameObject gear = UnityEngine.Object.Instantiate(prefab, gearSpawnInfo.Position, gearSpawnInfo.Rotation).Cast<GameObject>();
		gear.name = prefab.name;
		EnableObjectForXPMode(gear);
		return gear;
	}

	private static void EnableObjectForXPMode(GameObject gameObject)
	{
		DisableObjectForXPMode xpmode = gameObject.GetComponent<DisableObjectForXPMode>();
		if (xpmode != null)
		{
			UnityEngine.Object.Destroy(xpmode);
		}
	}

	/// <summary>
	/// Other than GameManager.SetAudioModeForLoadedScene(), QualitySettingsManager.ApplyCurrentQualitySettings is the last method called within GameManager.Update() before save file saving and loading occur. They only get called after the loading panel has closed, and they each only get called once. If GameManager.SetAudioModeForLoadedScene() was not inlined, it would be used instead.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.ApplyCurrentQualitySettings))]
	internal static void GameManager_ApplyCurrentQualitySettings()
	{
		//patch the scenes for loose items as they load
		PrepareScene();
	}
}
