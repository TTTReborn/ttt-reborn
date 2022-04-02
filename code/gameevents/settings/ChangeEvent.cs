namespace TTTReborn.Events.Settings
{
    [GameEvent("settings_change"), Hammer.Skip]
    public partial class ChangeEvent : GameEvent
    {
        /// <summary>
        /// Occurs when server or client settings are changed.
        /// </summary>
        public ChangeEvent() : base() { }
    }
}
