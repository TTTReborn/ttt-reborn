// TTT Reborn https://github.com/TTTReborn/tttreborn/
// Copyright (C) Neoxult

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see https://github.com/TTTReborn/tttreborn/blob/master/LICENSE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sandbox;

using TTTReborn.Items;
using TTTReborn.Player;
using TTTReborn.Roles;
using TTTReborn.Settings;

namespace TTTReborn.Rounds
{
    public class PreRound : BaseRound
    {
        public override string RoundName => "Preparing";
        public override int RoundDuration
        {
            get => ServerSettings.Instance.Round.PreRoundTime;
        }

        public override void OnPlayerKilled(TTTPlayer player)
        {
            _ = StartRespawnTimer(player);

            player.MakeSpectator();

            base.OnPlayerKilled(player);
        }

        protected override void OnStart()
        {
            if (Host.IsServer)
            {
                Gamemode.Game.Instance.MapHandler.Reset();

                foreach (Client client in Client.All)
                {
                    if (client.Pawn is TTTPlayer player)
                    {
                        player.RemoveLogicButtons();
                        player.Respawn();
                    }
                }
            }
        }

        protected override void OnTimeUp()
        {
            base.OnTimeUp();

            List<TTTPlayer> players = new();
            List<TTTPlayer> spectators = new();

            foreach (TTTPlayer player in Utils.GetPlayers())
            {
                player.Client.SetValue("forcedspectator", player.IsForcedSpectator);

                if (player.IsForcedSpectator)
                {
                    player.MakeSpectator(false);
                    spectators.Add(player);
                }
                else
                {
                    players.Add(player);
                }
            }

            AssignRolesAndRespawn(players);

            Gamemode.Game.Instance.ChangeRound(new InProgressRound
            {
                Players = players,
                Spectators = spectators
            });
        }

        private void AssignRolesAndRespawn(List<TTTPlayer> players)
        {
            int traitorCount = (int) Math.Max(players.Count * 0.25f, 1f);

            for (int i = 0; i < traitorCount; i++)
            {
                List<TTTPlayer> unassignedPlayers = players.Where(p => p.Role is NoneRole).ToList();
                int randomId = Utils.RNG.Next(unassignedPlayers.Count);

                if (unassignedPlayers[randomId].Role is NoneRole)
                {
                    unassignedPlayers[randomId].SetRole(new TraitorRole());
                }
            }

            int detectiveCount = (int) (players.Count * 0.125f);

            for (int i = 0; i < detectiveCount; i++)
            {
                List<TTTPlayer> unassignedPlayers = players.Where(p => p.Role is NoneRole).ToList();
                int randomId = Utils.RNG.Next(unassignedPlayers.Count);

                if (unassignedPlayers[randomId].Role is NoneRole)
                {
                    unassignedPlayers[randomId].SetRole(new DetectiveRole());
                }
            }

            foreach (TTTPlayer player in players)
            {
                if (player.Role is NoneRole)
                {
                    player.SetRole(new InnocentRole());
                }

                using (Prediction.Off())
                {
                    player.SendClientRole();
                }

                if (player.LifeState == LifeState.Dead)
                {
                    player.Respawn();
                }
                else
                {
                    player.SetHealth(player.MaxHealth);
                }
            }
        }

        private static async Task StartRespawnTimer(TTTPlayer player)
        {
            await Task.Delay(1000);

            if (player.IsValid() && Gamemode.Game.Instance.Round is PreRound)
            {
                player.Respawn();
            }
        }

        public override void OnPlayerSpawn(TTTPlayer player)
        {
            bool handsAdded = player.Inventory.TryAdd(new Hands(), deleteIfFails: true, makeActive: false);

            Log.Debug($"Attempting to add Hands to {player.Client.Name} {handsAdded}");

            base.OnPlayerSpawn(player);
        }
    }
}
