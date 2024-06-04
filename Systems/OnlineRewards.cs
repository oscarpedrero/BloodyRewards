using Bloody.Core;
using Bloody.Core.API.v1;
using Bloody.Core.GameData.v1;
using Bloody.Core.Methods;
using Bloody.Core.Models.v1;
using BloodyRewards.DB;
using BloodyRewards.DB.Models;
using BloodyWallet.API;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BloodyRewards.Systems
{
    internal class OnlineRewards
    {

        internal static void OnlineUser(ServerBootstrapSystem sender, NetConnectionId netConnectionId)
        {

            if (!ConfigDB.DailyLoginRewards)
            {
                return;
            }

            var userIndex = sender._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = sender._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;

            UserModel userModel = GameData.Users.FromEntity(userEntity);

            bool isNewPlayer = IsNewUser(userEntity);
            var userNick = userModel.CharacterName;

            if (!isNewPlayer)
            {
                var DailyModelUser = ShareDB.getDayliLoginTimeModel().Where(x => x.player == userNick).FirstOrDefault();

                if(DailyModelUser != null )
                {
                    if (DailyModelUser.LastDayConnection != DateTime.Now.ToString("yyyy/MM/dd"))
                    {
                        DailyModelUser.LastDayConnection = DateTime.Now.ToString("yyyy/MM/dd");
                        if (ConfigDB.WalletSystem)
                        {
                            WalletAPI.AddTokenToUser(ConfigDB.AmountDailyLoginReward, "BloodyRewards", userModel.Entity, userModel.Entity, out string message);
                            userModel.SendSystemMessage(message);
                        }
                        else
                        {
                            var rewards = ShareDB.getRewardList().ToList();
                            var random = new Random();
                            int indexRewards = random.Next(rewards.Count);

                            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);
                            userModel.DropItemNearby(prefabRewardGUID, ConfigDB.AmountDailyLoginReward);
                        }

                        SaveDataToFiles.saveDayliTimeLogin();
                    }
                    
                } else
                {
                    if (ConfigDB.WalletSystem)
                    {
                        WalletAPI.AddTokenToUser(ConfigDB.AmountDailyLoginReward, "BloodyRewards", userModel.Entity, userModel.Entity, out string message);
                        userModel.SendSystemMessage(message);
                    }
                    else
                    {
                        var rewards = ShareDB.getRewardList().ToList();
                        var random = new Random();
                        int indexRewards = random.Next(rewards.Count);

                        var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);
                        userModel.DropItemNearby(prefabRewardGUID, ConfigDB.AmountDailyLoginReward);
                    }

                    DayliLoginTimeModel dayliModel = new();

                    dayliModel.player = userModel.CharacterName;
                    dayliModel.LastDateTime = DateTime.Now;
                    dayliModel.LastDayConnection = DateTime.Now.ToString("yyyy/MM/dd");

                    ShareDB.getDayliLoginTimeModel().Add(dayliModel);

                    SaveDataToFiles.saveDayliTimeLogin();
                }
                
            }
        }

        public static bool IsNewUser(Entity userEntity)
        {
            var userComponent = userEntity.Read<User>();
            return userComponent.CharacterName.IsEmpty;

        }


    }
}
