using System.Runtime.CompilerServices;

namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Not actually a cache, just stub for atra's cache
/// </summary>
public static class NPCCache
{
    /// <summary>
    /// Just uses the vanilla getCharacterFromName
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NPC? GetByVillagerName(string name) => Game1.getCharacterFromName(name);
}
