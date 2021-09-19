using Sandbox;
using Sandbox.UI;

using TTTReborn.Player;

namespace TTTReborn.UI
{
    public partial class Hud : HudEntity<RootPanel>
    {
        public static Hud Current { set; get; }

        public AliveHud AliveHudPanel;
        public GeneralHud GeneralHudPanel;

        public Hud()
        {
            if (!IsClient)
            {
                return;
            }

            AliveHudPanel = RootPanel.AddChild<AliveHud>();
            GeneralHudPanel = RootPanel.AddChild<GeneralHud>();
            Current = this;
        }

        [Event.Hotload]
        public static void OnHotReloaded()
        {
            if (Host.IsClient)
            {
                Local.Hud?.Delete();

                Hud hud = new();

                if (Local.Client.Pawn is TTTPlayer player && player.LifeState == LifeState.Alive)
                {
                    hud.AliveHudPanel.SetChildrenEnabled(true);
                }
            }
        }

        [Event("tttreborn.player.spawned")]
        private void OnPlayerSpawned(TTTPlayer player)
        {
            if (player != Local.Client.Pawn)
            {
                return;
            }

            AliveHudPanel.SetChildrenEnabled(true);
        }

        [Event("tttreborn.player.died")]
        private void OnPlayerDied(TTTPlayer deadPlayer)
        {
            if (deadPlayer != Local.Client.Pawn)
            {
                return;
            }

            AliveHudPanel.SetChildrenEnabled(false);
        }

        public class GeneralHud : Panel
        {
            public GeneralHud() : base()
            {
                AddClass("fullscreen");
                AddChild<RadarDisplay>();
                AddChild<PlayerRoleDisplay>();
                AddChild<PlayerInfoDisplay>();
                AddChild<InventoryWrapper>();
                AddChild<ChatBox>();

                AddChild<VoiceChatDisplay>();
                AddChild<GameTimerDisplay>();

                AddChild<VoiceList>();

                AddChild<InfoFeed>();
                AddChild<InspectMenu>();
                AddChild<PostRoundMenu>();
                AddChild<Scoreboard>();
                AddChild<Menu.Menu>();
            }
        }

        public class AliveHud : Panel
        {
            public AliveHud() : base()
            {
                AddClass("fullscreen");
                AddChild<Crosshair>();
                AddChild<DrowningIndicator>();
                AddChild<QuickShop>();
                AddChild<DamageIndicator>();
            }
        }
    }
}
