namespace GingerIslandMainlandAdjustments;

using GingerIslandMainlandAdjustments.AssetManagers;
using GingerIslandMainlandAdjustments.AtraStuff;
using GingerIslandMainlandAdjustments.CustomConsoleCommands;
using GingerIslandMainlandAdjustments.DialogueChanges;
using GingerIslandMainlandAdjustments.Integrations;
using GingerIslandMainlandAdjustments.Niceties;
using GingerIslandMainlandAdjustments.ScheduleManager;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    private bool haveFixedSchedulesToday = false;

    /// <summary>
    /// Gets the logger for this mod.
    /// </summary>
    public static IMonitor ModMonitor { get; private set; } = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModMonitor = this.Monitor;
        this.Monitor.Log($"Starting up: {this.ModManifest.UniqueID} - {this.GetType().Assembly.FullName}");

        I18n.Init(helper.Translation);
        Globals.Initialize(helper, this.Monitor, this.ModManifest);
        AssetEditor.Initialize(helper.GameContent);

        ConsoleCommands.Register(this.Helper.ConsoleCommands);

        // Register events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        helper.Events.GameLoop.DayStarted += static (_, _) => MarriageDialogueHandler.OnDayStart();
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        helper.Events.GameLoop.ReturnedToTitle += this.ReturnedToTitle;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Player.Warped += this.OnPlayerWarped;

        helper.Events.Content.AssetRequested += this.OnAssetRequested;

        AssetLoader.Init(helper.GameContent);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            AssetEditor.DisposeContentManager();
        }
        base.Dispose(disposing);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        AssetLoader.Load(e);
        AssetEditor.Edit(e);
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        MultiplayerHelpers.AssertMultiplayerVersions(
            this.Helper.Multiplayer,
            Globals.Manifest,
            Globals.ModMonitor,
            this.Helper.Translation
        );
        if (Context.IsSplitScreen && Context.ScreenId != 0)
        {
            return;
        }

        GenerateGMCM.BuildNPCDictionary();
        Globals.LoadDataFromSave();

        if (Game1.getLocationFromName("IslandSouth") is IslandSouth islandSouth)
        {
            this.Monitor.DebugOnlyLog("Found IslandSouth.", LogLevel.Info);
            if (
                islandSouth.parrotUpgradePerches.FirstOrDefault(perch =>
                    perch.tilePosition.X == 17 && perch.tilePosition.Y == 22
                )
                    is ParrotUpgradePerch perch
                && perch.currentState.Value != ParrotUpgradePerch.UpgradeState.Complete
            )
            {
                this.Monitor.DebugOnlyLog("Found perch, applying watching.", LogLevel.Info);
                IslandSouthWatcher southWatcher = new(this.Helper.GameContent);
                perch.upgradeCompleteEvent.onEvent += southWatcher.OnResortFixed;
            }
        }
    }

    /// <summary>
    /// Clear all caches at the end of the day and if the player exits to menu.
    /// </summary>
    private void ClearCaches()
    {
        NPCCache.Clear();
        DialoguePatches.ClearTalkRecord();
        DialogueUtilities.ClearDialogueLog();

        if (Context.IsSplitScreen && Context.ScreenId != 0)
        {
            return;
        }
        this.haveFixedSchedulesToday = false;
        MidDayScheduleEditor.Reset();
        IslandSouthPatches.ClearCache();
        GIScheduler.ClearCache();
        GIScheduler.DayEndReset();
        ConsoleCommands.ClearCache();
        ScheduleUtilities.ClearCache();
    }

    /// <summary>
    /// Clear caches when returning to title.
    /// </summary>
    /// <param name="sender">Unknown, never used.</param>
    /// <param name="e">Possible parameters.</param>
    private void ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.ClearCaches();
        GenerateGMCM.Build();
    }

    /// <summary>
    /// Clear cache at day end.
    /// </summary>
    /// <param name="sender">Unknown, never used.</param>
    /// <param name="e">Possible parameters.</param>
    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        this.ClearCaches();
        NPCPatches.ResetAllFishers();

        if (Context.IsMainPlayer)
        {
            Game1.netWorldState.Value.IslandVisitors.Clear();
            Globals.SaveCustomData();
        }
    }

    /// <summary>
    /// Applies and logs this mod's harmony patches.
    /// </summary>
    /// <param name="harmony">My harmony instance.</param>
    private void ApplyPatches(Harmony harmony)
    {
        try
        {
            // handle patches from annotations.
            harmony.PatchAll(typeof(ModEntry).Assembly);
            if (Globals.Config.DebugMode)
            {
                ScheduleDebugPatches.ApplyPatches(harmony);
            }
        }
        catch (Exception ex)
        {
            Globals.ModMonitor.Log(string.Format(ErrorMessageConsts.HARMONYCRASH, ex), LogLevel.Error);
        }

        harmony.Snitch(Globals.ModMonitor, this.ModManifest.UniqueID, transpilersOnly: true);
    }

    /// <summary>
    /// Initialization after other mods have started.
    /// </summary>
    /// <param name="sender">Unknown, never used.</param>
    /// <param name="e">Possible parameters.</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Applies harmony patches.
        this.ApplyPatches(new Harmony(this.ModManifest.UniqueID));

        // Generate the GMCM for this mod.
        GenerateGMCM.Initialize(this.ModManifest, this.Helper.Translation);
        GenerateGMCM.Build();

        // Add CP tokens for this mod.
        GenerateCPTokens.AddTokens(this.ModManifest);

        // Bind Child2NPC's IsChildNPC method
        if (Globals.GetIsChildToNPC())
        {
            Globals.ModMonitor.Log("Successfully grabbed Child2NPC for integration", LogLevel.Debug);
        }
    }

    private void OnPlayerWarped(object? sender, WarpedEventArgs e) => ShopHandler.AddBoxToShop(e);

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button.IsActionButton() && AtraSharedUtils.IsNormalGameplay())
        {
            ShopHandler.HandleWillyShop(e);
            ShopHandler.HandleSandyShop(e);
        }
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }
        MidDayScheduleEditor.AttemptAdjustGISchedule(e);
        if (!this.haveFixedSchedulesToday && e.NewTime > 615)
        {
            // No longer need the exclusions cache.
            IslandSouthPatches.ClearCache();

            ScheduleUtilities.FixUpSchedules();
            if (Globals.Config.DebugMode)
            {
                ScheduleDebugPatches.FixNPCs();
            }
            this.haveFixedSchedulesToday = true;
        }
    }
}
