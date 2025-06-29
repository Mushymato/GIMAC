﻿namespace GingerIslandMainlandAdjustments.Niceties;

using GingerIslandMainlandAdjustments.AtraStuff;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

/// <summary>
/// Holds patches against GameLocation to prevent trampling of objects on IslandWest.
/// </summary>
[HarmonyPatch(typeof(GameLocation))]
internal class GameLocationPatches
{
    /// <summary>
    /// Prefix to prevent trampling.
    /// </summary>
    /// <param name="__instance">Gamelocation.</param>
    /// <returns>True to continue to original function, false to skip original function.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameLocation.characterTrampleTile), [typeof(Vector2)])]
    private static bool PrefixCharacterTrample(GameLocation __instance)
    {
        try
        {
            return __instance is not IslandWest;
        }
        catch (Exception ex)
        {
            Globals.ModMonitor.LogError($"preventing trampling at {__instance.NameOrUniqueName}", ex);
        }
        return true;
    }

    /// <summary>
    /// Prefix to prevent characters from destroying things.
    /// </summary>
    /// <param name="__instance">GameLocation.</param>
    /// <returns>True to continue to original function, false to skip original function.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(
        nameof(GameLocation.characterDestroyObjectWithinRectangle),
        new Type[] { typeof(Rectangle), typeof(bool) }
    )]
    private static bool PrefixCharacterDestroy(GameLocation __instance)
    {
        try
        {
            return __instance is not IslandWest;
        }
        catch (Exception ex)
        {
            Globals.ModMonitor.LogError($"preventing trampling at {__instance.NameOrUniqueName}", ex);
        }
        return true;
    }
}
