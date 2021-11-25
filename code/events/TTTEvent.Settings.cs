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

namespace TTTReborn.Events
{
    public static partial class TTTEvent
    {
        public static class Settings
        {
            /// <summary>
            /// Occurs when server or client settings are changed.
            /// <para>No data is passed to this event.</para>
            /// </summary>
            public const string Change = "tttreborn.settings.change";

            /// <summary>
            /// Occurs when server or client language settings are changed.
            /// <para>Event is passed the old <strong><see cref="TTTReborn.Globalization.Language"/></strong> instance.</para>
            /// <para>Event is passed the new <strong><see cref="TTTReborn.Globalization.Language"/></strong> instance.</para>
            /// </summary>
            public const string LanguageChange = "tttreborn.settings.languagechange";
        }
    }
}
