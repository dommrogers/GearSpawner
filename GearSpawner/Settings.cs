using ModSettings;

namespace GearSpawner;

internal sealed class Settings : JsonModSettings
{
	internal static Settings Instance { get; } = new();

	[Section("Loose Item Spawn Probability Multipliers")]
	[Name("Pilgram / Very High Loot Custom")]
	[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float pilgramSpawnProbabilityMultiplier = 1f;

	[Name("Voyager / High Loot Custom")]
	[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float voyagerSpawnProbabilityMultiplier = 0.6f;

	[Name("Stalker / Medium Loot Custom")]
	[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float stalkerSpawnProbabilityMultiplier = 0.4f;

	[Name("Interloper / Low Loot Custom")]
	[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float interloperSpawnProbabilityMultiplier = 0.2f;

	//[Name("Wintermute")]
	//[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	//[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	//public float storySpawnProbabilityMultiplier = 1f;

	[Name("Challenges")]
	[Description("Scales the default probability of finding MODDED spawns on this game mode. Setting to zero disables loose item spawns on this game mode. Doesn't affect container loot. Can be overridden by mod-specific settings.")]
	[Slider(0f, 1f, 101, NumberFormat = "{0:F2}")]
	public float challengeSpawnProbabilityMultiplier = 1f;

	[Section("Advanced")]
	[Name("Always Spawn Loose Items")]
	[Description("For testing and new spawn point creation. Converts all loose spawn points into guaranteed spawns. Does not necessarily work on existing saves.")]
	public bool alwaysSpawnItems = false;
}
