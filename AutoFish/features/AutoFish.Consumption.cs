using System.Text;
using Terraria.ID;
using TShockAPI;

namespace AutoFish;

public partial class Plugin
{
    private static bool CanConsumeFish(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        if (playerData.CanConsume())
        {
            return true;
        }
        if (ConsumeBaitAndEnableMode(player, playerData))
        {
            return true;
        }
        ExitTip(player, playerData);
        return false;
    }

    /// <summary>
    ///     Consume bait and enable consumption mode.
    /// </summary>
    private static bool ConsumeBaitAndEnableMode(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        if (Configuration.Instance.BaitRewards.Count == 0)
        {
            return false;
        }
        var availableBaits = new List<(int itemId, int slot, int stack, Configuration.BaitReward reward)>();
        for (var i = 0; i < player.TPlayer.inventory.Length; i++)
        {
            var slot = player.TPlayer.inventory[i];
            if (slot.type == player.TPlayer.inventory[player.TPlayer.selectedItem].type)
            {
                continue;
            }

            if (Configuration.Instance.BaitRewards.TryGetValue(slot.type, out var reward))
            {
                if (slot.stack >= reward.Count)
                {
                    availableBaits.Add((slot.type, i, slot.stack, reward));
                }
            }
        }

        if (availableBaits.Count == 0)
        {
            return false;
        }

        var bestBait = availableBaits.OrderByDescending(b => b.reward.Minutes).First();
        var consumedCount = bestBait.reward.Count;
        var rewardMinutes = bestBait.reward.Minutes;

        var inventorySlot = player.TPlayer.inventory[bestBait.slot];
        inventorySlot.stack -= consumedCount;

        if (inventorySlot.stack < 1)
        {
            inventorySlot.TurnToAir();
        }

        player.SendData(PacketTypes.PlayerSlot, "", player.Index, PlayerItemSlotID.Inventory0 + bestBait.slot);
        playerData.ConsumeOverTime = DateTime.Now.AddMinutes(rewardMinutes);
        player.SendMessage(GetString($"Player [c/46C2D4:{player.Name}] enabled [c/F5F251:Auto Fishing] using bait: [i:{bestBait.itemId}x{consumedCount}]"), 247, 244, 150);

        if (playerData.ConsumeStartTime == default)
        {
            playerData.ConsumeStartTime = DateTime.Now;
        }

        return true;
    }

    /// <summary>
    ///     Detect timeout in consumption mode and disable auto fishing.
    /// </summary>
    private static void ExitTip(TSPlayer player, AFPlayerData.ItemData playerData)
    {
        if (playerData.ConsumeStartTime == default)
        {
            return;
        }

        var expiredMessage = new StringBuilder();
        expiredMessage.AppendLine(GetString("[i:3455][c/AD89D5:A][c/D68ACA:u][c/DF909A:t][c/E5A894:o][i:3454]"));
        var timeElapsed = DateTime.Now - playerData.ConsumeStartTime;
        var minutes = (int) timeElapsed.TotalMinutes;
        var seconds = timeElapsed.Seconds;
        playerData.ConsumeStartTime = default;

        expiredMessage.AppendLine(GetString("The following player has lost [c/76D5B4:auto fishing] permission:"));
        expiredMessage.AppendFormat(GetString("[c/A7DDF0:{0}]:[c/74F3C9:{1}m {2}s]"), playerData.Name, minutes, seconds);

        player.SendMessage(expiredMessage.ToString(), 247, 244, 150);
    }
}