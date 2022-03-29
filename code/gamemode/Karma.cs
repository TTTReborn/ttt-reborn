using Sandbox;

namespace TTTReborn.Gamemode
{
    public partial class KarmaSystem
    {
        // [ServerVar("ttt_karma_default", Help = "The default amount of karma given to a player.")]
        // public static int TTTKarmaDefault { get; set; } = 1000;

        // [ServerVar("ttt_karma_max", Help = "The maximum amount of karma a player can achieve.")]
        // public static int TTTKarmaMax { get; set; } = 1250;

        // [ServerVar("ttt_karma_min", Help = "The minimum amount of karma a player can have.")]
        // public static int TTTKarmaMin { get; set; } = 500;

        // [ServerVar("ttt_karma_ban", Help = "Should the player be banned once they reach minimum karma?")]
        // public static bool TTTKarmaBan { get; set; } = true;

        // [ServerVar("ttt_karma_gain", Help = "The amount of passive karma gain every round.")]
        // public static int TTTKarmaGain { get; set; } = 50;

        // [ServerVar("ttt_karma_loss", Help = "The amount of karma loss per damage dealt.")]
        // public static float TTTKarmaLoss { get; set; } = 1f;

        // [ServerVar("ttt_karma_penalty_max", Help = "The maximum amount of karma loss per player.")]
        // public static int TTTKarmaPenaltyMax { get; set; } = 100;

        public void RegisterPlayer(Client client)
        {
            client.SetInt("karma", 1000);
        }

        public void RegisterPlayerDamage(Player attacker, Player victim, float damage)
        {
            //int updatedDamage = 0;

            // TODO: Once network dictionaries are supported, implement.
            // DamageRecords.TryGetValue((attacker.PlayerId, victim.PlayerId), out updatedDamage);
            //
            // updatedDamage += damage;
            // updatedDamage = Math.Min(updatedDamage, 100);
            //
            // DamageRecords[(attacker.PlayerId, victim.PlayerId)] = updatedDamage;
        }

        public void UpdatePlayerKarma(Player player, int delta)
        {
            UpdatePlayerIdKarma(player.Client.PlayerId, delta);
        }

        public void UpdatePlayerIdKarma(long playerId, int delta)
        {
            // int updatedKarma = 0;

            // // TODO: Once network dictionaries are supported, implement.
            // // KarmaRecords.TryGetValue(playerId, out updatedKarma);

            // updatedKarma += delta;

            // // Math.Clamp(updatedKarma, TTTKarmaMin, TTTKarmaMax)
            // updatedKarma = updatedKarma > TTTKarmaMax ? TTTKarmaMax : updatedKarma;
            // updatedKarma = updatedKarma < TTTKarmaMin ? TTTKarmaMin : updatedKarma;

            // TODO: Once network dictionaries are supported, implement.
            // KarmaRecords[player.playerId] = updatedKarma;
        }

        public void ResolveKarma()
        {
            // if (IsTracking)
            // {
            //     // TODO: Once network dictionaries are supported, implement.
            //     // Update karma records based on the damage done this round
            //     // foreach (var record in DamageRecords)
            //     // {
            //     //  UpdatePlayerIdKarma(record.Key.Item1, record.Value);
            //     // }
            // }

            // TODO: Once network dictionaries are supported, implement.
            // Clear all damage records
            // DamageRecords = new();
        }

        public bool IsBanned(Client client)
        {
            // TODO: Once network dictionaries are supported, implement. Return false meanwhile...
            // return (KarmaRecords[player.PlayerId] < TTTKarmaMin && TTTKarmaBan);
            return false;
        }
    }
}
