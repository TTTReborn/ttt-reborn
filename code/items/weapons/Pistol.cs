using Sandbox;

namespace TTTReborn.Items
{
    [Library("ttt_pistol")]
    partial class Pistol : TTTWeapon, IBuyableItem
    {
        public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
        public override HoldType HoldType => Items.HoldType.Pistol;
        public override bool UnlimitedAmmo => true;
        public override int ClipSize => 15;
        public override float PrimaryRate => 15.0f;
        public override float SecondaryRate => 1.0f;
        public override float ReloadTime => 3.0f;
        public override float DeployTime => 0.4f;
        public override bool HasLaserDot => true;
        public override int BaseDamage => 8;
        public override int Bucket => 1;

        public virtual int Price => 100;

        public override void Spawn()
        {
            base.Spawn();

            SetModel("weapons/rust_pistol/rust_pistol.vmdl");
        }

        public override bool CanPrimaryAttack()
        {
            return base.CanPrimaryAttack() && Input.Pressed(InputButton.Attack1);
        }

        public override void AttackPrimary()
        {
            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;

            if (!TakeAmmo(1))
            {
                PlaySound("pistol.dryfire").SetPosition(Position).SetVolume(0.2f);

                return;
            }

            (Owner as AnimEntity).SetAnimBool("b_attack", true);

            using (Prediction.Off())
            {
                ShootEffects();
            }

            PlaySound("rust_pistol.shoot").SetPosition(Position).SetVolume(0.8f);
            ShootBullet(0.05f, 1.5f, BaseDamage, 3.0f);
        }
    }
}
