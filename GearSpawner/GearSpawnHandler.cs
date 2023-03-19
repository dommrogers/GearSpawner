﻿namespace GearSpawner;

public abstract class GearSpawnHandler
{
	/// <summary>
	/// Should this item be spawned?
	/// </summary>
	/// <param name="difficultyLevel">The difficulty of the current playthrough.</param>
	/// <param name="firearmAvailability">The availability of firearms in the current playthrough.</param>
	/// <param name="gearSpawnInfo"></param>
	/// <returns>True if the item should be spawned. False otherwise.</returns>
	public abstract bool ShouldSpawn(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo);
}