﻿using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TTTReborn.UI;
using TTTReborn.Player;

namespace TTTReborn.Gamemode
{
[Library("tttreborn", Title = "Trouble in Terry's Town")]
partial class Game : Sandbox.Game
{
    public enum Round { Waiting, PreRound, InProgress, PostRound }

    public static Game Instance { get => Current as Game; }

    [Net] public Round CurrentRound { get; private set; }
    [Net] public int TimeRemaining { get; private set; }

    public KarmaSystem Karma = new KarmaSystem();

    public Game()
    {
        if (IsServer)
        {
            new Hud();
        }
    }

    private void ChangeRound(Round round)
    {
        switch (round)
        {
            case Game.Round.Waiting:
                TimeRemaining = 0;
                Karma.IsTracking = false;

                break;

            case Game.Round.PreRound:
                TimeRemaining = TTTPreRoundTime;

                break;

            case Game.Round.InProgress:
                TimeRemaining = TTTRoundTime;

                int detectiveCount = (int) (All.Count * 0.125f);
                int traitorCount = (int) Math.Max(All.Count * 0.25f, 1f);

                List<TTTPlayer> _players = Client.All.ToList().ConvertAll(p => p.Pawn as TTTPlayer);
                Random random = new Random();

                // SELECT DETECTIVES
                for (int i = 0; i < detectiveCount; i++)
                {
                    int randomId = random.Next(_players.Count);
                    _players[randomId].Role = TTTPlayer.RoleType.Detective;

                    _players.RemoveAt(randomId);
                }

                // SELECT TRAITORS
                for (int i = 0; i < traitorCount; i++)
                {
                    int randomId = random.Next(_players.Count);
                    _players[randomId].Role = TTTPlayer.RoleType.Traitor;

                    _players.RemoveAt(randomId);
                }

                // SET REMAINING PLAYERS TO INNOCENT
                for (int i = 0; i < _players.Count; i++)
                {
                    _players[i].Role = TTTPlayer.RoleType.Innocent;
                }

                Karma.IsTracking = true;

                break;

            case Game.Round.PostRound:
                TimeRemaining = TTTPostRoundTime;

                Karma.ResolveKarma();
                Karma.IsTracking = false;

                break;
        }

        CurrentRound = round;
    }

    private void CheckMinimumPlayers()
    {
        if (Client.All.ToList().Count >= TTTMinPlayers)
        {
            if (CurrentRound == Round.Waiting)
            {
                ChangeRound(Round.PreRound);
            }
        }
        else if (CurrentRound != Round.Waiting)
        {
            ChangeRound(Round.Waiting);
        }
    }

    private void CheckRoundState()
    {
        if (CurrentRound != Round.InProgress)
            return;

        bool traitorsDead = true;
        bool innocentsDead = true;

        // Check for alive players
        for (int i = 0; i < Client.All.Count; i++)
        {
            TTTPlayer player = Client.All[i].Pawn as TTTPlayer;

            if (player.LifeState == LifeState.Alive)
                continue;

            if (player.Role == TTTPlayer.RoleType.Traitor)
            {
                traitorsDead = false;
            }
            else
            {
                innocentsDead = false;
            }
        }

        // End this round if there is just one team alive
        if (innocentsDead || traitorsDead)
        {
            ChangeRound(Round.PostRound);
        }
    }

    private void UpdateRoundTimer()
    {
        if (CurrentRound == Round.Waiting)
            return;

        if (TimeRemaining == 0)
        {
            switch (CurrentRound)
            {
                case Round.PreRound:
                    ChangeRound(Round.InProgress);

                    break;

                case Round.InProgress:
                    ChangeRound(Round.PostRound);

                    break;

                case Round.PostRound:
                    ChangeRound(Round.PreRound);

                    break;
            }
        }
        else
        {
            TimeRemaining--;
        }
    }

    private async Task StartGameTimer()
    {
        while (true)
        {
            UpdateGameTimer();
            await Task.DelaySeconds(1);
        }
    }

    private void UpdateGameTimer()
    {
        CheckMinimumPlayers();
        CheckRoundState();
        UpdateRoundTimer();
    }

    public override void DoPlayerNoclip(Client client)
    {
        // Do nothing. The player can't noclip in this mode.
    }

    public override void DoPlayerSuicide(Client client)
    {
        base.DoPlayerSuicide(client);
    }

    public override void PostLevelLoaded()
    {
        base.PostLevelLoaded();

        _ = StartGameTimer();
    }

    public override void OnKilled(Entity entity)
    {
        Client client = entity.GetClientOwner();
        if (client != null)
        {
            CheckRoundState();
        }

        base.OnKilled(entity);
    }

    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // TODO: KarmaSystem is waiting on network dictionaries.
        // Karma.RegisterPlayer(client);
        // if (Karma.IsBanned(player))
        // {
        //  KickPlayer(player);
        //
        //  return;
        // }

        TTTPlayer player = new TTTPlayer();
        Karma.RegisterPlayer(player);
        client.Pawn = player;

        player.Respawn();
    }

    public override void ClientDisconnect(Client client, NetworkDisconnectionReason reason)
    {
        Log.Info(client.Name + " left, checking minimum player count...");

        CheckRoundState();

        base.ClientDisconnect(client, reason);
    }
}

}
