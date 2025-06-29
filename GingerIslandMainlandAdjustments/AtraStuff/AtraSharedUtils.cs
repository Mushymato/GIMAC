using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using xTile.Layers;

namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Utility methods.
/// </summary>
public static class AtraSharedUtils
{
    /// <summary>
    /// Gets the configuration instance, or returns a default one.
    /// </summary>
    /// <typeparam name="T">Type of config.</typeparam>
    /// <param name="helper">Smapi's helper.</param>
    /// <param name="monitor">Logger.</param>
    /// <returns>Config.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T GetConfigOrDefault<T>(IModHelper helper, IMonitor monitor)
        where T : class, new()
    {
        try
        {
            return helper.ReadConfig<T>();
        }
        catch (Exception ex)
        {
            monitor.Log(
                helper
                    .Translation.Get("IllFormatedConfig")
                    .Default(
                        "Config file seems ill-formated, using default. Please use Generic Mod Config Menu to configure."
                    ),
                LogLevel.Warn
            );
            monitor.Log(ex.ToString());
            return new();
        }
    }

    /// <summary>
    /// Yields all tiles around a specific tile.
    /// </summary>
    /// <param name="tile">Vector2 location of tile.</param>
    /// <param name="radius">A radius to search in.</param>
    /// <returns>All tiles within radius.</returns>
    /// <remarks>This actually returns a square, not a circle.</remarks>
    public static IEnumerable<Point> YieldSurroundingTiles(Vector2 tile, int radius = 1)
    {
        int x = (int)tile.X;
        int y = (int)tile.Y;
        for (int xdiff = -radius; xdiff <= radius; xdiff++)
        {
            for (int ydiff = -radius; ydiff <= radius; ydiff++)
            {
                yield return new Point(x + xdiff, y + ydiff);
            }
        }
    }

    /// <summary>
    /// Yields an iterator over all building tiles on a location.
    /// </summary>
    /// <param name="location">Location to check.</param>
    /// <returns>IEnumerable of all tiles.</returns>
    public static IEnumerable<Vector2> YieldAllTilesBuildings(GameLocation location)
    {
        if (location == null || location.Map == null)
            yield break;

        Layer layer = location.Map.RequireLayer("Buildings");
        for (int x = 0; x < layer.LayerWidth; x++)
        {
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                yield return new Vector2(x, y);
            }
        }
    }

    /// <summary>
    /// Tries to get a CultureInfo corresponding to the player's current culture. Falls back to the thread
    /// culture.
    /// </summary>
    /// <returns>CultureInfo.</returns>
    public static CultureInfo GetCurrentCulture()
    {
        try
        {
            return new CultureInfo(LocalizedContentManager.LanguageCodeString(Game1.content.GetCurrentLanguage()));
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.CurrentCulture;
        }
    }

    /// <summary>
    /// Returns a StringComparer for the current language the player is using.
    /// </summary>
    /// <param name="ignoreCase">Whether or not to ignore case.</param>
    /// <returns>A string comparer.</returns>
    public static StringComparer GetCurrentLanguageComparer(bool ignoreCase = false) =>
        StringComparer.Create(GetCurrentCulture(), ignoreCase);

    /// <summary>
    /// Sort strings, taking into account CultureInfo of currently selected language.
    /// </summary>
    /// <param name="enumerable">IEnumerable of strings to sort.</param>
    /// <returns>A sorted list of strings.</returns>
    public static List<string> ContextSort(IEnumerable<string> enumerable)
    {
        List<string> outputlist = enumerable.ToList();
        outputlist.Sort(GetCurrentLanguageComparer(ignoreCase: true));
        return outputlist;
    }

    // Thanks, RSV, for reminding me that there are other conditions for which I should probably not be handling shops....
    // From: https://github.com/Rafseazz/Ridgeside-Village-Mod/blob/816a66d0c9e667d3af662babc170deed4070c9ff/Ridgeside%20SMAPI%20Component%202.0/RidgesideVillage/TileActionHandler.cs#L37

    /// <summary>
    /// A couple of common checks.
    /// </summary>
    /// <returns>True if raising a menu is reasonable, false if that would be unwise.</returns>
    public static bool IsNormalGameplay() =>
        Game1.keyboardDispatcher.Subscriber is null
        && Context.IsWorldReady
        && Context.CanPlayerMove
        && !Game1.player.isRidingHorse()
        && Game1.currentLocation is not null
        && !Game1.eventUp
        && !Game1.isFestival()
        && !Game1.IsFading()
        && Game1.activeClickableMenu is null;
}
