using Sandbox;

namespace TTTReborn.Items
{
    [Library("ttt_ammo_buckshot")]
    [Hammer.EditorModel("models/ammo/ammo_buckshot.vmdl")]
    partial class BuckshotAmmo : TTTAmmo
    {
        public override string Name => "buckshot";
        public override int Amount => 12;
        public override int Max => 36;
        public override string ModelPath => "models/ammo/ammo_buckshot.vmdl";
    }
}
