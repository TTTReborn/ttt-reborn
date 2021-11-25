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

using Sandbox;

using TTTReborn.Globals;
using TTTReborn.Player;

namespace TTTReborn.VisualProgramming
{
    public partial class PercentageSelectionStackNode : StackNode
    {
        public List<float> PercentList { get; set; } = new();

        public PercentageSelectionStackNode() : base()
        {

        }

        public override object[] Build(params object[] input)
        {
            if (input == null || input[0] is not List<TTTPlayer> playerList)
            {
                return null;
            }

            int percentListCount = PercentList.Count;

            if (percentListCount < 2)
            {
                throw new NodeStackException("Missing values in RandomNode.");
            }

            int allPlayerAmount = Client.All.Count;

            object[] buildArray = new object[percentListCount];

            for (int i = 0; i < percentListCount; i++)
            {
                int playerAmount = (int) MathF.Floor((float) allPlayerAmount * (PercentList[i] / 100f));

                List<TTTPlayer> selectedPlayers = new();

                for (int index = 0; index < playerAmount; index++)
                {
                    int rnd = Utils.RNG.Next(playerList.Count);

                    selectedPlayers.Add(playerList[rnd]);
                    playerList.RemoveAt(rnd);
                }

                buildArray[i] = selectedPlayers;
            }

            if (playerList.Count > 0)
            {
                (buildArray[^1] as List<TTTPlayer>).AddRange(playerList);
            }

            return base.Build(buildArray);
        }
    }
}
