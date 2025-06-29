namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Functions to help with handling multi-player.
/// </summary>
public static class MultiplayerHelpers
{
    /// <summary>
    /// Checks if the versions installed of the mod are the same for farmhands.
    /// Prints errors to console if wrong.
    /// </summary>
    /// <param name="multi">Multi-player helper.</param>
    /// <param name="manifest">Manifest of mod.</param>
    /// <param name="monitor">Logger.</param>
    /// <param name="translation">Translation helper.</param>
    /// <returns>Whether mod version matches or not.</returns>
    public static bool AssertMultiplayerVersions(
        IMultiplayerHelper multi,
        IManifest manifest,
        IMonitor monitor,
        ITranslationHelper translation
    )
    {
        if (Context.IsMultiplayer && !Context.IsMainPlayer && !Context.IsSplitScreen)
        {
            if (
                multi.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(manifest.UniqueID)
                is not IMultiplayerPeerMod hostMod
            )
            {
                monitor.Log(
                    translation
                        .Get("host-not-installed")
                        .Default(
                            "The host does not seem to have this mod installed. Some features may not be available."
                        ),
                    LogLevel.Warn
                );
                PlayerAlertHandler.AddMessage(
                    new HUDMessage($"Mismatched mods {manifest.UniqueID} may cause issues", HUDMessage.error_type)
                );
                return false;
            }
            else if (!hostMod.Version.Equals(manifest.Version))
            {
                monitor.Log(
                    translation
                        .Get("host-version-different")
                        .Default(
                            "The host seems to have a different version of this mod ({{version}}). Some features may not work."
                        )
                        .Tokens(new { version = manifest.Version }),
                    LogLevel.Warn
                );
                PlayerAlertHandler.AddMessage(
                    new HUDMessage(
                        $"Mismatched mod version {manifest.UniqueID} may cause issues",
                        HUDMessage.error_type
                    )
                );
                return false;
            }
        }

        return true;
    }
}
