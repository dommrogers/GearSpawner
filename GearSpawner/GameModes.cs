using Il2Cpp;
using Il2CppTLD.Gameplay.Tunable;

namespace GearSpawner;

internal static class GameModes
{
	public static DifficultyLevel GetDifficultyLevel()
	{
		if (GameManager.IsStoryMode())
		{
			return DifficultyLevel.Storymode;
		}

		return ExperienceModeManager.GetCurrentExperienceModeType() switch
		{
			ExperienceModeType.Pilgrim => DifficultyLevel.Pilgram,
			ExperienceModeType.Voyageur => DifficultyLevel.Voyager,
			ExperienceModeType.Stalker => DifficultyLevel.Stalker,
			ExperienceModeType.Interloper => DifficultyLevel.Interloper,
			ExperienceModeType.Custom => GetCustomDifficultyLevel(),
			ExperienceModeType.ChallengeArchivist => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeDeadManWalking => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeHunted => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeHuntedPart2 => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeNomad => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeNowhereToHide => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeRescue => DifficultyLevel.Challenge,
			ExperienceModeType.ChallengeWhiteout => DifficultyLevel.Challenge,
			_ => DifficultyLevel.Other,
		};
	}

	private static DifficultyLevel GetCustomDifficultyLevel()
	{
		return GameManager.GetCustomMode().m_BaseResourceAvailability switch
		{
			CustomTunableLMHV.VeryHigh => DifficultyLevel.Pilgram,
			CustomTunableLMHV.High => DifficultyLevel.Voyager,
			CustomTunableLMHV.Medium => DifficultyLevel.Stalker,
			CustomTunableLMHV.Low => DifficultyLevel.Interloper,
			_ => DifficultyLevel.Other,
		};
	}

	public static FirearmAvailability GetFirearmAvailability()
	{
		if (GameManager.IsStoryMode())
		{
			return SaveGameSystem.m_CurrentEpisode is Episode.One or Episode.Two
				? FirearmAvailability.Rifle
				: FirearmAvailability.All;
		}

		return ExperienceModeManager.GetCurrentExperienceModeType() switch
		{
			ExperienceModeType.Interloper => FirearmAvailability.None,
			ExperienceModeType.Custom => GetCustomFirearmAvailability(),
			_ => FirearmAvailability.All,
		};
	}

	private static FirearmAvailability GetCustomFirearmAvailability()
	{
		FirearmAvailability result = FirearmAvailability.None;
		if (GameManager.GetCustomMode().m_RevolversInWorld)
		{
			result |= FirearmAvailability.Revolver;
		}
		if (GameManager.GetCustomMode().m_RiflesInWorld)
		{
			result |= FirearmAvailability.Rifle;
		}
		return result;
	}
}
