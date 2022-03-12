using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox;

using TTTReborn.Player;

namespace TTTReborn.Teams
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TeamAttribute : LibraryAttribute
    {
        public TeamAttribute(string name) : base("ttt_team_" + name)
        {

        }
    }

    public abstract class Team
    {
        public readonly string Name;

        public abstract Color Color { get; }

        public readonly List<TTTPlayer> Members = new();

        public static Dictionary<string, Team> Teams = new();

        public Team()
        {
            Name = Utils.GetLibraryName(GetType());

            Teams[Name] = this;
        }

        public IEnumerable<Client> GetClients()
        {
            return Members.Select(x => x.Client);
        }

        public virtual bool CheckWin(TTTPlayer player) => true;

        public virtual bool CheckPreventWin(TTTPlayer player) => false;
    }

    public static class TeamFunctions
    {
        public static Team TryGetTeam(string teamname)
        {
            if (teamname == null || !Team.Teams.TryGetValue(teamname, out Team team))
            {
                return null;
            }

            return team;
        }

        public static Team GetTeam(string teamname)
        {
            if (teamname == null)
            {
                return null;
            }

            if (!Team.Teams.TryGetValue(teamname, out Team team))
            {
                team = Utils.GetObjectByType<Team>(Utils.GetTypeByLibraryName<Team>(teamname));
            }

            return team;
        }

        public static Team GetTeam(Type teamType)
        {
            foreach (Team team in Team.Teams.Values)
            {
                if (team.GetType() == teamType)
                {
                    return team;
                }
            }

            return Utils.GetObjectByType<Team>(teamType);
        }
    }
}
