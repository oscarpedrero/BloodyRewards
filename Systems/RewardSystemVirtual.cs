using BloodyRewards.DB;
using BloodyRewards.DB.Models;
using ProjectM;
using System;
using Unity.Collections;
using Unity.Entities;
using Bloody.Core;
using Bloody.Core.Methods;
using Bloody.Core.Models.v1;
using Bloody.Core.Helper.v1;
using System.Linq;
using Stunlock.Core;
using BloodyWallet.API;
using Bloody.Core.GameData.v1;

namespace BloodyRewards.Systems
{
    public class RewardSystemVirtual
    {
        private static EntityManager em = Plugin.SystemsCore.EntityManager;

        private static Random rnd = new Random();

        private static PrefabGUID vBloodType = Prefabs.BloodType_VBlood;

        public static void ServerEvents_OnDeath(DeathEventListenerSystem sender, NativeArray<DeathEvent> deathEvents)
        {
            if (!ConfigDB.RewardsEnabled) return;
            if (!ConfigDB.WalletSystem) return;

                foreach (var deathEvent in deathEvents)
            {
                if (em.HasComponent<PlayerCharacter>(deathEvent.Killer) && em.HasComponent<Movement>(deathEvent.Died))
                {
                    bool isNPC = em.HasComponent<UnitLevel>(deathEvent.Died);
                    if (isNPC)
                    {
                        pveReward(deathEvent.Killer, deathEvent.Died);
                    }
                }
            }
        }

        public static void ServerEvents_OnVampireDowned(VampireDownedServerEventSystem sender, NativeArray<Entity> vampireDownedEntitys)
        {
            if (!ConfigDB.RewardsEnabled) return;

            foreach (var entity in vampireDownedEntitys)
            {
                VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, em, out var Died);
                Entity Source = em.GetComponentData<VampireDownedBuff>(entity).Source;
                VampireDownedServerEventSystem.TryFindRootOwner(Source, 1, em, out var Killer);

                if (em.HasComponent<PlayerCharacter>(Killer) && em.HasComponent<PlayerCharacter>(Died) && !Killer.Equals(Died))
                {
                    pvpReward(Killer, Died);
                }
            }
        }

