using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GearSpawner;

internal static class Patches
{
	/// <summary>
	/// This method is called when a loot table item is validated, requires this patch due to GearSpawner using asset refs (e.g. GEAR_AssetName) that aren't guids like HL uses
	/// Makes sense for it to be part of GearSpawner as this is the mod that modifies the loot tables.
	/// </summary>
	[HarmonyPatch(typeof(AssetReference), nameof(AssetReference.RuntimeKeyIsValid))]
	internal static class RuntimeKeyIsValid
	{
		private static void Postfix(AssetReference __instance, ref bool __result)
		{

			if (__result || __instance.AssetGUID == null)
			{
				return;
			}

			if (__instance.AssetGUID != null && __instance.AssetGUID.StartsWith("GEAR_"))
			{
				GameObject? testObject = Addressables.LoadAssetAsync<GameObject>(__instance.AssetGUID).WaitForCompletion();
				if (testObject != null && testObject.name == __instance.AssetGUID)
				{
					//MelonLoader.MelonLogger.Warning("patched " + __instance.AssetGUID);
					__result = true;
				}
			}
		}
	}	
}
