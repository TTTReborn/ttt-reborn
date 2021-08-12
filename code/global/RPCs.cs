using Sandbox;

using TTTReborn.Items;
using TTTReborn.Player;
using TTTReborn.Roles;
using TTTReborn.Teams;
using TTTReborn.UI;

namespace TTTReborn.Globals
{
    public partial class RPCs
    {
        [ClientRpc]
        public static void ClientOnPlayerDied(TTTPlayer player)
        {
            if (!player.IsValid())
            {
                return;
            }

            Event.Run("tttreborn.player.died", player);
        }

        [ClientRpc]
        public static void ClientOnPlayerSpawned(TTTPlayer player)
        {
            if (!player.IsValid())
            {
                return;
            }

            player.IsMissingInAction = false;
            player.IsConfirmed = false;
            player.CorpseConfirmer = null;

            player.SetRole(new NoneRole());

            Event.Run("tttreborn.player.spawned", player);
        }

        /// <summary>
        /// Must be called on the server, updates TTTPlayer's `Role`.
        /// </summary>
        /// <param name="player">The player whose `Role` is to be updated</param>
        /// <param name="roleName">Same as the `TTTReborn.Roles.TTTRole`'s `TTTReborn.Roles.RoleAttribute`'s name</param>
        /// <param name="teamName">The name of the team</param>
        [ClientRpc]
        public static void ClientSetRole(TTTPlayer player, string roleName, string teamName = null)
        {
            if (!player.IsValid())
            {
                return;
            }

            player.SetRole(Utils.GetObjectByType<TTTRole>(Utils.GetTypeByName<TTTRole>(roleName)), TeamFunctions.GetTeam(teamName));

            Scoreboard.Instance.UpdatePlayer(player.GetClientOwner());
        }

        [ClientRpc]
        public static void ClientConfirmPlayer(TTTPlayer confirmPlayer, PlayerCorpse playerCorpse, TTTPlayer deadPlayer, string roleName, string teamName, ConfirmationData confirmationData, string killerWeapon, string[] perks)
        {
            if (!deadPlayer.IsValid())
            {
                return;
            }

            deadPlayer.SetRole(Utils.GetObjectByType<TTTRole>(Utils.GetTypeByName<TTTRole>(roleName)), TeamFunctions.GetTeam(teamName));

            deadPlayer.IsConfirmed = true;
            deadPlayer.CorpseConfirmer = confirmPlayer;

            if (playerCorpse.IsValid())
            {
                playerCorpse.Player = deadPlayer;
                playerCorpse.KillerWeapon = killerWeapon;
                playerCorpse.Perks = perks;

                playerCorpse.CopyConfirmationData(confirmationData);

                if (InspectMenu.Instance?.PlayerCorpse == playerCorpse)
                {
                    InspectMenu.Instance.InspectCorpse(playerCorpse);
                }
            }

            Scoreboard.Instance.UpdatePlayer(deadPlayer.GetClientOwner());

            if (!confirmPlayer.IsValid())
            {
                return;
            }

            Client confirmClient = confirmPlayer.GetClientOwner();
            Client deadClient = deadPlayer.GetClientOwner();

            InfoFeed.Current?.AddEntry(
                confirmClient,
                deadClient,
                "found the body of",
                $"({deadPlayer.Role.Name})"
            );

            if (confirmPlayer == Local.Pawn as TTTPlayer && deadPlayer.CorpseCredits > 0)
            {
                InfoFeed.Current?.AddEntry(
                    confirmClient,
                    $"found $ {deadPlayer.CorpseCredits} credits!"
                );
            }
        }

        [ClientRpc]
        public static void ClientAddMissingInAction(TTTPlayer missingInActionPlayer)
        {
            if (!missingInActionPlayer.IsValid())
            {
                return;
            }

            missingInActionPlayer.IsMissingInAction = true;

            Scoreboard.Instance.UpdatePlayer(missingInActionPlayer.GetClientOwner());
        }

        [ClientRpc]
        public static void ClientOpenAndSetPostRoundMenu(string winningTeam, Color winningColor)
        {
            PostRoundMenu.Instance.OpenAndSetPostRoundMenu(new PostRoundStats(
                winningRole: winningTeam,
                winningColor: winningColor
            ));
        }

        [ClientRpc]
        public static void ClientClosePostRoundMenu()
        {
            PostRoundMenu.Instance.IsShowing = false;
        }

        [ClientRpc]
        public static void ClientOnPlayerCarriableItemPickup(Entity carriable)
        {
            Event.Run("tttreborn.player.carriableitem.pickup", carriable as ICarriableItem);
        }

        [ClientRpc]
        public static void ClientOnPlayerCarriableItemDrop(Entity carriable)
        {
            Event.Run("tttreborn.player.carriableitem.drop", carriable as ICarriableItem);
        }

        [ClientRpc]
        public static void ClientClearInventory()
        {
            Event.Run("tttreborn.player.inventory.clear");
        }
    }
}
