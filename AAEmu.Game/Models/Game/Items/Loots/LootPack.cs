using System;
using System.Collections.Generic;
using System.Linq;
using AAEmu.Commons.Utils;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Models.Game.Units;
using NLog;

namespace AAEmu.Game.Models.Game.Items.Loots;

/*
 * Original Authors: AAGene, spiral
 * Original Source: AAGenesis
 * Modified by: ZeromusXYZ
 */

public class LootPack
{
    private Logger _logger = LogManager.GetCurrentClassLogger();

    public uint Id { get; set; }

    public uint GroupCount { get; set; }

    public List<Loot> Loots { get; set; }
    public Dictionary<uint, LootGroups> Groups { get; set; }
    public Dictionary<uint, LootActabilityGroups> ActabilityGroups { get; set; }
    public Dictionary<uint, List<Loot>> LootsByGroupNo { get; set; }

    private List<(uint itemId, int count, byte grade)> _generatedPack;


    /// <summary>
    /// Generates the contents of a LootPack, in the form of a list of tuples. This list is stored internally
    /// </summary>
    /// <param name="player">Player who's loot multipliers need to be used</param>
    /// <returns></returns>
    public List<(uint itemId, int count, byte grade)> GeneratePack(Character player)
    {
        var lootDropRate = (100f + player.DropRateMul) / 100f;
        var lootGoldRate = (100f + player.LootGoldMul) / 100f;
        return GeneratePack(lootDropRate, lootGoldRate);
    }
    
    /// <summary>
    /// Generates the contents of a LootPack, in the form of a list of tuples. This list is stored internally
    /// </summary>
    /// <param name="lootDropRate">1.0f = 100%</param>
    /// <param name="lootGoldRate">1.0f = 100% applies to coins item only</param>
    /// <returns></returns>
    public List<(uint itemId, int count, byte grade)> GeneratePack(float lootDropRate, float lootGoldRate)
    {
        // Use 8000022 as an example

        var items = new List<(uint itemId, int count, byte grade)>();

        // For every group
        for (uint gIdx = 0; gIdx <= GroupCount; gIdx++)
        {
            var hasLootGroup = false;
            var lootGradeDistribId = 0u;

            if (!LootsByGroupNo.ContainsKey(gIdx))
                continue;
            
            // If that group has a LootGroup, roll the dice
            if (Groups.TryGetValue(gIdx, out var lootGroup))
            {
                hasLootGroup = true;
                lootGradeDistribId = lootGroup.ItemGradeDistributionId;
                var dice = (long)Rand.Next(0, 10000000);
                
                // Use generic loot multiplier for the groups ?
                dice = (long)Math.Floor(dice / (lootDropRate * AppConfiguration.Instance.World.LootRate));
                
                if (dice > lootGroup.DropRate)
                    continue;
            }

            // If that group has a LootActGroup, roll the dice
            if (ActabilityGroups.TryGetValue(gIdx, out var actabilityGroup))
            {
                var dice = (long)Rand.Next(0, 10000); 

                // Use generic loot multiplier for the ActGroups ?
                dice = (long)Math.Floor(dice / (lootDropRate * AppConfiguration.Instance.World.LootRate));
                
                if (dice > actabilityGroup.MaxDice)
                    continue;
            }

            var loots = LootsByGroupNo[gIdx];
            if (loots == null || loots.Count == 0)
                continue;

            var uniqueItemDrop = loots[0].DropRate == 1;
            var itemRoll = Rand.Next(0, 10000000);
            
            // Apply multiplier for loot drop rate
            itemRoll = (int)Math.Round(itemRoll / lootDropRate);
            
            var itemStackingRoll = 0u;

            List<Loot> selected = new List<Loot>();


            if (uniqueItemDrop || hasLootGroup || (GroupCount <= 1))
            {
                selected.Add(loots.RandomElementByWeight(l => l.DropRate));
            }
            else
            {

                selected.AddRange(loots.Where(loot => loot.AlwaysDrop || loot.DropRate == 10000000).ToList());

                foreach (var loot in loots.Where(loot => !(loot.AlwaysDrop || loot.DropRate == 10000000)))
                {
                    if (loot.DropRate + itemStackingRoll < itemRoll)
                    {
                        itemStackingRoll += loot.DropRate;
                        continue;
                    }

                    itemStackingRoll += loot.DropRate;

                    selected.Add(loot);
                    break;
                }
            }

            foreach (var selectedPack in selected)
            {
                var lootCount = Rand.Next(selectedPack.MinAmount, selectedPack.MaxAmount + 1);

                var grade = selectedPack.GradeId;
                if (lootGradeDistribId > 0)
                    grade = GetGradeFromDistribution(lootGradeDistribId);

                // Multiply gold as needed
                if (selectedPack.ItemId == Item.Coins)
                    lootCount = (int)Math.Round(lootCount * (lootGoldRate * AppConfiguration.Instance.World.GoldLootMultiplier));

                items.Add((selectedPack.ItemId, lootCount, grade));
            }
        }

        _generatedPack = items;
        return items;
    }

    public List<Item> GenerateNpcPackItems(ref ulong baseId, float lootDropRate = 1.0f, float lootGoldRate = 1.0f)
    {
        var packList = GeneratePack(lootDropRate, lootGoldRate);
        var itemList = packList
            .Select(tuple => ItemManager.Instance.Create(tuple.itemId, tuple.count, tuple.grade, false)).ToList();
        foreach (var item in itemList)
        {
            item.Id = ++baseId;
        }

        return itemList;
    }
    
    /// <summary>
    /// Gives a lootpack to the specified player. It is possible to pass in a pre-generated list if we wanted to do some extra checks on our player's inventory.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="taskType"></param>
    /// <param name="generatedList"></param>
    public void GiveLootPack(Character character, ItemTaskType taskType, List<(uint itemId, int count, byte grade)> generatedList = null)
    {
        // If it is not generated yet, generate loot pack info now
        if (generatedList == null)
            generatedList = GeneratePack(character);

        // Distribute the items (and coins)
        foreach (var tuple in generatedList)
        {
            if (tuple.itemId == 500)
            {
                _logger.Debug("{Category} - {Character} got {Amount} from lootpack {Lootpack}");
                character.AddMoney(SlotType.Inventory, tuple.count, taskType);
                continue;
            }

            if (!character.Inventory.Bag.AcquireDefaultItem(taskType, tuple.itemId, tuple.count, tuple.grade))
                _logger.Error($"Unable to give loot to {character.Name} - ItemId: {tuple.itemId} x {tuple.count} at grade {tuple.grade}");
        }
    }

    private byte GetGradeFromDistribution(uint id)
    {
        byte gradeId = 0;
        var distributions = ItemManager.Instance.GetGradeDistributions((byte)id);

        var array = new[]
        {
            distributions.Weight0, distributions.Weight1, distributions.Weight2, distributions.Weight3,
            distributions.Weight4, distributions.Weight5, distributions.Weight6, distributions.Weight7,
            distributions.Weight8, distributions.Weight9, distributions.Weight10, distributions.Weight11
        };

        var old = 0;
        var gradeDrop = Rand.Next(0, 100);
        for (byte i = 0; i <= 11; i++)
        {
            if (gradeDrop <= array[i] + old)
            {
                gradeId = i;
                i = 11;
            }
            else
            {
                old += array[i];
            }
        }

        return gradeId;
    }
}