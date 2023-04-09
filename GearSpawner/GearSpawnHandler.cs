using System.Buffers.Binary;
using System.Security.Cryptography;

namespace GearSpawner;

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

	/// <summary>
	/// Rolls the chance that an action is successful.
	/// </summary>
	/// <param name="percent">Chance of success between 0 and 100</param>
	/// <returns>True if successful and false otherwise.</returns>
	public static bool RollChance(float percent)
	{
		return percent switch
		{
			<= 0 => false,
			>= 100 => true,
			_ => RandomFloat() < percent
		};

		static float RandomFloat()
		{
			return (float) new System.Random().Next(0,100);
		}
	}
}