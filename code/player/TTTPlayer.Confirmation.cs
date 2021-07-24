using Sandbox;

using TTTReborn.Globals;
using TTTReborn.UI;

namespace TTTReborn.Player
{
    public partial class TTTPlayer
    {
        public PlayerCorpse PlayerCorpse { get; set; }

        [Net]
        public int CorpseCredits { get; set; } = 0;

        public bool IsConfirmed = false;

        public bool IsMissingInAction = false;

        public TTTPlayer CorpseConfirmer = null;

        private const float INSPECT_CORPSE_DISTANCE = 80f;

        private PlayerCorpse _inspectingPlayerCorpse = null;

        public void RemovePlayerCorpse()
        {
            if (PlayerCorpse == null || !PlayerCorpse.IsValid())
            {
                return;
            }

            PlayerCorpse.Delete();
            PlayerCorpse = null;
        }

        private void TickAttemptInspectPlayerCorpse()
        {
            using (Prediction.Off())
            {
                PlayerCorpse playerCorpse = IsLookingAtType<PlayerCorpse>(INSPECT_CORPSE_DISTANCE);

                if (playerCorpse != null)
                {
                    if (IsServer && !playerCorpse.IsIdentified && Input.Pressed(InputButton.Use) && LifeState == LifeState.Alive)
                    {
                        playerCorpse.IsIdentified = true;

                        // TODO Handling if a player disconnects!
                        if (playerCorpse.Player != null && playerCorpse.Player.IsValid())
                        {
                            playerCorpse.Player.IsConfirmed = true;
                            playerCorpse.Player.CorpseConfirmer = this;

                            int credits = playerCorpse.Player.Credits;

                            if (credits > 0)
                            {
                                Credits += credits;
                                playerCorpse.Player.Credits = 0;
                                playerCorpse.Player.CorpseCredits = credits;
                            }

                            playerCorpse.Player.GetClientOwner()?.SetScore("alive", false);

                            RPCs.ClientConfirmPlayer(this, playerCorpse, playerCorpse.Player, playerCorpse.Player.Role.Name);
                        }
                    }

                    if (_inspectingPlayerCorpse != playerCorpse)
                    {
                        _inspectingPlayerCorpse = playerCorpse;

                        if (IsClient)
                        {
                            InspectMenu.Instance.InspectCorpse(playerCorpse.Player);
                        }
                    }
                }
                else if (_inspectingPlayerCorpse != null)
                {
                    if (IsClient && InspectMenu.Instance.IsShowing)
                    {
                        InspectMenu.Instance.IsShowing = false;
                    }

                    _inspectingPlayerCorpse = null;
                }
            }
        }

        private void BecomePlayerCorpseOnServer(Vector3 force, int forceBone)
        {
            PlayerCorpse corpse = new PlayerCorpse
            {
                Position = Position,
                Rotation = Rotation
            };

            corpse.CopyFrom(this);
            corpse.ApplyForceToBone(force, forceBone);

            PlayerCorpse = corpse;
        }
    }
}
