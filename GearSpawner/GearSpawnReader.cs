﻿using MelonLoader;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GearSpawner;

internal static class GearSpawnReader
{
	private const string NUMBER = @"-?\d+(?:\.\d+)?";
	private const string VECTOR = NUMBER + @"\s*,\s*" + NUMBER + @"\s*,\s*" + NUMBER;

	private static readonly Regex LOOTTABLE_ENTRY_REGEX = new Regex(
		@"^item\s*=\s*(\w+)" +
		@"\W+w\s*=\s*(" + NUMBER + ")$");

	private static readonly Regex LOOTTABLE_REGEX = new Regex(@"^loottable\s*=\s*(\w+)$");
	private static readonly Regex SCENE_REGEX = new Regex(@"^scene\s*=\s*(\w+)$");
	private static readonly Regex TAG_REGEX = new Regex(@"^tag\s*=\s*(\w+)$");

	private static readonly Regex SPAWN_REGEX = new Regex(
		@"^item\s*=\s*(\w+)" +
		@"(?:\W+p\s*=\s*(" + VECTOR + "))?" +
		@"(?:\W+r\s*=\s*(" + VECTOR + "))?" +
		@"(?:\W+\s*c\s*=\s*(" + NUMBER + "))?$");

	private static string GetTrimmedLine(string line)
	{
		if (line == null)
		{
			return "";
		}
		else
		{
			return line.Trim().ToLowerInvariant();
		}
	}

	private static float ParseFloat(string value, float defaultValue, string line)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}

		try
		{
			return float.Parse(value, CultureInfo.InvariantCulture);
		}
		catch (System.Exception)
		{
			throw new System.ArgumentException($"Could not parse '{value}' as numeric value in line {line}.");
		}
	}

	private static int ParseInt(string value, int defaultValue, string line)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}

		try
		{
			return int.Parse(value);
		}
		catch (System.Exception)
		{
			throw new System.ArgumentException($"Could not parse '{value}' as numeric value in line {line}.");
		}
	}

	private static Vector3 ParseVector(string value, string line)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Vector3.zero;
		}

		string[] components = value.Split(',');
		if (components.Length != 3)
		{
			throw new System.ArgumentException($"A vector requires 3 components, but found {components.Length} in line '{line}'.");
		}

		Vector3 result = new Vector3();
		result.x = ParseFloat(components[0].Trim(), 0, line);
		result.y = ParseFloat(components[1].Trim(), 0, line);
		result.z = ParseFloat(components[2].Trim(), 0, line);
		return result;
	}

	internal static void ProcessLines(string[] lines)
	{
		string? scene = null;
		string? loottable = null;
		string tag = "none";

		foreach (string eachLine in lines)
		{
			var trimmedLine = GetTrimmedLine(eachLine);
			if (trimmedLine.Length == 0 || trimmedLine.StartsWith("#"))
			{
				continue;
			}

			var match = SCENE_REGEX.Match(trimmedLine);
			if (match.Success)
			{
				scene = match.Groups[1].Value;
				loottable = null;
				continue;
			}

			match = TAG_REGEX.Match(trimmedLine);
			if (match.Success)
			{
				tag = match.Groups[1].Value;
				MelonLogger.Msg("Tag found while reading spawn file. '{0}'", tag);
				continue;
			}

			match = SPAWN_REGEX.Match(trimmedLine);
			if (match.Success)
			{
				if (string.IsNullOrEmpty(scene))
				{
					throw new InvalidFormatException($"No scene name defined before line '{eachLine}'. Did you forget a 'scene = <SceneName>'?");
				}

				GearSpawnInfo info = new GearSpawnInfo
				{
					PrefabName = match.Groups[1].Value,
					SpawnChance = ParseFloat(match.Groups[4].Value, 100, eachLine),
					Position = ParseVector(match.Groups[2].Value, eachLine),
					Rotation = Quaternion.Euler(ParseVector(match.Groups[3].Value, eachLine)),
					Tag = tag
				};

				GearSpawnManager.AddGearSpawnInfo(scene!, info);
				continue;
			}

			match = LOOTTABLE_REGEX.Match(trimmedLine);
			if (match.Success)
			{
				loottable = match.Groups[1].Value;
				scene = null;
				continue;
			}

			match = LOOTTABLE_ENTRY_REGEX.Match(trimmedLine);
			if (match.Success)
			{
				if (string.IsNullOrEmpty(loottable))
				{
					throw new InvalidFormatException($"No loottable name defined before line '{eachLine}'. Did you forget a 'loottable = <LootTableName>'?");
				}

				LootTableEntry entry = new()
				{
					PrefabName = match.Groups[1].Value,
					Weight = ParseInt(match.Groups[2].Value, 0, eachLine)
				};
				LootTableManager.AddLootTableEntry(loottable!, entry);
				continue;
			}

			//Only runs if nothing matches
			throw new InvalidFormatException($"Unrecognized line '{eachLine}'.");
		}
	}
}
