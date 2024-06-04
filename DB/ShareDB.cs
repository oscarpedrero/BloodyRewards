using Bloody.Core;
using Bloody.Core.Models.v1;
using ProjectM;
using BloodyRewards.DB.Models;
using System.Collections.Generic;
using System.Linq;
using Bloody.Core.API.v1;
using System;

namespace BloodyRewards.DB
{
    public class ShareDB
    {

        public static List<RewardModel> rewardsList = new();

        public static List<DayliLoginTimeModel> ListDayliLoginTimeModel = new();

        public static List<RewardModel> getRewardList()
        {
            return rewardsList;
        }

        public static List<DayliLoginTimeModel> getDayliLoginTimeModel()
        {
            return ListDayliLoginTimeModel;
        }

        public static void addDayliLoginTimeModel(DayliLoginTimeModel dayliLoginTimeModel)
        {
            ListDayliLoginTimeModel.Add(dayliLoginTimeModel);
        }

        public static RewardModel getReward(int guid)
        {
            return rewardsList.FirstOrDefault(reward => reward.guid == guid);
        }

        public static RewardModel getRewardByName(string name)
        {
            return rewardsList.FirstOrDefault(reward => reward.name == name);
        }

        public static bool setRewardList(List<RewardModel> rewards)
        {

            rewardsList = rewards;

            return true;
        }

        public static bool settDayliLoginTimeModel(List<DayliLoginTimeModel> rewards)
        {

            ListDayliLoginTimeModel = rewards;

            return true;
        }

        public static bool addRewardList(string name, int guid, bool onlyVBlood)
        {
            var reward = new RewardModel();
            var id = getRewardList().Last().id +1;
            reward.name = name;
            reward.guid = guid;
            reward.id = id;
            reward.onlyVBlood = onlyVBlood;

            rewardsList.Add(reward);

            return true;
        }

        public static List<string> GetRewardListMessage()
        {
            var listReward = new List<string>();

            foreach (RewardModel item in rewardsList)
            {
                listReward.Add($"{FontColorChatSystem.White("[")}{FontColorChatSystem.Yellow(item.id.ToString())}{FontColorChatSystem.White("]")} " +
                        $"{FontColorChatSystem.Yellow(item.name)} ");
            }

            return listReward;

        }

        public static bool SearchRewardByCommand(int index, out RewardModel rewardModel)
        {
           
            rewardModel = rewardsList.FirstOrDefault(reward => reward.id == index);
            if (rewardModel == null)
                return false;

            return true;
        }

        public static bool RemoveRewardyByCommand(int index)
        {
            try
            {
                var rewards = rewardsList.RemoveAll(reward => reward.id == index);
                return true;
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                return false;
            }

        }

        public static List<RewardModel> searchRewardByNameForShop(string text)
        {

            var result = new List<RewardModel>();
            foreach (var reward in rewardsList)
            {
                if (reward.name.Contains(text))
                {
                    result.Add(reward);
                }
            }

            return result;
        }

        public static int searchIdForReward(int GUID)
        {
            foreach (var reward in rewardsList)
            {
                if (reward.guid == GUID)
                {
                    return reward.id;
                }
            }

            return -1;
        }
    }
}
