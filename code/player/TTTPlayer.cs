using Sandbox;

using TTTReborn.Player.Camera;
using TTTReborn.Roles;
using TTTReborn.Items;

namespace TTTReborn.Player
{
    public partial class TTTPlayer : Sandbox.Player
    {
        private static int WeaponDropVelocity { get; set; } = 300;

        public BaseRole Role { get; set; } = new NoneRole();

        [Net, Local]
        public int Credits { get; set; } = 0;

        private DamageInfo lastDamageInfo;

        private TimeSince timeSinceDropped = 0;

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
            Credits = 400;

            GetClientOwner().SetScore("alive", true);

            using(Prediction.Off())
            {
                ClientOnPlayerSpawned(this);
                ClientSetRole(To.Single(this), Role.Name);
            }

            RemovePlayerCorpse();
            Inventory.DeleteContents();
            TTTReborn.Gamemode.Game.Instance?.Round?.OnPlayerSpawn(this);

            base.Respawn();

            // hacky
            // TODO use a spectator flag, otherwise, no player can respawn during round with an item etc.
            // TODO spawn player as spectator instantly
            if (Gamemode.Game.Instance.Round is Rounds.InProgressRound || Gamemode.Game.Instance.Round is Rounds.PostRound)
            {
                GetClientOwner().SetScore("alive", false);

                return;
            }

            if (Gamemode.Game.Instance.Round is Rounds.PreRound)
            {
                IsConfirmed = false;
                CorpseConfirmer = null;
            }
        }

        public override void OnKilled()
        {
            base.OnKilled();

            BecomePlayerCorpseOnServer(lastDamageInfo.Force, GetHitboxBone(lastDamageInfo.HitboxIndex));

            Inventory.DropActive();
            Inventory.DeleteContents();

            using(Prediction.Off())
            {
                ClientOnPlayerDied(To.Single(this), this);
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
                int weaponSlot = (int) (ActiveChild as TTTWeapon).WeaponType;
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

        public override void TakeDamage(DamageInfo info)
        {
            // Headshot deals x2 damage
            if (info.HitboxIndex == 0)
            {
                info.Damage *= 2.0f;
            }

            if (info.Attacker is TTTPlayer attacker && attacker != this)
            {
                attacker.ClientDidDamage(info.Position, info.Damage, ((float)Health).LerpInverse(100, 0));
            }

            if (info.Weapon != null)
            {
                ClientTookDamage(info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position);
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
    }
}
