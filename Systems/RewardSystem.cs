using BloodyRewards.DB;
using BloodyRewards.DB.Models;
using ProjectM;
using System;
using Unity.Collections;
using Unity.Entities;
using Bloody.Core.Methods;
using Bloody.Core.Models.v1;
using Bloody.Core.Helper.v1;
using System.Linq;
using Stunlock.Core;
using Bloody.Core.GameData.v1;
using Bloody.Core;

namespace BloodyRewards.Systems
{
    public class RewardSystem
    {
        private static EntityManager em = Plugin.SystemsCore.EntityManager;

        private static Random rnd = new Random();

        private static PrefabGUID vBloodType = Prefabs.BloodType_VBlood;

        public static void ServerEvents_OnDeath(DeathEventListenerSystem sender, NativeArray<DeathEvent> deathEvents)
        {
            if (ConfigDB.WalletSystem) return;

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
            if (ConfigDB.WalletSystem) return;

            foreach (var entity in vampireDownedEntitys)
            {
                ProcessVampireDowned(entity);
            }
        }

        private static void ProcessVampireDowned(Entity entity)
        {

            if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, Plugin.SystemsCore.EntityManager, out var victimEntity))
            {
                Plugin.Logger.LogMessage("Couldn't get victim entity");
                return;
            }

            var downBuff = entity.Read<VampireDownedBuff>();


            if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, Plugin.SystemsCore.EntityManager, out var killerEntity))
            {
                Plugin.Logger.LogMessage("Couldn't get victim entity");
                return;
            }

            var victim = victimEntity.Read<PlayerCharacter>();

            Plugin.Logger.LogMessage($"{victim.Name} is victim");
            var unitKiller = killerEntity.Has<UnitLevel>();

            if (unitKiller)
            {
                Plugin.Logger.LogInfo($"{victim.Name} was killed by a unit. [He is currently not receiving a reward]");
                return;
            }

            var playerKiller = killerEntity.Has<PlayerCharacter>();

            if (!playerKiller)
            {
                Plugin.Logger.LogWarning($"Killer could not be identified for {victim.Name}, if you know how to reproduce this please contact Trodi on discord or report on github");
                return;
            }

            var killer = killerEntity.Read<PlayerCharacter>();

            if (killer.UserEntity == victim.UserEntity)
            {
                Plugin.Logger.LogInfo($"{victim.Name} killed themselves. [He is currently not receiving a reward]");
                return;
            }

            pvpReward(killerEntity, victimEntity);

        }

        private static void pveReward(Entity killer, Entity died)
        {
            if (em.HasComponent<Minion>(died)) return;

            var playerCharacterKiller = em.GetComponentData<PlayerCharacter>(killer);
            var userModelKiller = GameData.Users.FromEntity(playerCharacterKiller.UserEntity);

            UnitLevel UnitDiedLevel = em.GetComponentData<UnitLevel>(died);

            var diedLevel = UnitDiedLevel.Level;

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
                totalRewards = rnd.Next(ConfigDB.DropPvpRewardsMin, ConfigDB.DropPvpRewardsMax);

                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName, out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountPvp + totalRewards;
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerPvp)
                    {
                        userRewardsPerDay.AmountPvp = virtualAmount;
                        if (!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }

                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop PVP {totalRewards} rewards");
                        return;
                    }
                    else if (userRewardsPerDay.AmountNpc < ConfigDB.MaxRewardsPerDayPerPlayerPvp)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerPvp - userRewardsPerDay.AmountPvp;
                        userRewardsPerDay.AmountPvp += totalRewards;
                        if (!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }
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
            if (rewards.Count == 0)
            {
                rewards = ShareDB.getRewardList();
            }
            var random = new Random();
            int indexRewards = random.Next(rewards.Count);

            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);

            var percentFinal = calculateDropPercentage(diedLevel, ConfigDB.DropNpcPercentage, ConfigDB.IncrementPercentageDropEveryTenLevelsNpc);
            if (probabilityOeneratingReward(percentFinal))
            {
                var totalRewards = 0;
                totalRewards = rnd.Next(ConfigDB.DropdNpcRewardsMin, ConfigDB.DropNpcRewardsMax);
                
                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName,out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountNpc + totalRewards;
                    
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerNpc)
                    {
                        
                        userRewardsPerDay.AmountNpc = virtualAmount;
                        if(!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }

                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    } else if (userRewardsPerDay.AmountNpc < ConfigDB.MaxRewardsPerDayPerPlayerNpc)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerNpc - userRewardsPerDay.AmountNpc;
                        userRewardsPerDay.AmountNpc += totalRewards;

                        if (!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }
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
            if(rewards.Count == 0 )
            {
                rewards = ShareDB.getRewardList();
            }
            var random = new Random();
            int indexRewards = random.Next(rewards.Count);

            var prefabRewardGUID = new PrefabGUID(rewards[indexRewards].guid);

            var percentFinal = calculateDropPercentage(diedLevel, ConfigDB.DropdVBloodPercentage, ConfigDB.IncrementPercentageDropEveryTenLevelsVBlood);
            if (probabilityOeneratingReward(percentFinal))
            {
                var totalRewards = 0;
                totalRewards = rnd.Next(ConfigDB.DropVBloodRewardsMin, ConfigDB.DropVBloodRewardsMax);
                
                if (ConfigDB.searchUserRewardPerDay(userModelKiller.CharacterName, out UserRewardsPerDayModel userRewardsPerDay))
                {
                    var virtualAmount = userRewardsPerDay.AmountVBlood + totalRewards;
                    if (virtualAmount <= ConfigDB.MaxRewardsPerDayPerPlayerVBlood)
                    {
                        userRewardsPerDay.AmountVBlood = virtualAmount;
                        if (!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }
                        ConfigDB.addUserRewardsPerDayToList(userRewardsPerDay);
                        SaveDataToFiles.saveUsersRewardsPerDay();
                        //Plugin.Logger.LogInfo($"Drop NPC {totalRewards} rewards");
                        return;
                    }
                    else if (userRewardsPerDay.AmountVBlood < ConfigDB.MaxRewardsPerDayPerPlayerVBlood)
                    {
                        totalRewards = ConfigDB.MaxRewardsPerDayPerPlayerVBlood - userRewardsPerDay.AmountVBlood;
                        userRewardsPerDay.AmountVBlood += totalRewards;
                        if (!userModelKiller.TryGiveItem(prefabRewardGUID, totalRewards, out Entity ItemEntity))
                        {
                            userModelKiller.DropItemNearby(prefabRewardGUID, totalRewards);
                        }
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
