using System.ComponentModel.DataAnnotations;

using Sandbox;

namespace TTTReborn.Items
{
    [Library("ttt_weapon_knife")]
    [Weapon(CarriableCategories.Melee)]
    [Buyable(Price = 100)]
    [Precached("weapons/rust_boneknife/v_rust_boneknife.vmdl", "weapons/rust_boneknife/rust_boneknife.vmdl")]
    [EditorModel("weapons/rust_boneknife/rust_boneknife.vmdl")]
    [Display(Name = "Knife", GroupName = "Weapons")]
    [Title("Knife")]
    public partial class Knife : MeleeWeapon
    {
        public override string ViewModelPath => "weapons/rust_boneknife/v_rust_boneknife.vmdl";
        public override string ModelPath => "weapons/rust_boneknife/rust_boneknife.vmdl";

        public override WeaponInfo WeaponInfo { get; set; } = new()
        {
            DeployTime = 0.2f
        };

        public override ClipInfo[] ClipInfos { get; set; } = new ClipInfo[]
        {
            new()
            {
                Damage = 45,
                RPM = 60
            }
        };
    }
}
