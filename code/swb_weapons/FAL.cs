using System.Collections.Generic;

using Sandbox;

using SWB_Base;

using TTTReborn.Globalization;
using TTTReborn.Items;
using TTTReborn.Player;
using TTTReborn.UI;

namespace SWB_WEAPONS
{
    [Library("swb_fal", Title = "FN FAL")]
    public class FAL : WeaponBase, ICarriableItem, IEntityHint
    {
        public SlotType SlotType { get; } = SlotType.Primary;
        public new bool CanDrop() => true;
        public string LibraryName { get; }
        public void Equip(TTTPlayer player) { }
        public void OnEquip() { }
        public void Remove() { }
        public void OnRemove() { }
        public bool CanHint(TTTPlayer client)
        {
            return true;
        }

        public TranslationData TextOnTick => new("GENERIC_PICKUP", Input.GetKeyWithBinding("+iv_use").ToUpper(), new TranslationData(LibraryName.ToUpper()));
        public EntityHintPanel DisplayHint(TTTPlayer client)
        {
            return new Hint(TextOnTick);
        }
        public void Tick(TTTPlayer player)
        {
            if (IsClient)
            {
                return;
            }

            if (player.LifeState != LifeState.Alive)
            {
                return;
            }

            using (Prediction.Off())
            {
                if (Input.Pressed(InputButton.Use))
                {
                    if (player.Inventory.Active is ICarriableItem carriable && carriable.SlotType == SlotType)
                    {
                        player.Inventory.DropActive();
                    }

                    player.Inventory.TryAdd(this, deleteIfFails: false, makeActive: true);
                }
            }
        }


        public override int Bucket => 3;
        public override HoldType HoldType => HoldType.Rifle;
        public override string HandsModelPath => "weapons/swb/hands/rebel/v_hands_rebel.vmdl";
        public override string ViewModelPath => "weapons/swb/rifles/fal/v_fal.vmdl";
        public override AngPos ViewModelOffset => new()
        {
            Angle = new Angles(0, -5, 0),
            Pos = new Vector3(-5, 0, 0)
        };
        public override string WorldModelPath => "weapons/swb/rifles/fal/w_fal.vmdl";
        public override string Icon => "/swb_weapons/textures/fal.png";
        public override int FOV => 75;
        public override int ZoomFOV => 75;
        public override float WalkAnimationSpeedMod => 0.85f;

        public FAL()
        {
            General = new WeaponInfo
            {
                DrawTime = 1f,
                ReloadTime = 2.03f,
                ReloadEmptyTime = 2.67f
            };

            Primary = new ClipInfo
            {
                Ammo = 20,
                AmmoType = AmmoType.Rifle,
                ClipSize = 20,

                BulletSize = 4f,
                Damage = 20f,
                Force = 3f,
                Spread = 0.1f,
                Recoil = 0.5f,
                RPM = 600,
                FiringType = FiringType.semi,
                ScreenShake = new ScreenShake
                {
                    Length = 0.5f,
                    Speed = 4.0f,
                    Size = 0.5f,
                    Rotation = 0.5f
                },

                DryFireSound = "swb_rifle.empty",
                ShootSound = "fal.fire",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",

                InfiniteAmmo = InfiniteAmmoType.reserve
            };

            ZoomAnimData = new AngPos
            {
                Angle = new Angles(-0.1f, 4.95f, -1f),
                Pos = new Vector3(-5f, -4.211f, 0.75f)
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };

            CustomizeAnimData = new AngPos
            {
                Angle = new Angles(-2.25f, 51.84f, 0f),
                Pos = new Vector3(11.22f, -4.96f, 1.078f)
            };

            // Attachments //
            AttachmentCategories = new List<AttachmentCategory>()
            {
                new AttachmentCategory
                {
                    Name = AttachmentCategoryName.Muzzle,
                    BoneOrAttachment = "muzzle",
                    Attachments = new List<AttachmentBase>()
                    {
                        new RifleSilencer
                        {
                            Enabled = false,
                            MuzzleFlashParticle = "particles/swb/muzzle/flash_medium_silenced.vpcf",
                            ShootSound = "swb_rifle.silenced.fire",
                            ViewParentBone = "fal",
                            ViewTransform = new Transform {
                                Position = new Vector3(0.019f, 3.65f, 38.057f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 90f)),
                                Scale = 15f
                            },
                            WorldParentBone = "fal",
                            WorldTransform = new Transform {
                                Position = new Vector3(0.019f, 1.8f, 38.057f),
                                Rotation = Rotation.From(new Angles(-90f, 0f, 90f)),
                                Scale = 15f
                            },
                        }
                    }
                }
            };
        }
    }
}
