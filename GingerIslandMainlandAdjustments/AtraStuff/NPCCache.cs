using System.Runtime.CompilerServices;

namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Much simpler NPC cache
/// </summary>
public static class NPCCache
{
    private static readonly Dictionary<string, NPC> _cache = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NPC? GetByVillagerName(string name)
    {
        if (!_cache.TryGetValue(name, out NPC? npc))
        {
            npc = Game1.getCharacterFromName(name);
            _cache[name] = npc;
        }
        return npc;
    }

    public static void Clear() => _cache.Clear();
}
