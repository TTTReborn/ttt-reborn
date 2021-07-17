using Sandbox;

using TTTReborn.Player;
using TTTReborn.UI;

namespace TTTReborn.Items
{
    // [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    // public class WeaponAttribute : LibraryAttribute
    // {
    //     public WeaponType WeaponType;

    //     public WeaponAttribute(string name) : base(name)
    //     {

    //     }
    // }

    [Library("ttt_weapon")]
    public abstract partial class TTTWeapon : BaseWeapon, ICarriableItem
    {
        public virtual HoldType HoldType => Items.HoldType.Pistol;
        public virtual AmmoType AmmoType => AmmoType.Pistol;
        public virtual int ClipSize => 16;
        public virtual float ReloadTime => 3.0f;
        public virtual float DeployTime => 0.6f;
        public virtual int Bucket => 1;
        public virtual int BucketWeight => 100;
        public virtual bool UnlimitedAmmo => false;
        public virtual float ChargeAttackDuration => 2;
        public virtual bool HasFlashlight => false;
        public virtual bool HasLaserDot => false;
        public virtual int BaseDamage => 10;
        public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
        // TODO add player role to weapon to access in UI InventorySelection.cs.
        // E.G. this weapon is bought in traitor shop: Role => "Traitor";
        // This weapon is a normal weapon: Role => "None"

        [Net, Predicted]
        public int AmmoClip { get; set; }

        [Net, Predicted]
        public TimeSince TimeSinceReload { get; set; }

        [Net, Predicted]
        public bool IsReloading { get; set; }

        [Net, Predicted]
        public TimeSince TimeSinceDeployed { get; set; }

        [Net, Predicted]
        public TimeSince TimeSinceChargeAttack { get; set; }

        public float ChargeAttackEndTime;

        public PickupTrigger PickupTrigger { get; protected set; }

        public string Name { get; }

        public TTTWeapon() : base()
        {
            LibraryAttribute attribute = Library.GetAttribute(GetType());

            Name = attribute.Name;
        }

        public void Equip(TTTPlayer player)
        {
            OnEquip();
        }

        public virtual void OnEquip()
        {

        }

        public void Remove()
        {
            OnRemove();
        }

        public virtual void OnRemove()
        {

        }

        public int AvailableAmmo()
        {
            if (Owner is not TTTPlayer owner)
            {
                return 0;
            }

            return (owner.Inventory as Inventory).Ammo.Count(AmmoType);
        }

        public override void ActiveStart(Entity owner)
        {
            base.ActiveStart(owner);

            TimeSinceDeployed = 0;

            IsReloading = false;
        }

        public override void Spawn()
        {
            base.Spawn();

            AmmoClip = ClipSize;

            SetModel("weapons/rust_pistol/rust_pistol.vmdl");

            PickupTrigger = new PickupTrigger();
            PickupTrigger.Parent = this;
            PickupTrigger.Position = Position;
        }

        public override void Reload()
        {
            if (HoldType == Items.HoldType.Melee || IsReloading || AmmoClip >= ClipSize)
            {
                return;
            }

            TimeSinceReload = 0;

            if (Owner is TTTPlayer player && !UnlimitedAmmo && (player.Inventory as Inventory).Ammo.Count(AmmoType) <= 0)
            {
                return;
            }

            IsReloading = true;

            (Owner as AnimEntity).SetAnimBool("b_reload", true);

            DoClientReload();
        }

        public override void Simulate(Client owner)
        {
            if (TimeSinceDeployed < DeployTime)
            {
                return;
            }

            if (owner.Pawn is TTTPlayer player)
            {
                if (player.LifeState == LifeState.Alive)
                {
                    if (ChargeAttackEndTime > 0f && Time.Now >= ChargeAttackEndTime)
                    {
                        OnChargeAttackFinish();

                        ChargeAttackEndTime = 0f;
                    }
                }
                else
                {
                    ChargeAttackEndTime = 0f;
                }
            }

            if (!IsReloading)
            {
                base.Simulate(owner);
            }
            else if (TimeSinceReload > ReloadTime)
            {
                OnReloadFinish();
            }
        }

