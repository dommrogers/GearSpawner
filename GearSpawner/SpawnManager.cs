using Il2Cpp;
using System.Text.RegularExpressions;

namespace GearSpawner;

public static class SpawnManager
{
	public static event Action<string>? OnStartSpawning;
	public static event Action<IReadOnlyList<GearItem>>? OnFinishSpawning;

	public static void ParseSpawnInformation(string text)
	{
		string[] lines = Regex.Split(text, "\r\n|\r|\n");
		GearSpawnReader.ProcessLines(lines);
	}

	public static void AddLootTableEntry(string lootTable, LootTableEntry entry) => LootTableManager.AddLootTableEntry(lootTable, entry);

	public static void AddGearSpawnInfo(string sceneName, GearSpawnInfo gearSpawnInfo) => GearSpawnManager.AddGearSpawnInfo(sceneName, gearSpawnInfo);

	internal static void InvokeFinishSpawningEvent(IReadOnlyList<GearItem> items) => OnFinishSpawning?.Invoke(items);

	internal static void InvokeStartSpawningEvent(string sceneName) => OnStartSpawning?.Invoke(sceneName);
}
