using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Bloody.Core;
using Bloody.Core.API.v1;
using BloodyRewards.DB;
using BloodyRewards.Systems;
using HarmonyLib;
using Unity.Entities;
using VampireCommandFramework;

namespace BloodyRewards;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("trodi.Bloody.Core")]
[BepInDependency("trodi.bloody.Wallet", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{

    Harmony _harmony;

    public static Bloody.Core.Helper.v1.Logger Logger;
    public static SystemsCore SystemsCore;

    /// 
    /// Rewards System
    /// 
    public static ConfigEntry<bool> RewardsEnabled;

    // NPC CONFIG DROP
    public static ConfigEntry<int> DropNpcPercentage;
    public static ConfigEntry<int> IncrementPercentageDropEveryTenLevelsNpc;
    public static ConfigEntry<int> DropdNpcRewardsMin;
    public static ConfigEntry<int> DropNpcRewardsMax;
    public static ConfigEntry<int> MaxRewardsPerDayPerPlayerNpc;

    // VBLOOD CONFIG DROP
    public static ConfigEntry<int> DropdVBloodPercentage;
    public static ConfigEntry<int> IncrementPercentageDropEveryTenLevelsVBlood;
    public static ConfigEntry<int> DropVBloodRewardsMin;
    public static ConfigEntry<int> DropVBloodRewardsMax;
    public static ConfigEntry<int> MaxRewardsPerDayPerPlayerVBlood;

    // PVP CONFIG DROP
    public static ConfigEntry<int> DropPvpPercentage;
    public static ConfigEntry<int> IncrementPercentageDropEveryTenLevelsPvp;
    public static ConfigEntry<int> DropPvpRewardsMin;
    public static ConfigEntry<int> DropPvpRewardsMax;
    public static ConfigEntry<int> MaxRewardsPerDayPerPlayerPvp;

    public static ConfigEntry<int> MaximumDeathsSamePlayer { get; set; }
    public static ConfigEntry<int> CoolDownDeathsSamePlayer { get; set; }

    // WALLET CONFIG
    public static ConfigEntry<bool> WalletSystem { get; set; }
    public static ConfigEntry<int> WalletAmountPveMax { get; set; }
    public static ConfigEntry<int> WalletAmountPveMin { get; set; }
    public static ConfigEntry<int> WalletAmountVBloodMax { get; set; }
    public static ConfigEntry<int> WalletAmountVBloodMin { get; set; }
    public static ConfigEntry<int> WalletAmountPVPMax { get; set; }
    public static ConfigEntry<int> WalletAmountPVPMin { get; set; }

    // DAYLI REWARDS CONFIG
    public static ConfigEntry<bool> DailyLoginRewards { get; set; }
    public static ConfigEntry<int> AmountDailyLoginReward { get; set; }

    // CONECTION TIME REWARDS CONFIG
    public static ConfigEntry<bool> ConnectionTimeReward { get; set; }
    public static ConfigEntry<int> AmountTimeReward { get; set; }
    public static ConfigEntry<int> TimeReward { get; set; }

    public override void Load()
    {
        Logger = new(Log);
        
        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        // Register all commands in the assembly with VCF
        CommandRegistry.RegisterAll();

        EventsHandlerSystem.OnInitialize += GameDataOnInitialize;
        InitConfigServer();

        LoadDataFromFiles.CreateFilesConfig();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private void GameDataOnInitialize(World world)
    {
        SystemsCore = Core.SystemsCore;

        LoadDataFromFiles.SetConfigMod();

        if (ConfigDB.DailyLoginRewards)
        {
            EventsHandlerSystem.OnUserConnected += OnlineRewards.OnlineUser;
        }

        if (ConfigDB.ConnectionTimeReward)
        {
            ConnectionTimeSystemRewards.UserRewardTimne();
        }

        if (ConfigDB.WalletSystem)
        {
            EventsHandlerSystem.OnDeath += RewardSystemVirtual.ServerEvents_OnDeath;
            EventsHandlerSystem.OnVampireDowned += RewardSystemVirtual.ServerEvents_OnVampireDowned;
        } else
        {
            EventsHandlerSystem.OnDeath += RewardSystem.ServerEvents_OnDeath;
            EventsHandlerSystem.OnVampireDowned += RewardSystem.ServerEvents_OnVampireDowned;
        }
        

        LoadDataFromFiles.LoadRewardsToDB();
        LoadDataFromFiles.LoadUserRewardsPerDayToDB();
        LoadDataFromFiles.LoadRewardLDayliistFiles();
        LoadDataFromFiles.LoadKillersListFiles();
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void InitConfigServer()
    {
        // Rewards System CONFIG
        RewardsEnabled = Config.Bind("RewardsSystem", "enabled", true, "Enable Rewards System");

        // NPC DROP CONFIG
        DropNpcPercentage = Config.Bind("RewardsSystem", "minPercentageDropNpc", 5, "Percent chance that an NPC will drop the type of reward from the shop");
        IncrementPercentageDropEveryTenLevelsNpc = Config.Bind("RewardsSystem", "IncrementPercentageDropEveryTenLevelsNpc", 5, "Percentage increase for every rank of 10 levels of the NPC");
        DropdNpcRewardsMin = Config.Bind("RewardsSystem", "DropdNpcRewardsMin", 1, "Minimum reward an NPC can drop");
        DropNpcRewardsMax = Config.Bind("RewardsSystem", "DropNpcRewardsMax", 5, "Maximum reward an NPC can drop");
        MaxRewardsPerDayPerPlayerNpc = Config.Bind("RewardsSystem", "MaxRewardsPerDayPerPlayerNpc", 5, "Maximum number of reward that a user can get per day by NPC death");

        // VBLOOD DROP CONFIG
        DropdVBloodPercentage = Config.Bind("RewardsSystem", "minPercentageDropVBlood", 20, "Percent chance that an VBlood will drop the type of reward from the shop");
        IncrementPercentageDropEveryTenLevelsVBlood = Config.Bind("RewardsSystem", "IncrementPercentageDropEveryTenLevelsVBlood", 5, "Percentage increase for every rank of 10 levels of the VBlood");
        DropVBloodRewardsMin = Config.Bind("RewardsSystem", "DropVBloodRewardsMin", 10, "Minimum reward an VBlood can drop");
        DropVBloodRewardsMax = Config.Bind("RewardsSystem", "DropVBloodRewardsMax", 20, "Maximum reward an VBlood can drop");
        MaxRewardsPerDayPerPlayerVBlood = Config.Bind("RewardsSystem", "MaxRewardsPerDayPerPlayerVBlood", 20, "Maximum number of reward that a user can get per day by VBlood death");

        // PVP DROP CONFIG
        DropPvpPercentage = Config.Bind("RewardsSystem", "minPercentageDropPvp", 100, "Percent chance that victory in a PVP duel will drop the type of reward in the store");
        IncrementPercentageDropEveryTenLevelsPvp = Config.Bind("RewardsSystem", "IncrementPercentageDropEveryTenLevelsPvp", 5, "Percentage increase for every rank of 10 levels of the Player killed in pvp duel");
        DropPvpRewardsMin = Config.Bind("RewardsSystem", "DropPvpRewardsMin", 15, "Minimum reward can drop victory in PVP");
        DropPvpRewardsMax = Config.Bind("RewardsSystem", "DropPvpRewardsMax", 20, "Maximum reward can drop victory in PVP");
        MaxRewardsPerDayPerPlayerPvp = Config.Bind("RewardsSystem", "MaxRewardsPerDayPerPlayerPvp", 20, "Maximum number of reward that a user can get per day by victory in PVP");
        MaximumDeathsSamePlayer = Config.Bind("RewardsSystem", "MaximumDeathsSamePlayer", 1, "Maximum number of consecutive deaths to the same player that are allowed before executing the cooldown");
        CoolDownDeathsSamePlayer = Config.Bind("RewardsSystem", "CoolDownDeathsSamePlayer", 30, "Minutes a player must wait to receive a reward for killing the same player again");

        // WALLET CONFIG
        WalletSystem = Config.Bind("Wallet", "enabled", false, "Activate rewards in virtual currency through BloodyWallet ( https://github.com/oscarpedrero/BloodyWallet )");
        WalletAmountPveMax = Config.Bind("Wallet", "amountPveMax", 2, "Maximum amount of virtual coins for when you drop in PVE");
        WalletAmountPveMin = Config.Bind("Wallet", "amountPveMin", 1, "Minumun amount of virtual coins for when you drop in PVE");
        WalletAmountVBloodMax = Config.Bind("Wallet", "amountVBloodMax", 2, "Maximum amount of virtual coins for when you drop in VBlood");
        WalletAmountVBloodMin = Config.Bind("Wallet", "amountVBloodMin", 1, "Minimun amount of virtual coins for when you drop in VBlood");
        WalletAmountPVPMax = Config.Bind("Wallet", "amountPVPMax", 2, "Maximum amount of virtual coins for when you drop in PVE");
        WalletAmountPVPMin = Config.Bind("Wallet", "amountPVPMin", 1, "Minimun amount of virtual coins for when you drop in PVE");

        // DAYLI REWARDS CONFIG
        DailyLoginRewards = Config.Bind("DayliLoginRewardsSystem", "DailyLoginRewards", true, "Daily reward for connecting to the server");
        AmountDailyLoginReward = Config.Bind("DayliLoginRewardsSystem", "AmountDailyLoginReward", 15, "Amount of rewards for login");

        // CONECTION TIME REWARDS CONFIG
        ConnectionTimeReward = Config.Bind("ConnectionTimeReward", "ConnectionTimeReward", true, "Connection time reward");
        AmountTimeReward = Config.Bind("ConnectionTimeReward", "AmountTimeReward", 1, "Amount of rewards for connection time");
        TimeReward = Config.Bind("ConnectionTimeReward", "TimeReward", 5, "Every how many minutes the reward will be awarded");
    }

    public void OnGameInitialized()
    {
        
    }

}
