﻿using MelonLoader;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GearSpawner
{
	public static class GearSpawnManager
	{
		private static Dictionary<string, List<GearSpawnInfo>> gearSpawnInfos = new Dictionary<string, List<GearSpawnInfo>>();

		public static event System.Action<GearItem[]> OnSpawnGearItems;

		public static void AddGearSpawnInfo(string sceneName, GearSpawnInfo gearSpawnInfo)
		{
			string normalizedSceneName = GetNormalizedSceneName(sceneName);
			if (!gearSpawnInfos.ContainsKey(normalizedSceneName))
			{
				gearSpawnInfos.Add(normalizedSceneName, new List<GearSpawnInfo>());
			}

			List<GearSpawnInfo> sceneGearSpawnInfos = gearSpawnInfos[normalizedSceneName];
			sceneGearSpawnInfos.Add(gearSpawnInfo);
		}

		public static void ParseSpawnInformation(string text)
		{
			string[] lines = Regex.Split(text, "\r\n|\r|\n");
			GearSpawnReader.ProcessLines(lines);
		}

		private static string GetNormalizedGearName(string gearName)
		{
			if (gearName != null && !gearName.ToLower().StartsWith("gear_"))
			{
				return "gear_" + gearName;
			}
			else return gearName;
		}

		private static string GetNormalizedSceneName(string sceneName) => sceneName.ToLower();

		private static IEnumerable<GearSpawnInfo> GetSpawnInfos(string sceneName)
		{
			gearSpawnInfos.TryGetValue(sceneName, out List<GearSpawnInfo> result);
			if (result == null) 
				MelonLogger.Msg("Could not find any spawn entries for '{0}'", sceneName);
			else 
				MelonLogger.Msg("Found {0} spawn entries for '{1}'", result.Count, sceneName);
			return result;
		}

		internal static void PrepareScene()
		{
			if (IsNonGameScene()) return;

			string sceneName = GameManager.m_ActiveScene;
			MelonLogger.Msg($"Spawning items for scene '{sceneName}' ...");
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

			GearItem[] spawnedItems = SpawnGearForScene(GetNormalizedSceneName(sceneName));

			stopwatch.Stop();
			MelonLogger.Msg($"Spawned '{ProbabilityManager.GetDifficultyLevel()}' items for scene '{sceneName}' in {stopwatch.ElapsedMilliseconds} ms");

			OnSpawnGearItems?.Invoke(spawnedItems);
		}

		private static bool IsNonGameScene()
		{
			return string.IsNullOrEmpty(GameManager.m_ActiveScene) || GameManager.m_ActiveScene == "MainMenu" || GameManager.m_ActiveScene == "Boot" || GameManager.m_ActiveScene == "Empty";
		}

		/// <summary>
		/// Spawns the items into the scene. However, this can be overwritten by deserialization
		/// </summary>
		/// <param name="sceneName"></param>
		private static GearItem[] SpawnGearForScene(string sceneName)
		{
			IEnumerable<GearSpawnInfo> sceneGearSpawnInfos = GetSpawnInfos(sceneName);
			if (sceneGearSpawnInfos == null) return new GearItem[0];

			List<GearItem> spawnedItems = new List<GearItem>();

			foreach (GearSpawnInfo eachGearSpawnInfo in sceneGearSpawnInfos)
			{
				string normalizedGearName = GetNormalizedGearName(eachGearSpawnInfo.PrefabName);
				Object prefab = Resources.Load(normalizedGearName);

				if (prefab == null)
				{
					MelonLogger.Warning("Could not find prefab '{0}' to spawn in scene '{1}'.", eachGearSpawnInfo.PrefabName, sceneName);
					continue;
				}

				float spawnProbability = ProbabilityManager.GetAdjustedProbability(eachGearSpawnInfo);
				if (RandomUtils.RollChance(spawnProbability))
				{
					GameObject gear = Object.Instantiate(prefab, eachGearSpawnInfo.Position, eachGearSpawnInfo.Rotation).Cast<GameObject>();
					gear.name = prefab.name;
					DisableObjectForXPMode xpmode = gear.GetComponent<DisableObjectForXPMode>();
					if (xpmode != null) Object.Destroy(xpmode);
					spawnedItems.Add(gear.GetComponent<GearItem>());
				}
			}
			return spawnedItems.ToArray();
		}
	}
}
