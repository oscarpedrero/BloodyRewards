# BloodyRewards

Rewards System for VRising

## NEW IN 1.0.5

- You can now earn virtual currency rewards through [BloodyWallet](https://thunderstore.io/c/v-rising/p/Trodi/BloodyWallet/) mod. Check the configuration file to activate this option.
- Now you can activate rewards that will be awarded once per day through daily login to the server.  Check the configuration file to activate this option.
- Now you can activate rewards that will be awarded for connection time, the longer people are logged in, the more rewards they will get.  Check the configuration file to activate this option.

<details>
<summary>Changelog</summary>

`1.0.5`
- Updated to work with [BloodyWallet](https://thunderstore.io/c/v-rising/p/Trodi/BloodyWallet/)
- Added daily login reward system
- Added reward system for connection time

`1.0.4`
- Bug fixes and performance improvements

`1.0.3`
- Bloody.Core dependency removed as dll and added as framework

`1.0.0`
- First Release


</details>

# Sponsor this project

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/K3K8ENRQY)

# Requirements

1. [BepInEx](https://thunderstore.io/c/v-rising/p/BepInEx/BepInExPack_V_Rising/)
2. [Bloodstone](https://thunderstore.io/c/v-rising/p/deca/Bloodstone/)
3. [VampireCommandFramework](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/)
4. [Bloody.Core](https://thunderstore.io/c/v-rising/p/Trodi/BloodyCore/)
5. [BloodyWallet](https://thunderstore.io/c/v-rising/p/Trodi/BloodyWallet/)

## Configuration

Once the mod is installed, it's time for configuration:

For this we will go to the folder that we have defined within BepInEx for the configuration files and there we will find the configuration file of the mod called `BloodyRewards.cfg` that we will explain:

```
## Settings file was created by plugin BloodyRewards v1.0.5
## Plugin GUID: BloodyRewards

[ConnectionTimeReward]

## Connection time reward
# Setting type: Boolean
# Default value: true
ConnectionTimeReward = true

## Amount of rewards for connection time
# Setting type: Int32
# Default value: 1
AmountTimeReward = 1

## Every how many minutes the reward will be awarded
# Setting type: Int32
# Default value: 5
TimeReward = 2

[DayliLoginRewardsSystem]

## Daily reward for connecting to the server
# Setting type: Boolean
# Default value: true
DailyLoginRewards = true

## Amount of rewards for login
# Setting type: Int32
# Default value: 15
AmountDailyLoginReward = 15

[RewardsSystem]

## Enable Rewards System
# Setting type: Boolean
# Default value: true
enabled = true

## Percent chance that an NPC will drop the type of reward from the shop
# Setting type: Int32
# Default value: 5
minPercentageDropNpc = 5

## Percentage increase for every rank of 10 levels of the NPC
# Setting type: Int32
# Default value: 5
IncrementPercentageDropEveryTenLevelsNpc = 5

## Minimum reward an NPC can drop
# Setting type: Int32
# Default value: 1
DropdNpcRewardsMin = 1

## Maximum reward an NPC can drop
# Setting type: Int32
# Default value: 5
DropNpcRewardsMax = 5

## Maximum number of reward that a user can get per day by NPC death
# Setting type: Int32
# Default value: 5
MaxRewardsPerDayPerPlayerNpc = 5

## Percent chance that an VBlood will drop the type of reward from the shop
# Setting type: Int32
# Default value: 20
minPercentageDropVBlood = 20

## Percentage increase for every rank of 10 levels of the VBlood
# Setting type: Int32
# Default value: 5
IncrementPercentageDropEveryTenLevelsVBlood = 5

## Minimum reward an VBlood can drop
# Setting type: Int32
# Default value: 10
DropVBloodRewardsMin = 10

## Maximum reward an VBlood can drop
# Setting type: Int32
# Default value: 20
DropVBloodRewardsMax = 20

## Maximum number of reward that a user can get per day by VBlood death
# Setting type: Int32
# Default value: 20
MaxRewardsPerDayPerPlayerVBlood = 20

## Percent chance that victory in a PVP duel will drop the type of reward in the store
# Setting type: Int32
# Default value: 100
minPercentageDropPvp = 100

## Percentage increase for every rank of 10 levels of the Player killed in pvp duel
# Setting type: Int32
# Default value: 5
IncrementPercentageDropEveryTenLevelsPvp = 5

## Minimum reward can drop victory in PVP
# Setting type: Int32
# Default value: 15
DropPvpRewardsMin = 15

## Maximum reward can drop victory in PVP
# Setting type: Int32
# Default value: 20
DropPvpRewardsMax = 20

## Maximum number of reward that a user can get per day by victory in PVP
# Setting type: Int32
# Default value: 20
MaxRewardsPerDayPerPlayerPvp = 20

[Wallet]

## Activate rewards in virtual currency through BloodyWallet ( https://github.com/oscarpedrero/BloodyWallet )
# Setting type: Boolean
# Default value: false
enabled = true

## Maximum amount of virtual coins for when you drop in PVE
# Setting type: Int32
# Default value: 2
amountPveMax = 2

## Minumun amount of virtual coins for when you drop in PVE
# Setting type: Int32
# Default value: 1
amountPveMin = 1

## Maximum amount of virtual coins for when you drop in VBlood
# Setting type: Int32
# Default value: 2
amountVBloodMax = 2

## Minimun amount of virtual coins for when you drop in PVE
# Setting type: Int32
# Default value: 1
amountVBloodMin = 1

## Maximum amount of virtual coins for when you drop in PVE
# Setting type: Int32
# Default value: 2
amountPVPMax = 2

## Minimun amount of virtual coins for when you drop in PVE
# Setting type: Int32
# Default value: 1
amountPVPMin = 1
```

|SECTION|PARAM| DESCRIPTION                                                     | DEFAULT
|----------------|-------------------------------|-----------------------------------------------------------------|-----------------------------|
|RewardsSystem|`enabled `            | Define if the system enabled or disabled | true
|RewardsSystem|`minPercentageDropNpc`            | Percent chance that an NPC will drop the type of reward | 5
|RewardsSystem|`IncrementPercentageDropEveryTenLevelsNpc`            |  Percentage increase for every rank of 10 levels of the NPC| 5
|RewardsSystem|`DropdNpcRewardsMin`            |  Minimum reward an NPC can drop| 5
|RewardsSystem|`DropNpcRewardsMax`            |  Maximum reward an NPC can drop| 5
|RewardsSystem|`MaxRewardsPerDayPerPlayerNpc`            |  Maximum number of reward that a user can get per day by NPC death| 5
|RewardsSystem|`minPercentageDropVBlood`            |  Percent chance that an VBlood will drop the type of reward| 20
|RewardsSystem|`IncrementPercentageDropEveryTenLevelsVBlood`            |  Percentage increase for every rank of 10 levels of the VBlood| 1
|RewardsSystem|`DropVBloodRewardsMin`            |  Minimum reward an VBlood can drop| 10
|RewardsSystem|`DropVBloodRewardsMax`            |  Maximum reward an VBlood can drop| 20
|RewardsSystem|`MaxRewardsPerDayPerPlayerVBlood`            |  Maximum number of reward that a user can get per day by VBlood death| 20
|RewardsSystem|`minPercentageDropPvp`            |  Percent chance that victory in a PVP duel will drop the type of reward in the store| 100
|RewardsSystem|`IncrementPercentageDropEveryTenLevelsPvp`            |  Percentage increase for every rank of 10 levels of the Player killed in pvp duel| 5
|RewardsSystem|`DropPvpRewardsMin`            |  Minimum reward can drop victory in PVP| 15
|RewardsSystem|`DropPvpRewardsMax`            |  Maximum reward can drop victory in PVP| 20
|RewardsSystem|`MaxRewardsPerDayPerPlayerPvp`            |  Maximum number of reward that a user can get per day by victory in PVP| 20

## Rewards System

This system can be activated or not depending on whether the material we want to use as reward drops very frequently or not.

The system works in such a way that we will define a percentage of opportunity for the rewards to drop each time the following events occur:

- Death of an NPC by a player
- Death of a VBlood by a player
- Every time someone wins a PVP duel

For this, we have included several parameters that are configured through the mod configuration file on the server. For each of those three events there are several values ​​that we can configure:

- Percentage of chance of falling when the event jumps
- Percentage of drop chance increases every 10 levels.
- Minimum number of rewards that can drop in each event
- Maximum number of rewards that can drop in each event
- Maximum number of rewards a player can get through this drop system.

Let's take an example of an NPC drop system:

- We want to have a 10% probability that an NPC, when killed, drops a number of rewards of a minimum of 1 and a maximum of 5
- And that the increase every 10 levels of that NPC is 2%
- With a maximum of 40 coins per day for each player.

- To make this type of configuration we would have to put the following parameters in the server file:

```
## Percent chance that an NPC will drop the type of reward from the system
# Setting type: Int32
#Default value: 5
minPercentageDropNpc = 10

## Percentage increase for every rank of 10 levels of the NPC
# Setting type: Int32
#Default value: 5
IncrementPercentageDropEveryTenLevelsNpc = 2

## Minimum reward an NPC can drop
# Setting type: Int32
#Default value: 1
DropdNpcRewardsMin = 1

## Maximum reward an NPC can drop
# Setting type: Int32
#Default value: 5
DropNpcRewardsMax = 5

## Maximum number of reward that a user can get per day by NPC death
# Setting type: Int32
#Default value: 5
MaxRewardsPerDayPerPlayerNpc = 40
```

This is what happened in the different cases of the rewars system on the death of an NPC:

- If the NPC is level from 1 to 10, his percentage chance of having a drop of coins from the NPC is 10% with a maximum of 1 coin and a maximum of 5.
- If the NPC is level from 11 to 20, his percentage chance of having a drop of coins from the NPC is 12% with a maximum of 1 coin and a maximum of 5.
- If the NPC is level from 21 to 30, his percentage chance of having a drop of coins from the NPC is 14% with a maximum of 1 coin and a maximum of 5.
- If the NPC is level 31 to 40, his percentage chance of having a drop of coins from the NPC is 16% with a maximum of 1 coin and a maximum of 5.
- ...      And so we would continue up to the level that is indicated for each NPC until we reach the limit per player per day, which in that case would not have any chance of getting drops from the store.

This same configuration can be customized for the three drop events that we mentioned above.

## Admin commands

There are currently four commands for admins:

| COMMAND                                          |DESCRIPTION
|--------------------------------------------------|-------------------------------|
| `.brw add "<NameOfReward" <PrefabGUID> <OnlyForVBlood (true/false)>`   | Command to add an reward. To get the PrefabGUID you must visit [Complete list of prefabs](https://discord.com/channels/978094827830915092/1117273637024714862/1117273642817044571) and get the PrefabGUID Value as shown in the image below. The last parameter is used to indicate to the drop system if you want to activate this reward so that it drops when killing an NPC Example: .brw add "Silver Coin" -949672483 true 
| `.brw list` | Command to get the list of rewards.
| `.brw remove <NumberItem>` | Command to remove an reward.

# Credits

[V Rising Mod Community](https://discord.gg/vrisingmods) the best community of mods for V Rising

@Godwin for all the ideas you have brought to the project.

[@Deca](https://github.com/decaprime) for your help and the wonderful framework [VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework) and [BloodStone](https://github.com/decaprime/Bloodstone) based in [WetStone](https://github.com/molenzwiebel/Wetstone) by [@Molenzwiebel](https://github.com/molenzwiebel)

[@Adain](https://github.com/adainrivers) for encouraging me to develop a UI to be able to use the mod from the client, for the support and for its [VRising.GameData](https://github.com/adainrivers/VRising.GameData) framework

[@Paps](https://github.com/phillipsOG) for all the help and encouragement he has given us to get this idea off the ground.

**A special thanks to the testers and supporters of the project:**

- @Vex [Vexor Gaming](https://discord.gg/rxaTBzjuMc) as a tester and great supporter, who provided his server as a test platform!