        private static void pveReward(Entity killer, Entity died)
        {
            if (em.HasComponent<Minion>(died)) return;

            var playerCharacterKiller = em.GetComponentData<PlayerCharacter>(killer);
            var userModelKiller = GameData.Users.FromEntity(playerCharacterKiller.UserEntity);

            //Plugin.Logger.LogInfo($"PVE DROP");

            UnitLevel UnitDiedLevel = em.GetComponentData<UnitLevel>(died);


            var diedLevel = UnitDiedLevel.Level;

           // Plugin.Logger.LogInfo($"NPC Level {diedLevel}");

            bool isVBlood;
            if (em.HasComponent<BloodConsumeSource>(died))
            {
                BloodConsumeSource BloodSource = em.GetComponentData<BloodConsumeSource>(died);
                isVBlood = BloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            if (isVBlood)
            {
                rewardForVBlood(userModelKiller, diedLevel);
            } else
            {
                rewardForNPC(userModelKiller, diedLevel);
            }

        }

        private static void pvpReward(Entity killer, Entity died)
        {

            if (em.HasComponent<Minion>(died)) return;

            var playerCharacterKiller = em.GetComponentData<PlayerCharacter>(killer);
            var userModelKiller = GameData.Users.FromEntity(playerCharacterKiller.UserEntity);

            var rewards = ShareDB.getRewardList().ToList();
            var random = new Random();
            int indexRewards = random.Next(rewards.Count);

            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);

            //Plugin.Logger.LogInfo($"PVP DROP");
            
            var playerCharacterDied = em.GetComponentData<PlayerCharacter>(died);
            var userModelDied = GameData.Users.FromEntity(playerCharacterKiller.UserEntity);
            var diedLevel = userModelDied.Character.Equipment.Level;

            //Plugin.Logger.LogInfo($"User Died Level {diedLevel}");

            var percentFinal = calculateDropPercentage((int) diedLevel, ConfigDB.DropPvpPercentage, ConfigDB.IncrementPercentageDropEveryTenLevelsPvp);
            if (probabilityOeneratingReward(percentFinal))
            {
                var totalRewards = 0;
                totalRewards = rnd.Next(ConfigDB.WalletAmountPVPMin, ConfigDB.WalletAmountPVPMax);
                
                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName, out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountPvp + totalRewards;
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerPvp)
                    {
                        userRewardsPerDay.AmountPvp = virtualAmount;
                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop PVP {totalRewards} rewards");
                        return;
                    }
                    else if (userRewardsPerDay.AmountNpc < ConfigDB.MaxRewardsPerDayPerPlayerPvp)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerPvp - userRewardsPerDay.AmountPvp;
                        userRewardsPerDay.AmountPvp += totalRewards;
                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                       
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop PVP {totalRewards} rewards");
                        return;
                    }
                }
            }


        }

        private static void rewardForNPC(UserModel userModelKiller, int diedLevel)
        {
            var rewards = ShareDB.getRewardList().Where(x => x.onlyVBlood == false).ToList();
            var random = new Random();
            int indexRewards = random.Next(rewards.Count);

            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);

            var percentFinal = calculateDropPercentage(diedLevel, ConfigDB.DropNpcPercentage, ConfigDB.IncrementPercentageDropEveryTenLevelsNpc);
            if (probabilityOeneratingReward(percentFinal))
            {
                var totalRewards = 0;
                totalRewards = rnd.Next(ConfigDB.WalletAmountPveMin, ConfigDB.WalletAmountPveMax);
                
                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName,out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountNpc + totalRewards;
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerNpc)
                    {
                        userRewardsPerDay.AmountNpc = virtualAmount;
                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    } else if (userRewardsPerDay.AmountNpc < ConfigDB.MaxRewardsPerDayPerPlayerNpc)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerNpc - userRewardsPerDay.AmountNpc;
                        userRewardsPerDay.AmountNpc += totalRewards;
                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                       // Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    }
                }
                    
                
            }
        }

        private static void rewardForVBlood(UserModel userModelKiller, int diedLevel)
        {
            var rewards = ShareDB.getRewardList().Where(x => x.onlyVBlood == true ).ToList();
            var random = new Random();
            int indexRewards = random.Next(rewards.Count);

            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);

            var percentFinal = calculateDropPercentage(diedLevel, ConfigDB.DropdVBloodPercentage, ConfigDB.IncrementPercentageDropEveryTenLevelsVBlood);
            if (probabilityOeneratingReward(percentFinal))
            {
                var totalRewards = 0;
                totalRewards = rnd.Next(ConfigDB.WalletAmountVBloodMin, ConfigDB.WalletAmountVBloodMax);
               
                
                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName, out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountVBlood + totalRewards;
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerVBlood)
                    {
                        userRewardsPerDay.AmountVBlood = virtualAmount;

                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                        

                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    }
                    else if (userRewardsPerDay.AmountVBlood < ConfigDB.MaxRewardsPerDayPerPlayerVBlood)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerVBlood - userRewardsPerDay.AmountVBlood;
                        userRewardsPerDay.AmountVBlood += totalRewards;

                        WalletAPI.AddTokenToUser(ConfigDB.WalletPassword, totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);

                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    }
                }
            }
        }

        private static int calculateDropPercentage(int level, int initialPercent, int incremental)
        {
            decimal tensDecimal = level / 10;
            decimal tens = Math.Ceiling(tensDecimal);

            return Decimal.ToInt32(tens * incremental) + initialPercent;
        }

        private static bool probabilityOeneratingReward(int percentage)
        {
            var number = new Random().Next(1, 100);

            if (number <= (percentage * 1))
            {
                return true;
            }

            return false;
        }
    }
}
