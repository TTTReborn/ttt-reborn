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

using Sandbox;

using TTTReborn.Player;
using TTTReborn.Roles;

namespace TTTReborn.VisualProgramming
{
    public partial class RoleSelectionStackNode : StackNode
    {
        public TTTRole SelectedRole { get; set; }

        public RoleSelectionStackNode() : base()
        {

        }

        public override object[] Build(params object[] input)
        {
            if (input == null || input[0] is not List<TTTPlayer> playerList)
            {
                return null;
            }

            if (SelectedRole == null)
            {
                throw new NodeStackException("No selected role in RoleSelectionNode.");
            }

            foreach (TTTPlayer player in playerList)
            {
                Log.Info($"Selected '{player.Client.Name}' with role '{SelectedRole.Name}'");
            }

            return base.Build(input[0]);
        }
    }
}
