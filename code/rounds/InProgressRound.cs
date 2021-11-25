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

using System.Collections.Generic;
using System.Linq;

using Sandbox;

using TTTReborn.Events;
using TTTReborn.Items;
using TTTReborn.Map;
using TTTReborn.Player;
using TTTReborn.Settings;
using TTTReborn.Teams;

namespace TTTReborn.Rounds
{
    public partial class InProgressRound : BaseRound
    {
        public override string RoundName => "In Progress";

        [Net]
        public List<TTTPlayer> Players { get; set; }

        [Net]
        public List<TTTPlayer> Spectators { get; set; }

        private List<TTTLogicButton> _logicButtons;

        public override int RoundDuration
        {
            get => ServerSettings.Instance.Round.RoundTime;
        }

        public override void OnPlayerKilled(TTTPlayer player)
        {
            Players.Remove(player);
            Spectators.AddIfDoesNotContain(player);

            player.MakeSpectator();
            ChangeRoundIfOver();
        }

        public override void OnPlayerJoin(TTTPlayer player)
        {
            Spectators.AddIfDoesNotContain(player);
        }

        public override void OnPlayerLeave(TTTPlayer player)
        {
            Players.Remove(player);
            Spectators.Remove(player);

            ChangeRoundIfOver();
        }

        protected override void OnStart()
        {
            if (Host.IsServer)
            {
                // For now, if the RandomWeaponCount of the map is zero, let's just give the players
                // a fixed weapon loadout.
                if (Gamemode.Game.Instance.MapHandler.RandomWeaponCount == 0)
                {
                    foreach (TTTPlayer player in Players)
                    {
                        GiveFixedLoadout(player);
                    }
                }

                // Cache buttons for OnSecond tick.
                _logicButtons = Entity.All.Where(x => x.GetType() == typeof(TTTLogicButton)).Select(x => x as TTTLogicButton).ToList();
            }
        }

        private static void GiveFixedLoadout(TTTPlayer player)
        {
            Log.Debug($"Added Fixed Loadout to {player.Client.Name}");

            // Randomize between SMG and shotgun
            if (Utils.RNG.Next() % 2 == 0)
            {
                if (player.Inventory.TryAdd(new Shotgun(), deleteIfFails: true, makeActive: false))
                {
                    player.Inventory.Ammo.Give("ammo_buckshot", 16);
                }
            }
            else
            {
                if (player.Inventory.TryAdd(new SMG(), deleteIfFails: true, makeActive: false))
                {
                    player.Inventory.Ammo.Give("ammo_smg", 60);
                }
            }

            if (player.Inventory.TryAdd(new Pistol(), deleteIfFails: true, makeActive: false))
            {
                player.Inventory.Ammo.Give("ammo_pistol", 30);
            }
        }

        protected override void OnTimeUp()
        {
            LoadPostRound(TeamFunctions.GetTeam(typeof(InnocentTeam)));

            base.OnTimeUp();
        }

        private TTTTeam IsRoundOver()
        {
            List<TTTTeam> aliveTeams = new();

            foreach (TTTPlayer player in Players)
            {
                if (player.Team != null && !aliveTeams.Contains(player.Team))
                {
                    aliveTeams.Add(player.Team);
                }
            }

            if (aliveTeams.Count == 0)
            {
                return TeamFunctions.GetTeam(typeof(NoneTeam));
            }

            return aliveTeams.Count == 1 ? aliveTeams[0] : null;
        }

        private static void LoadPostRound(TTTTeam winningTeam)
        {
            Gamemode.Game.Instance.MapSelection.TotalRoundsPlayed++;
            Gamemode.Game.Instance.ForceRoundChange(new PostRound());
            RPCs.ClientOpenAndSetPostRoundMenu(
                winningTeam.Name,
                winningTeam.Color
            );
        }

        public override void OnSecond()
        {
            if (Host.IsServer)
            {
                if (!ServerSettings.Instance.Debug.PreventWin)
                {
                    base.OnSecond();
                }
                else
                {
                    RoundEndTime += 1f;
                }

                _logicButtons.ForEach(x => x.OnSecond()); // Tick role button delay timer.

                if (!Utils.HasMinimumPlayers() && IsRoundOver() == null)
                {
                    Gamemode.Game.Instance.ForceRoundChange(new WaitingRound());
                }
            }
        }

        private bool ChangeRoundIfOver()
        {
            TTTTeam result = IsRoundOver();

            if (result != null && !Settings.ServerSettings.Instance.Debug.PreventWin)
            {
                LoadPostRound(result);

                return true;
            }

            return false;
        }

        [Event(TTTEvent.Player.Role.Select)]
        private static void OnPlayerRoleChange(TTTPlayer player)
        {
            if (Host.IsClient)
            {
                return;
            }

            if (Gamemode.Game.Instance.Round is InProgressRound inProgressRound)
            {
                inProgressRound.ChangeRoundIfOver();
            }
        }

        [Event(TTTEvent.Settings.Change)]
        private static void OnChangeSettings()
        {
            if (Gamemode.Game.Instance.Round is not InProgressRound inProgressRound)
            {
                return;
            }

            inProgressRound.ChangeRoundIfOver();
        }
    }
}
