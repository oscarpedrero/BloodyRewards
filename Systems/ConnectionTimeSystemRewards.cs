using Bloody.Core.GameData.v1;
using Bloody.Core.Methods;
using Bloody.Core.Models.v1;
using BloodyRewards.DB;
using BloodyRewards.DB.Models;
using BloodyWallet.API;
using ProjectM;
using Stunlock.Core;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodyRewards.Systems
{
    internal class ConnectionTimeSystemRewards
    {
        internal static Action action;
        internal static void UserRewardTimne()
        {

            action = () =>
            {
                var users = GameData.Users.Online;
                DayliLoginTimeModel dayliModel = new DayliLoginTimeModel();
                var dateTime = DateTime.Now;
                foreach (var user in users)
                {
                    dayliModel = ShareDB.getDayliLoginTimeModel().Where(x => x.player == user.CharacterName).FirstOrDefault();

                    if (dayliModel == null)
                    {
                        dayliModel.player = user.CharacterName;
                        dayliModel.LastDateTime = dateTime;
                        dayliModel.LastDayConnection = dateTime.ToString("yyyy/MM/dd");
                        ShareDB.getDayliLoginTimeModel().Add(dayliModel);
                        SaveDataToFiles.saveDayliTimeLogin();
                        continue;
                    }

                    var diffInminutes = (dateTime - dayliModel.LastDateTime).TotalMinutes + 1;

                    if (diffInminutes >= ConfigDB.TimeReward)
                    {
                        dayliModel.LastDayConnection = dateTime.ToString("yyyy/MM/dd");
                        dayliModel.LastDateTime = dateTime;
                        if (ConfigDB.WalletSystem)
                        {
                            WalletAPI.AddTokenToUser(ConfigDB.AmountTimeReward, "BloodyRewards", user.Entity, user.Entity, out string message);
                            user.SendSystemMessage(message);
                        }
                        else
                        {
                            var rewards = ShareDB.getRewardList().ToList();
                            var random = new Random();
                            int indexRewards = random.Next(rewards.Count);

                            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);
                            user.DropItemNearby(prefabRewardGUID, ConfigDB.AmountTimeReward);
                        }

                        SaveDataToFiles.saveDayliTimeLogin();
                    }

                }
                Plugin.Logger.LogInfo($"Next checking at {ConfigDB.TimeReward * 60}");
                TimerSystem.RunActionOnceAfterDelay(action, ConfigDB.TimeReward * 60);
            };
            Plugin.Logger.LogInfo($"Next checking at {ConfigDB.TimeReward * 60}");
            TimerSystem.RunActionOnceAfterDelay(action, ConfigDB.TimeReward * 60);
        }
    }
}
