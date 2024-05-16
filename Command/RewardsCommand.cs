using BloodyRewards.DB.Models;
using BloodyRewards.DB;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using Bloody.Core;
using Bloody.Core.API;
using VampireCommandFramework;
using Unity.Entities;

namespace BloodyRewards.Command
{
    [CommandGroup("brw")]
    internal class RewardsCommand
    {
        public static RewardModel reward { get; private set; }
        public static List<RewardModel> rewards { get; private set; }

        [Command("add", usage: "\"<Name>\" <PrefabGuid> <OnlyForVBlood true/false>", description: "Add a reward. To know the PrefabGuid of an item you must look for the item in the following URL <#4acc45><u>https://gaming.tools/v-rising/items</u></color>", adminOnly: true)]
        public static void AddReward(ChatCommandContext ctx, string name, int item, bool onlyVBlood)
        {

            var prefabGUID = new PrefabGUID(item);
            //Entity entity = Plugin.SystemsCore.PrefabCollectionSystem.PrefabLookupMap[prefabGUID];
            //var itemModel = Core.Items.FromEntity(entity);
            //var itemModel = Core.Items.GetPrefabById(prefabGUID);

            /*if (itemModel == null)
            {
                throw ctx.Error("Invalid item type");
            }*/

            if (!ShareDB.addRewardList(name, item, onlyVBlood))
            {
                throw ctx.Error("Invalid item type");
            }

            SaveDataToFiles.saveRewardsList();

            ctx.Reply(FontColorChatSystem.Yellow($"Added reward {FontColorChatSystem.White($"{name}")} to the store"));

        }

        [Command("list", usage: "", description: "List of rewards available to buy in the store", adminOnly: true)]
        public static void ListReward(ChatCommandContext ctx)
        {

            if (!ConfigDB.RewardsEnabled)
            {
                throw ctx.Error("Reward System is disabled");
            }

            var rewardList = ShareDB.GetRewardListMessage();

            if (rewardList.Count <= 0)
            {
                throw ctx.Error("No reward available");
            }

            foreach (string item in rewardList)
            {
                ctx.Reply(item);
            }

        }

        [Command("remove", shortHand: "crm", usage: "<NumberItem>", description: "Delete a reward", adminOnly: true)]
        public static void RemoveReward(ChatCommandContext ctx, int index)
        {

            try
            {
                if (ShareDB.rewardsList.Count == 1)
                {
                    throw ctx.Error(FontColorChatSystem.Yellow($"Do not remove all reward from the store."));
                }
                if (!ShareDB.SearchRewardByCommand(index, out RewardModel currecyModel))
                {
                    throw ctx.Error(FontColorChatSystem.Yellow($"Reward removed error."));
                }
                if (!ShareDB.RemoveRewardyByCommand(index))
                {
                    throw ctx.Error(FontColorChatSystem.Yellow($"Reward {FontColorChatSystem.White($"{currecyModel.name}")} removed error."));
                }

                SaveDataToFiles.saveRewardsList();
                LoadDataFromFiles.loadRewards();

                ctx.Reply(FontColorChatSystem.Yellow($"Reward {FontColorChatSystem.White($"{currecyModel.name}")} removed successful."));

            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                throw ctx.Error($"Error: {error.Message}");
            }

        }
    }
}
