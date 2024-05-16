using System;

namespace BloodyRewards.DB.Models
{
    [Serializable]
    public class RewardModel
    {
        public bool onlyVBlood { get; set; }

        public int id { get; set; }
        public string name { get; set; }
        public int guid { get; set; }

    }
}
