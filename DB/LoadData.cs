﻿using BepInEx;
using BloodyRewards.DB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BloodyRewards.DB
{
    internal class LoadDataFromFiles
    {

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "BloodyRewards");
        public static string RewardListFile = Path.Combine(ConfigPath, "currency_list.json");
        public static string KillerPVPListFile = Path.Combine(ConfigPath, "pvp_kills.json");
        public static string RewardLDayliistFile = Path.Combine(ConfigPath, "dayli_time_online_list.json");
        public static string UserRewardsPerDayFile = Path.Combine(ConfigPath, "user_currencies_per_day.json");

        public static bool loadRewards()
        {
            try
            {
                string json = File.ReadAllText(RewardListFile);

                var rewardsList = JsonSerializer.Deserialize<List<RewardModel>>(json);
                return ShareDB.setRewardList(rewardsList);
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static bool loadKillers()
        {
            try
            {
                string json = File.ReadAllText(KillerPVPListFile);

                var killersList = JsonSerializer.Deserialize<List<PvpModel>>(json);
                return ShareDB.setKillerPvpList(killersList);
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }
        public static bool loadRewardLDayliistFiles()
        {
            try
            {
                string json = File.ReadAllText(RewardLDayliistFile);
                var rewardsList = JsonSerializer.Deserialize<List<DayliLoginTimeModel>>(json);
                return ShareDB.settDayliLoginTimeModel(rewardsList);
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static bool loadUserRewardsPerDay()
        {
            try
            {
                string json = File.ReadAllText(UserRewardsPerDayFile);
                var usersRewardsPerDaList = JsonSerializer.Deserialize<List<UserRewardsPerDayModel>>(json);
                //Plugin.Logger.LogInfo($"Total usersRewardsPerDay List FROM JSON {usersRewardsPerDaList.Count}");
                return ConfigDB.setUsersRewardsPerDay(usersRewardsPerDaList);
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static void CreateFilesConfig()
        {

            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);

            if (!File.Exists(RewardListFile)) File.WriteAllText(RewardListFile, "[{\"id\":1,\"name\":\"Silver Coin\",\"guid\":-949672483}]");


            if (!File.Exists(UserRewardsPerDayFile)) File.WriteAllText(UserRewardsPerDayFile, "[]");

            if (!File.Exists(RewardLDayliistFile)) File.WriteAllText(RewardLDayliistFile, "[]");
            if (!File.Exists(KillerPVPListFile)) File.WriteAllText(KillerPVPListFile, "[]");

        }

        public static void LoadRewardsToDB()
        {
            if (!loadRewards())
            {
                Plugin.Logger.LogError($"Error loading RewardsList");
            }
        }

        public static void LoadUserRewardsPerDayToDB()
        {
            if (!loadUserRewardsPerDay())
            {
                Plugin.Logger.LogError($"Error loading loadUserRewardsPerDay");
            }
        }

        public static void LoadRewardLDayliistFiles()
        {
            if (!loadRewardLDayliistFiles())
            {
                Plugin.Logger.LogError($"Error loading loadUserRewardsPerDay");
            }
        }

        public static void LoadKillersListFiles()
        {
            if (!loadKillers())
            {
                Plugin.Logger.LogError($"Error loading loadKillers");
            }
        }

        public static void SetConfigMod()
        {

            ConfigDB.RewardsEnabled = Plugin.RewardsEnabled.Value;

            ConfigDB.DropNpcPercentage = Plugin.DropNpcPercentage.Value;
            ConfigDB.IncrementPercentageDropEveryTenLevelsNpc = Plugin.IncrementPercentageDropEveryTenLevelsNpc.Value;
            ConfigDB.DropdNpcRewardsMin = Plugin.DropdNpcRewardsMin.Value;
            ConfigDB.DropNpcRewardsMax = Plugin.DropNpcRewardsMax.Value;
            ConfigDB.MaxRewardsPerDayPerPlayerNpc = Plugin.MaxRewardsPerDayPerPlayerNpc.Value;

            ConfigDB.DropdVBloodPercentage = Plugin.DropdVBloodPercentage.Value;
            ConfigDB.IncrementPercentageDropEveryTenLevelsVBlood = Plugin.IncrementPercentageDropEveryTenLevelsVBlood.Value;
            ConfigDB.DropVBloodRewardsMin = Plugin.DropVBloodRewardsMin.Value;
            ConfigDB.DropVBloodRewardsMax = Plugin.DropVBloodRewardsMax.Value;
            ConfigDB.MaxRewardsPerDayPerPlayerVBlood = Plugin.MaxRewardsPerDayPerPlayerVBlood.Value;

            ConfigDB.DropPvpPercentage = Plugin.DropPvpPercentage.Value;
            ConfigDB.IncrementPercentageDropEveryTenLevelsPvp = Plugin.IncrementPercentageDropEveryTenLevelsPvp.Value;
            ConfigDB.DropPvpRewardsMin = Plugin.DropPvpRewardsMin.Value;
            ConfigDB.DropPvpRewardsMax = Plugin.DropPvpRewardsMax.Value;
            ConfigDB.MaxRewardsPerDayPerPlayerPvp = Plugin.MaxRewardsPerDayPerPlayerPvp.Value;

            ConfigDB.WalletSystem = Plugin.WalletSystem.Value;
            ConfigDB.WalletAmountPveMax = Plugin.WalletAmountPveMax.Value;
            ConfigDB.WalletAmountPveMin = Plugin.WalletAmountPveMin.Value;
            ConfigDB.WalletAmountVBloodMax = Plugin.WalletAmountVBloodMax.Value;
            ConfigDB.WalletAmountVBloodMin = Plugin.WalletAmountVBloodMin.Value;
            ConfigDB.WalletAmountPVPMax = Plugin.WalletAmountPVPMax.Value;
            ConfigDB.WalletAmountPVPMin = Plugin.WalletAmountPVPMin.Value;

            ConfigDB.DailyLoginRewards = Plugin.DailyLoginRewards.Value;
            ConfigDB.AmountDailyLoginReward = Plugin.AmountDailyLoginReward.Value;

            ConfigDB.ConnectionTimeReward = Plugin.ConnectionTimeReward.Value;
            ConfigDB.AmountTimeReward = Plugin.AmountTimeReward.Value;
            ConfigDB.TimeReward = Plugin.TimeReward.Value;
        }
    }
}
