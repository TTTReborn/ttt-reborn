using System;
using System.Text.Json.Serialization;

using Sandbox;

namespace TTTReborn
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GameEventAttribute : LibraryAttribute
    {
        public GameEventAttribute(string name) : base($"ttt_gameevent_{name}".ToLower()) { }
    }

    public class EventAttribute : Sandbox.EventAttribute
    {
        public EventAttribute(Type type) : base(Utils.GetAttribute<GameEventAttribute>(type).Name) { }
    }

    // currently (issues with [Net] reassignments and tons of transmitted objects) it's not valuable to make it BaseNetworkable, Transmit always and get rid of NetworkableGameEvent
    // that's why we are using our own networking stuff here on demand
    public abstract partial class GameEvent
    {
        public string Name { get; set; }

        public float CreatedAt { get; set; }

        [JsonIgnore]
        public GameEventScoring[] Scoring { get; set; } = Array.Empty<GameEventScoring>();

        public GameEvent()
        {
            GameEventAttribute attribute = Utils.GetAttribute<GameEventAttribute>(GetType());

            if (attribute != null)
            {
                Name = attribute.Name;
            }

            CreatedAt = Time.Now;
        }

        public virtual void Run() => Event.Run(Name);

        protected virtual void OnRegister()
        {
            foreach (GameEventScoring gameEventScoring in Scoring)
            {
                gameEventScoring.Init(this);
            }
        }

        internal void ProcessRegister()
        {
            if (Host.IsServer)
            {
                Gamemode.Game.Instance.Round?.GameEvents.Add(this);

                OnRegister();
            }
        }

        public static void Register<T>(T gameEvent, params GameEventScoring[] gameEventScorings) where T : GameEvent
        {
            gameEvent.Scoring = gameEventScorings ?? gameEvent.Scoring;

            gameEvent.ProcessRegister();
            gameEvent.Run();
        }

        public virtual Sandbox.UI.Panel GetEventPanel() => new UI.EventPanel(this);
    }

    public partial class GameEventScoring
    {
        public int Score { get; set; } = 0;
        public int Karma { get; set; } = 0;
        public Player Player { get; set; }

        public bool IsInitialized { get; set; } = false;

        public virtual void Init<T>(T gameEvent) where T : GameEvent
        {
            IsInitialized = true;
        }

        public virtual void Evaluate()
        {
            if (Player != null && Player.IsValid)
            {
                Player.Client.SetInt("score", Player.Client.GetInt("score") + Score);
                Player.Client.SetInt("karma", Player.Client.GetInt("karma") + Karma);
            }
        }

        public GameEventScoring(Player player)
        {
            Player = player;
        }
    }
}
