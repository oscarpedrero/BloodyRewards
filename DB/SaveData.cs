﻿using BloodyRewards.DB;
using System;
using System.IO;
using System.Text.Json;

namespace BloodyRewards.DB
{
    internal class SaveDataToFiles
    {

        public static bool saveKillerPVPList()
        {
            try
            {
                var KillerPVPLis = ShareDB.getKillersPVPList();
                var jsonOutPut = JsonSerializer.Serialize(KillerPVPLis);
                File.WriteAllText(LoadDataFromFiles.KillerPVPListFile, jsonOutPut);
                
                return true;
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static bool saveRewardsList()
        {
            try
            {
                var rewardList = ShareDB.getRewardList();
                var jsonOutPut = JsonSerializer.Serialize(rewardList);
                File.WriteAllText(LoadDataFromFiles.RewardListFile, jsonOutPut);
                
                return true;
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static bool saveDayliTimeLogin()
        {
            try
            {
                var rewardList = ShareDB.getDayliLoginTimeModel();
                var jsonOutPut = JsonSerializer.Serialize(rewardList);
                File.WriteAllText(LoadDataFromFiles.RewardLDayliistFile, jsonOutPut);
                
                return true;
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static bool saveUsersRewardsPerDay()
        {
            try
            {
                var usersRewardsPerDayList = ConfigDB.UsersRewardsPerDay;
                var jsonOutPut = JsonSerializer.Serialize(usersRewardsPerDayList);
                File.WriteAllText(LoadDataFromFiles.UserRewardsPerDayFile, jsonOutPut);
                
                return true;
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }
    }
}
