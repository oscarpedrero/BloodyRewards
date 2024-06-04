using BloodyRewards.DB.Models;
using System;
using System.Collections.Generic;

namespace BloodyRewards.DB
{
    public class ConfigDB
    {
        public static bool RewardsEnabled { get; set; } = true;
        public static int DropNpcPercentage { get; set; } = 0;
        public static int IncrementPercentageDropEveryTenLevelsNpc { get; set; } = 0;
        public static int DropdNpcRewardsMin { get; set; } = 0;
        public static int DropNpcRewardsMax { get; set; } = 0;
        public static int MaxRewardsPerDayPerPlayerNpc { get; set; } = 0;
        public static int DropdVBloodPercentage { get; set; } = 0;
        public static int IncrementPercentageDropEveryTenLevelsVBlood { get; set; } = 0;
        public static int DropVBloodRewardsMin { get; set; } = 0;
        public static int DropVBloodRewardsMax { get; set; } = 0;
        public static int MaxRewardsPerDayPerPlayerVBlood { get; set; } = 0;
        public static int DropPvpPercentage { get; set; } = 0;
        public static int IncrementPercentageDropEveryTenLevelsPvp { get; set; } = 0;
        public static int DropPvpRewardsMin { get; set; } = 0;
        public static int DropPvpRewardsMax { get; set; } = 0;
        public static int MaxRewardsPerDayPerPlayerPvp { get; set; } = 0;
        public static List<UserRewardsPerDayModel> UsersRewardsPerDay { get; set; } = new List<UserRewardsPerDayModel>();
        public static bool WalletSystem { get; internal set; }
        public static int WalletAmountPveMax { get; internal set; }
        public static int WalletAmountPveMin { get; internal set; }
        public static int WalletAmountPVPMin { get; internal set; }
        public static int WalletAmountPVPMax { get; internal set; }
        public static int WalletAmountVBloodMin { get; internal set; }
        public static int WalletAmountVBloodMax { get; internal set; }
        public static bool DailyLoginRewards { get; internal set; }
        public static int AmountDailyLoginReward { get; internal set; }
        public static bool ConnectionTimeReward { get; internal set; }
        public static int AmountTimeReward { get; internal set; }
        public static int TimeReward { get; internal set; }

        public static List<(string name, DateTime date, UserRewardsPerDayModel model)> _normalizedUsersRewardsPerDay = new();

        public static bool setUsersRewardsPerDay(List<UserRewardsPerDayModel> listUsersRewardsPerDay)
        {

            UsersRewardsPerDay = new();

            foreach (UserRewardsPerDayModel userRewardsPerDay in listUsersRewardsPerDay)
            {
                DateTime oDate = DateTime.Parse(userRewardsPerDay.date);
                if (oDate != DateTime.Today)
                {
                    continue;
                }
                _normalizedUsersRewardsPerDay.Add((userRewardsPerDay.CharacterName, oDate, userRewardsPerDay));
                UsersRewardsPerDay.Add(userRewardsPerDay);
            }

            return true;
        }

        public static void addUserRewardsPerDayToList(UserRewardsPerDayModel userRewardsPerDay)
        {
            foreach (var (name, date, model) in _normalizedUsersRewardsPerDay)
            {
                if (name == userRewardsPerDay.CharacterName)
                {
                    UsersRewardsPerDay.Remove(model);
                    _normalizedUsersRewardsPerDay.Remove((name, date, model));
                    UsersRewardsPerDay.Add(userRewardsPerDay);
                    _normalizedUsersRewardsPerDay.Add((userRewardsPerDay.CharacterName, DateTime.Parse(userRewardsPerDay.date), userRewardsPerDay));
                    break;
                }
            }
        }

        public static bool searchUserRewardPerDay(string characterName, out UserRewardsPerDayModel userRewardsPerDayModel)
        {

            userRewardsPerDayModel = null;
            if (characterName == "")
            {
                return false;
            }

            var today = DateTime.Today;

            var todayString = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";

            foreach (var (name, date, model) in _normalizedUsersRewardsPerDay)
            {
                if (name == characterName)
                {
                    if(today == date)
                    {
                        userRewardsPerDayModel = model;
                        break;
                    } else
                    {
                        UsersRewardsPerDay.Remove(model);
                        _normalizedUsersRewardsPerDay.Remove((name, date, model));
                        userRewardsPerDayModel = new UserRewardsPerDayModel
                        {
                            CharacterName = model.CharacterName,
                            date = todayString
                        };
                        UsersRewardsPerDay.Add(model);
                        _normalizedUsersRewardsPerDay.Add((characterName, today, userRewardsPerDayModel));
                        break;
                    }
                }
            }

            if(userRewardsPerDayModel == null)
            {
                userRewardsPerDayModel = new UserRewardsPerDayModel
                {
                    CharacterName = characterName,
                    date = todayString
                };
                UsersRewardsPerDay.Add(userRewardsPerDayModel);
                _normalizedUsersRewardsPerDay.Add((characterName, today, userRewardsPerDayModel));
                return true;

            } else
            {
                return true;
            }
        }
    }
}
