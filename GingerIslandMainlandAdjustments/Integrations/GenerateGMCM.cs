using System.Reflection;
using GingerIslandMainlandAdjustments.AtraStuff.Integrations;
using GingerIslandMainlandAdjustments.Configuration;

namespace GingerIslandMainlandAdjustments.Integrations;

/// <summary>
/// Class that generates the GMCM for this mod.
/// </summary>
internal static class GenerateGMCM
{
    private static IGenericModConfigMenuApi? api = null;
    private static IManifest? manifest = null;

    /// <summary>
    /// Grabs the GMCM api for this mod.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="translation">The translation helper.</param>
    internal static void Initialize(IManifest manifest, ITranslationHelper translation)
    {
        if ((api = Globals.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu")) is null)
        {
            return;
        }
        GenerateGMCM.manifest = manifest;
    }

    /// <summary>
    /// Generates the GMCM for this mod.
    /// </summary>
    internal static void Build()
    {
        if (api == null || manifest == null)
        {
            return;
        }

        api.Unregister(manifest);
        api.Register(
            manifest,
            reset: static () => Globals.Config = new ModConfig(),
            save: static () => Globals.Helper.WriteConfig(Globals.Config)
        );
        api.AddParagraph(manifest, I18n.ModDescription);
        api.AddBoolOption(
            manifest,
            static () => Globals.Config.EnforceGITiming,
            static value => Globals.Config.EnforceGITiming = value,
            I18n.Config_EnforceGITiming_Title,
            I18n.Config_EnforceGITiming_Description
        );
        api.AddBoolOption(
            manifest,
            static () => Globals.Config.RequireResortDialogue,
            static value => Globals.Config.RequireResortDialogue = value,
            I18n.Config_RequireResortDialogue_Title,
            I18n.Config_RequireResortDialogue_Description
        );
        api.AddTextOption(
            manifest,
            static () => Globals.Config.WearIslandClothing.ToString(),
            static value => Globals.Config.WearIslandClothing = Enum.Parse<WearIslandClothing>(value),
            I18n.Config_WearIslandClothing_Title,
            I18n.Config_WearIslandClothing_Description,
            Enum.GetNames(typeof(WearIslandClothing)),
            name => I18n.GetByKey($"config.WearIslandClothing.{name}").ToString()
        );
        api.AddBoolOption(
            manifest,
            static () => Globals.Config.UseThisScheduler,
            static value => Globals.Config.UseThisScheduler = value,
            I18n.Config_Scheduler_Title,
            I18n.Config_Scheduler_Otheroptions
        );
        api.AddParagraph(manifest, I18n.Config_Scheduler_Otheroptions);
        api.AddNumberOption(
            manifest,
            static () => Globals.Config.Capacity,
            static value => Globals.Config.Capacity = value,
            I18n.Config_Capacity_Title,
            I18n.Config_Capacity_Description,
            min: 0,
            max: 15
        );
        api.AddBoolOption(
            manifest,
            static () => Globals.Config.StageFarNpcsAtSaloon,
            static value => Globals.Config.StageFarNpcsAtSaloon = value,
            I18n.Config_Stage_Title,
            I18n.Config_Stage_Description
        );
        api.AddNumberOption(
            manifest,
            static () => Globals.Config.GroupChance,
            static value => Globals.Config.GroupChance = value,
            I18n.Config_GroupChance_Title,
            I18n.Config_GroupChance_Description,
            min: 0f,
            max: 1f,
            interval: 0.01f,
            formatValue: TwoPlaceFixedPoint
        );
        api.AddNumberOption(
            manifest,
            static () => Globals.Config.ExplorerChance,
            static value => Globals.Config.ExplorerChance = value,
            I18n.Config_ExplorerChance_Title,
            I18n.Config_ExplorerChance_Description,
            min: 0f,
            max: 1f,
            interval: 0.01f,
            formatValue: TwoPlaceFixedPoint
        );
        api.AddTextOption(
            manifest,
            static () => Globals.Config.GusDay.ToString(),
            static value => Globals.Config.GusDay = Enum.Parse<Configuration.DayOfWeek>(value),
            I18n.Config_WearIslandClothing_Title,
            I18n.Config_WearIslandClothing_Description,
            Enum.GetNames(typeof(Configuration.DayOfWeek)),
            name => I18n.GetByKey($"config.DayOfWeek.{name}").ToString()
        );
        api.AddNumberOption(
            manifest,
            static () => Globals.Config.GusChance,
            static value => Globals.Config.GusChance = value,
            I18n.Config_GusChance_Title,
            I18n.Config_GusChance_Description,
            min: 0f,
            max: 1f,
            interval: 0.01f,
            formatValue: TwoPlaceFixedPoint
        );

        foreach (PropertyInfo property in typeof(ModConfig).GetProperties())
        {
            if (property.Name.StartsWith("Allow", StringComparison.OrdinalIgnoreCase))
            {
                if (property.PropertyType == typeof(bool))
                {
                    api.AddBoolOption(
                        manifest,
                        () => (bool)(property.GetValue(Globals.Config) ?? false),
                        value => property.SetValue(Globals.Config, value),
                        I18n.GetByKey($"{property.Name}.title").ToString,
                        I18n.GetByKey($"{property.Name}.description").ToString
                    );
                }
                else if (property.PropertyType == typeof(VillagerExclusionOverride))
                {
                    api.AddTextOption(
                        manifest,
                        () => property.GetValue(Globals.Config)?.ToString() ?? "???",
                        value => property.SetValue(Globals.Config, Enum.Parse<VillagerExclusionOverride>(value)),
                        I18n.GetByKey($"{property.Name}.title").ToString,
                        I18n.GetByKey($"{property.Name}.description").ToString,
                        Enum.GetNames(typeof(VillagerExclusionOverride)),
                        name => I18n.GetByKey($"config.VillagerExclusionOverride.{name}").ToString()
                    );
                }
            }
        }
    }

    internal static void BuildNPCDictionary()
    {
        if (api == null || manifest == null)
        {
            return;
        }

        Globals.Config.PopulateScheduleStrictness();

        api.AddPageLink(manifest, "strictness", I18n.ScheduleStrictness, I18n.ScheduleStrictness_Description);
        api.AddPage(manifest, "strictness", I18n.ScheduleStrictness);
        api.AddParagraph(manifest, I18n.ScheduleStrictness_Description);
        foreach ((string k, ScheduleStrictness v) in Globals.Config.ScheduleStrictness)
        {
            api.AddTextOption(
                manifest,
                () =>
                    (
                        Globals.Config.ScheduleStrictness.TryGetValue(k, out ScheduleStrictness val)
                            ? val
                            : ScheduleStrictness.Default
                    ).ToString(),
                value => Globals.Config.ScheduleStrictness[k] = Enum.Parse<ScheduleStrictness>(value),
                () => Game1.getCharacterFromName(k)?.displayName ?? k,
                allowedValues: Enum.GetNames(typeof(ScheduleStrictness)),
                formatAllowedValue: name => I18n.GetByKey($"config.ScheduleStrictness.{name}").ToString()
            );
        }

        Globals.Helper.WriteConfig(Globals.Config);
    }

    private static string TwoPlaceFixedPoint(float f) => $"{f:f2}";
}
