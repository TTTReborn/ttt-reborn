using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

using TTTReborn.Gamemode;
using TTTReborn.Player.Camera;
using TTTReborn.Roles;
using TTTReborn.UI;
using TTTReborn.Weapons;

namespace TTTReborn.Player
{
    public partial class TTTPlayer : Sandbox.Player
    {
        public PlayerCorpse PlayerCorpse { get; set; }

        private static int WeaponDropVelocity { get; set; } = 300;

        public BaseRole Role { get; set; } = new NoneRole();

        [Net, Local]
        public int Credits { get; set; } = 0;

        private DamageInfo lastDamageInfo;
        private float inspectCorpseDistance = 80f;

        private TimeSince timeSinceDropped = 0;

        private PlayerCorpse inspectingPlayerCorpse = null;

        public TTTPlayer()
        {
            Inventory = new Inventory(this);
        }

        public void MakeSpectator(Vector3 position = default)
        {
            EnableAllCollisions = false;
            EnableDrawing = false;
            Controller = null;
            Camera = new SpectateCamera
            {
                DeathPosition = position,
                TimeSinceDied = 0
            };
        }

        public void RemovePlayerCorpse()
        {
            if (PlayerCorpse != null && PlayerCorpse.IsValid())
            {
                PlayerCorpse.Delete();
                PlayerCorpse = null;
            }
        }

        // Important: Server-side only
        // TODO: Convert to a player.RPC, event based system found inside of...
        // TODO: https://github.com/TTTReborn/ttt-reborn/commit/1776803a4b26d6614eba13b363bbc8a4a4c14a2e#diff-d451f87d88459b7f181b1aa4bbd7846a4202c5650bd699912b88ff2906cacf37R30
        public override void Respawn()
        {

            SetModel("models/citizen/citizen.vmdl");

            Controller = new WalkController();
            Animator = new StandardPlayerAnimator();
            Camera = new FirstPersonCamera();

            EnableAllCollisions = true;
            EnableDrawing = true;
            EnableHideInFirstPerson = true;
            EnableShadowInFirstPerson = true;

            Role = new NoneRole();
            Credits = 0;

            using(Prediction.Off())
            {
                To client = To.Single(this);

                ClientSetRole(client, Role.Name);
                ClientOnPlayerSpawned(client);
            }

            RemovePlayerCorpse();
            Inventory.DeleteContents();
            TTTReborn.Gamemode.Game.Instance?.Round?.OnPlayerSpawn(this);
            base.Respawn();
        }

        public override void OnKilled()
        {
            base.OnKilled();

            BecomePlayerCorpseOnServer(lastDamageInfo.Force, GetHitboxBone(lastDamageInfo.HitboxIndex));

            Inventory.DropActive();
            Inventory.DeleteContents();

            using(Prediction.Off())
            {
                ClientOnPlayerDied(To.Single(this));
            }
        }

        public override void Simulate(Client client)
        {
            // Input requested a weapon switch
            if (Input.ActiveChild != null)
            {
                ActiveChild = Input.ActiveChild;
            }

            if (LifeState != LifeState.Alive)
            {
                return;
            }

            TickPlayerUse();

            TickPlayerDropWeapon();

            SimulateActiveChild(client, ActiveChild);

            if (IsServer)
            {
                TickAttemptInspectPlayerCorpse();
            }

            PawnController controller = GetActiveController();
            controller?.Simulate(client, this, GetActiveAnimator());
        }

        protected override void UseFail()
        {
            // Do nothing. By default this plays a sound that we don't want.
        }

        public override void StartTouch(Entity other)
        {
            if (timeSinceDropped < 1)
            {
                return;
            }

            base.StartTouch(other);
        }

        private void TickPlayerDropWeapon()
        {
            if (Input.Pressed(InputButton.Drop) && ActiveChild != null && Inventory != null)
            {
                int weaponSlot = (int) (ActiveChild as Weapon).WeaponType;
                Entity droppedEntity = Inventory.DropActive();

                if (droppedEntity != null)
                {
                    if (droppedEntity.PhysicsGroup != null)
                    {
                        droppedEntity.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * WeaponDropVelocity;
                    }

                    timeSinceDropped = 0;
                }
            }
        }

        private void TickAttemptInspectPlayerCorpse()
        {
            using (Prediction.Off())
            {
                To client = To.Single(this);
                PlayerCorpse playerCorpse = IsLookingAtPlayerCorpse();

                if (playerCorpse != null)
                {
                    if (inspectingPlayerCorpse != playerCorpse)
                    {
                        inspectingPlayerCorpse = playerCorpse;

                        // Send the request to the player looking at the player corpse.
                        // https://wiki.facepunch.com/sbox/RPCs#targetingplayers
                        ClientOpenInspectMenu(client, playerCorpse.Player, playerCorpse.IsIdentified);
                    }

                    if (!playerCorpse.IsIdentified && Input.Down(InputButton.Use))
                    {
                        playerCorpse.IsIdentified = true;

                        ClientConfirmPlayer(this, playerCorpse.Player, playerCorpse.Player.Role.Name);

                        ClientOpenInspectMenu(client, playerCorpse.Player, playerCorpse.IsIdentified);
                    }

                    return;
                }

                if (inspectingPlayerCorpse != null)
                {
                    ClientCloseInspectMenu(client);

                    inspectingPlayerCorpse = null;
                }
            }
        }

        private PlayerCorpse IsLookingAtPlayerCorpse()
        {
            TraceResult trace = Trace.Ray(EyePos, EyePos + EyeRot.Forward * inspectCorpseDistance)
                .HitLayer(CollisionLayer.Debris)
                .Ignore(ActiveChild)
                .Ignore(this)
                .Radius(2)
                .Run();

            if (trace.Hit && trace.Entity is PlayerCorpse corpse && corpse.Player != null)
            {
                return corpse;
            }

            return null;
        }

        public override void TakeDamage(DamageInfo info)
        {
            // Headshot deals x2 damage
            if (info.HitboxIndex == 0)
            {
                info.Damage *= 2.0f;
            }

            if (info.Attacker is TTTPlayer attacker && attacker != this)
            {
                attacker.DidDamage(info.Position, info.Damage, ((float) Health).LerpInverse(100, 0));
            }

            if (info.Weapon != null)
            {
                TookDamage(info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position);
            }

            // Play pain sounds
            if ((info.Flags & DamageFlags.Fall) == DamageFlags.Fall)
            {
                PlaySound("fall");
            }
            else if ((info.Flags & DamageFlags.Bullet) == DamageFlags.Bullet)
            {
                PlaySound("grunt" + Rand.Int(1, 4));
            }

            // Register player damage with the Karma system
            TTTReborn.Gamemode.Game.Instance?.Karma?.RegisterPlayerDamage(info.Attacker as TTTPlayer, this, info.Damage);

            lastDamageInfo = info;

            base.TakeDamage(info);
        }

        protected override void OnDestroy()
        {
            RemovePlayerCorpse();

            base.OnDestroy();
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
            corpse.Player = this;

            PlayerCorpse = corpse;
        }
    }
}
