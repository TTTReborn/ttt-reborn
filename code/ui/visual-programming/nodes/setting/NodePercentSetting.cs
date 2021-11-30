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

using Sandbox.UI.Construct;

namespace TTTReborn.UI.VisualProgramming
{
    [NodeSetting("percent")]
    public class NodePercentSetting : NodeSetting
    {
        public Sandbox.UI.TextEntry PercentEntry;

        public NodePercentSetting() : base()
        {
            Content.SetPanelContent((panelContent) =>
            {
                PercentEntry = panelContent.Add.TextEntry(""); // TODO improve with validity checks and error toggling
                PercentEntry.AddEventListener("onchange", (panelEvent) =>
                {
                    try
                    {
                        if (Node is PercentageSelectionNode percentageSelectionNode)
                        {
                            percentageSelectionNode.OnChange();
                        }

                        Node?.RemoveHighlights();
                    }
                    catch (Exception)
                    {
                        Node?.HighlightError();
                    }
                });
            });
        }
    }
}
