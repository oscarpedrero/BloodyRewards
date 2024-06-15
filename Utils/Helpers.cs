using BloodyRewards.DB.Models;
using BloodyRewards.DB;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Bloody.Core;
using BloodyRewards.Systems;

namespace BloodyRewards.Utils
{
    internal class Helpers
    {
        internal static void ProcessVampireDowned(Entity entity)
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

            PvpModel pvpModel = new PvpModel();
            pvpModel.killer = killer.Name.Value;
            pvpModel.target = victim.Name.Value;
            pvpModel.date = DateTime.Now;

            var killerSamePlayer = ShareDB.KillersPVP.Where(x => x.target == victim.Name.Value && x.killer == killer.Name.Value && x.date.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd")).ToList();

            if (killerSamePlayer.Count == 0)
            {
                ShareDB.KillersPVP.Add(pvpModel);
                if (ConfigDB.WalletSystem)
                {
                    RewardSystemVirtual.pvpReward(killerEntity, victimEntity);
                } else
                {
                    RewardSystem.pvpReward(killerEntity, victimEntity);
                }
                SaveDataToFiles.saveKillerPVPList();

            }
            else if (killerSamePlayer.Count <= Plugin.MaximumDeathsSamePlayer.Value)
            {
                ShareDB.KillersPVP.Add(pvpModel);
                if (ConfigDB.WalletSystem)
                {
                    RewardSystemVirtual.pvpReward(killerEntity, victimEntity);
                }
                else
                {
                    RewardSystem.pvpReward(killerEntity, victimEntity);
                }
                SaveDataToFiles.saveKillerPVPList();

            }
            else
            {
                var killModel = killerSamePlayer.LastOrDefault();
                var minutesLast = (killModel.date - DateTime.Now).TotalMinutes;

                if (minutesLast >= Plugin.CoolDownDeathsSamePlayer.Value)
                {
                    foreach (var registers in killerSamePlayer)
                    {
                        ShareDB.KillersPVP.Remove(registers);
                    }
                    if (ConfigDB.WalletSystem)
                    {
                        RewardSystemVirtual.pvpReward(killerEntity, victimEntity);
                    }
                    else
                    {
                        RewardSystem.pvpReward(killerEntity, victimEntity);
                    }
                    ShareDB.KillersPVP.Add(pvpModel);
                    SaveDataToFiles.saveKillerPVPList();
                }
            }
        }
    }
}
