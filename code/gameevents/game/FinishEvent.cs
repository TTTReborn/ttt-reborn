using System.Collections.Generic;
using System.Text.Json.Serialization;

using Sandbox;

using TTTReborn.Globalization;
using TTTReborn.Teams;

namespace TTTReborn.Events.Game
{
    [GameEvent("game_finish"), Hammer.Skip]
    public partial class FinishEvent : NetworkableGameEvent, ILoggedGameEvent
    {
        public string TeamName { get; set; }

        [JsonIgnore]
        public Team Team
        {
            get => Utils.GetObjectByType<Team>(Utils.GetTypeByLibraryName<Team>(TeamName));
        }

        /// <summary>
        /// Round end event reporting the winning team.
        /// </summary>
        public FinishEvent(Team team) : base()
        {
            if (team != null)
            {
                TeamName = team.Name;
            }
        }

        public TranslationData GetDescriptionTranslationData() => new(GetTranslationKey("DESCRIPTION"), Team != null ? new TranslationData(Team.GetTranslationKey("NAME")) : "???");

        public override void Run() => Event.Run(Name, Team);

        protected override void OnRegister()
        {
            List<TTTReborn.Player> list = new();
            int aliveWinners = 0;

            foreach (TTTReborn.Player player in Utils.GetPlayers())
            {
                if (player.Role is not Roles.NoneRole)
                {
                    list.Add(player);

                    if (player.LifeState == LifeState.Alive && player.Team?.Name == TeamName)
                    {
                        aliveWinners++;
                    }
                }
            }

            Scoring = new GameEventScoring[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                TTTReborn.Player player = list[i];

                if (player.Team != null && player.Team.Name == TeamName && Team.CheckWin(player))
                {
                    Scoring[i] = new(player)
                    {
                        Karma = 10 * aliveWinners,
                        Score = 1 * aliveWinners
                    };

                    continue;
                }

                Scoring[i] = new(player)
                {
                    Karma = 0,
                    Score = Team is NoneTeam ? 0 : -1
                };
            }
        }

        public bool Contains(Client client) => true;
    }
}