        public override bool CanPrimaryAttack()
        {
            if (ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime || TimeSinceDeployed <= DeployTime)
            {
                return false;
            }

            return base.CanPrimaryAttack();
        }

        public override bool CanSecondaryAttack()
        {
            if (ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime || TimeSinceDeployed <= DeployTime)
            {
                return false;
            }

            return base.CanSecondaryAttack();
        }

        public virtual void StartChargeAttack()
        {
            ChargeAttackEndTime = Time.Now + ChargeAttackDuration;
        }

        public virtual void OnChargeAttackFinish()
        {

        }

        public virtual void OnReloadFinish()
        {
            IsReloading = false;

            if (Owner is not TTTPlayer player)
            {
                return;
            }

            if (!UnlimitedAmmo)
            {
                int ammo = (player.Inventory as Inventory).Ammo.Take(AmmoType, ClipSize - AmmoClip);

                if (ammo == 0)
                {
                    return;
                }

                AmmoClip += ammo;
            }
            else
            {
                AmmoClip = ClipSize;
            }
        }

        [ClientRpc]
        public virtual void DoClientReload()
        {
            ViewModelEntity?.SetAnimBool("reload", true);
        }

        public override void AttackPrimary()
        {
            TimeSincePrimaryAttack = 0;
            TimeSinceSecondaryAttack = 0;

            ShootEffects();

            ShootBullet(0.05f, 1.5f, BaseDamage, 3.0f);
        }

        [ClientRpc]
        protected virtual void ShootEffects()
        {
            Host.AssertClient();

            if (HoldType != Items.HoldType.Melee)
            {
                Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle");
            }

            if (IsLocalPawn)
            {
                new Sandbox.ScreenShake.Perlin();
            }

            ViewModelEntity?.SetAnimBool("fire", true);
            CrosshairPanel?.CreateEvent("fire");
        }

        public virtual void ShootBullet(float spread, float force, float damage, float bulletSize)
        {
            Vector3 forward = Owner.EyeRot.Forward;
            forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
            forward = forward.Normal;

            foreach (TraceResult tr in TraceBullet(Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize))
            {
                tr.Surface.DoBulletImpact(tr);

                if (!IsServer || !tr.Entity.IsValid())
                {
                    continue;
                }

                using (Prediction.Off())
                {
                    DamageInfo damageInfo = DamageInfo.FromBullet(tr.EndPos, forward * 100 * force, damage)
                        .UsingTraceResult(tr)
                        .WithAttacker(Owner)
                        .WithWeapon(this);

                    tr.Entity.TakeDamage(damageInfo);
                }
            }
        }

        public bool TakeAmmo(int amount)
        {
            if (AmmoClip < amount)
            {
                return false;
            }

            AmmoClip -= amount;

            return true;
        }

        public override void CreateViewModel()
        {
            Host.AssertClient();

            if (string.IsNullOrEmpty(ViewModelPath))
            {
                return;
            }

            ViewModelEntity = new ViewModel
            {
                Position = Position,
                Owner = Owner,
                EnableViewmodelRendering = true
            };

            ViewModelEntity.SetModel(ViewModelPath);
        }

        public override void CreateHudElements()
        {
            if (Local.Hud == null)
            {
                return;
            }

            // TODO: Give users a way to change their crosshair.
            CrosshairPanel = new Crosshair().SetupCrosshair(new Crosshair.Properties(true,
                false,
                false,
                10,
                2,
                0,
                0,
                Color.Green));
            CrosshairPanel.Parent = Local.Hud;
            CrosshairPanel.AddClass(ClassInfo.Name);
        }

        public bool IsUsable()
        {
            if (HoldType == Items.HoldType.Melee || ClipSize == 0 || AmmoClip > 0)
            {
                return true;
            }

            return AvailableAmmo() > 0;
        }

        public override void OnCarryStart(Entity carrier)
        {
            base.OnCarryStart(carrier);

            if (PickupTrigger.IsValid())
            {
                PickupTrigger.EnableTouch = false;
            }
        }

        public override void OnCarryDrop(Entity dropper)
        {
            base.OnCarryDrop(dropper);

            if (PickupTrigger.IsValid())
            {
                PickupTrigger.EnableTouch = true;
            }
        }

        public virtual bool CanDrop() => true;
    }
}
