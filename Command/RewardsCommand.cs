using BloodyRewards.DB.Models;
using BloodyRewards.DB;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using Bloody.Core.API.v1;
using VampireCommandFramework;
using BloodyRewards.Systems;
using UnityEngine;
using System.Collections;
using static ProjectM.RandomizedSpawnChainSettingsAuthoring;

namespace BloodyRewards.Command
{
    [CommandGroup("brw")]
    internal class RewardsCommand
    {
        public static RewardModel reward { get; private set; }
        public static List<RewardModel> rewards { get; private set; }

        public static bool forceRestartTimerConnection = false;

        [Command("add", usage: "\"<Name>\" <PrefabGuid> <OnlyForVBlood true/false>", description: "Add a reward. To know the PrefabGuid of an item you must look for the item in the following URL <#4acc45><u>https://gaming.tools/v-rising/items</u></color>", adminOnly: true)]
        public static void AddReward(ChatCommandContext ctx, string name, int item, bool onlyVBlood)
        {
            if (Plugin.WalletSystem.Value)
            {
                throw ctx.Error("BloodyWallet is activated as a reward so this command is disabled.");
            }
            var prefabGUID = new PrefabGUID(item);

            if (!ShareDB.addRewardList(name, item, onlyVBlood))
            {
                throw ctx.Error("Invalid item type");
            }

            SaveDataToFiles.saveRewardsList();

            ctx.Reply(FontColorChatSystem.Yellow($"Added reward {FontColorChatSystem.White($"{name}")} to the system"));

        }

        [Command("list", usage: "", description: "List of rewards available", adminOnly: true)]
        public static void ListReward(ChatCommandContext ctx)
        {
            if (Plugin.WalletSystem.Value)
            {
                throw ctx.Error("BloodyWallet is activated as a reward so this command is disabled.");
            }

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
            if (Plugin.WalletSystem.Value)
            {
                throw ctx.Error("BloodyWallet is activated as a reward so this command is disabled.");
            }
            try
            {
                if (ShareDB.rewardsList.Count == 1)
                {
                    throw ctx.Error(FontColorChatSystem.Yellow($"Do not remove all reward."));
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

        [Command("force start connection timer", shortHand: "", usage: "", description: "Start the connection time rewards timer", adminOnly: true)]
        public static void RestartReward(ChatCommandContext ctx)
        {
            
            try
            {
                if(forceRestartTimerConnection)
                {
                    ConnectionTimeSystemRewards.UserRewardTimne();
                    forceRestartTimerConnection = false;
                    ctx.Reply(FontColorChatSystem.White($"Successfully started connection time reward system."));
                } else
                {
                    forceRestartTimerConnection = true;
                    ctx.Reply(FontColorChatSystem.Red($"You are going to force a restart of the connection time reward system." +
                        $" If it was not stopped before, this could cause a double reward for connection time. To continue, run the command " +
                        FontColorChatSystem.Yellow("'.brw force start connection timer' again.")));
                }
                
                
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"Error: {error.Message}");
                throw ctx.Error($"Error: {error.Message}");
            }

        }
    }
}
