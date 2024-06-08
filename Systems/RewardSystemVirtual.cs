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
using Bloodstone.API;

namespace BloodyRewards.Systems
{
    public class RewardSystemVirtual
    {
        private static EntityManager em = Plugin.SystemsCore.EntityManager;

        private static Random rnd = new Random();

        private static PrefabGUID vBloodType = Prefabs.BloodType_VBlood;

        public static void ServerEvents_OnDeath(DeathEventListenerSystem sender, NativeArray<DeathEvent> deathEvents)
        {
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

            Plugin.Logger.LogInfo("HELOOOOO 1");
            if (!ConfigDB.WalletSystem) return;

            foreach (var entity in vampireDownedEntitys)
            {
                Plugin.Logger.LogInfo("HELOOOOO 2");
                ProcessVampireDowned(entity);
            }
        }


        private static void ProcessVampireDowned(Entity entity)
        {
            Plugin.Logger.LogInfo("HELOOOOO 3");
            if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, VWorld.Server.EntityManager, out var victimEntity))
            {
                Plugin.Logger.LogInfo("Couldn't get victim entity");
                return;
            }

            var downBuff = entity.Read<VampireDownedBuff>();

            Plugin.Logger.LogInfo("HELOOOOO 4");
            if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity))
            {
                Plugin.Logger.LogMessage("Couldn't get victim entity");
                return;
            }
            Plugin.Logger.LogInfo("HELOOOOO 5");
            var victim = victimEntity.Read<PlayerCharacter>();

            Plugin.Logger.LogMessage($"{victim.Name} is victim");
            var unitKiller = killerEntity.Has<UnitLevel>();
            Plugin.Logger.LogInfo("HELOOOOO 6");
            if (unitKiller)
            {
                Plugin.Logger.LogInfo("HELOOOOO 7");
                Plugin.Logger.LogInfo($"{victim.Name} was killed by a unit. [He is currently not receiving a reward]");
                return;
            }
            Plugin.Logger.LogInfo("HELOOOOO 8");
            var playerKiller = killerEntity.Has<PlayerCharacter>();
            Plugin.Logger.LogInfo("HELOOOOO 3");
            if (!playerKiller)
            {
                Plugin.Logger.LogInfo("HELOOOOO 9");
                Plugin.Logger.LogWarning($"Killer could not be identified for {victim.Name}, if you know how to reproduce this please contact Trodi on discord or report on github");
                return;
            }
            Plugin.Logger.LogInfo("HELOOOOO 10");
            var killer = killerEntity.Read<PlayerCharacter>();

            if (killer.UserEntity == victim.UserEntity)
            {
                Plugin.Logger.LogInfo("HELOOOOO 11");
                Plugin.Logger.LogInfo($"{victim.Name} killed themselves. [He is currently not receiving a reward]");
                return;
            }
            Plugin.Logger.LogInfo("HELOOOOO 12");
            pvpReward(killerEntity, victimEntity);

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
            if (died.Has<VBloodUnit>())
            {
                isVBlood = true;
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
                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
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
                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
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
                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
                        userModelKiller.SendSystemMessage(message);
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    } else if (userRewardsPerDay.AmountNpc < ConfigDB.MaxRewardsPerDayPerPlayerNpc)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerNpc - userRewardsPerDay.AmountNpc;
                        userRewardsPerDay.AmountNpc += totalRewards;
                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
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

                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
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

                        WalletAPI.AddTokenToUser(totalRewards, "BloodyRewards", userModelKiller.Entity, userModelKiller.Entity, out string message);
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
