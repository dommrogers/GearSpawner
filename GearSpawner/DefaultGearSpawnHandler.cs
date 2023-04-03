using UnityEngine;

namespace GearSpawner;

public class DefaultGearSpawnHandler : IGearSpawnHandler
{
	public virtual float GetProbability(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		return GetAdjustedProbability(difficultyLevel, gearSpawnInfo.SpawnChance);
	}

	private static float GetAdjustedProbability(DifficultyLevel difficultyLevel, float baseProbability)
	{
		var multiplier = difficultyLevel switch
		{
			DifficultyLevel.Pilgram => Math.Max(0f, Settings.Instance.pilgramSpawnProbabilityMultiplier),
			DifficultyLevel.Voyager => Math.Max(0f, Settings.Instance.voyagerSpawnProbabilityMultiplier),
			DifficultyLevel.Stalker => Math.Max(0f, Settings.Instance.stalkerSpawnProbabilityMultiplier),
			DifficultyLevel.Interloper => Math.Max(0f, Settings.Instance.interloperSpawnProbabilityMultiplier),
//			DifficultyLevel.Storymode => Math.Max(0f, Settings.Instance.storySpawnProbabilityMultiplier),
			DifficultyLevel.Challenge => Math.Max(0f, Settings.Instance.challengeSpawnProbabilityMultiplier),
			_ => 1f,
		};
		if (multiplier == 0f)
		{
			return 0f; //can disable spawns for a game mode
		}

		float clampedProbability = Mathf.Clamp(baseProbability, 0f, 100f);//just to be safe

		if (clampedProbability == 100f)
		{
			return 100f; //for guaranteed spawns
		}
		else
		{
			return Mathf.Clamp(multiplier * clampedProbability, 0f, 100f); //for normal spawns
		}
	}
}