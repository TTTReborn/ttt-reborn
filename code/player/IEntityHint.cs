using TTTReborn.Globalization;
using TTTReborn.UI;

namespace TTTReborn
{
    public interface IEntityHint
    {
        /// <summary>
        /// The max viewable distance of the hint.
        /// </summary>
        float HintDistance => 2048f;

        /// <summary>
        /// If we should show a glow around the entity.
        /// </summary>
        bool ShowGlow => true;

        /// <summary>
        /// The text to display on the hint each tick.
        /// </summary>
        TranslationData TextOnTick => null;

        /// <summary>
        /// Whether or not we can show the UI hint.
        /// </summary>
        bool CanHint(Player client);

        /// <summary>
        /// The hint we should display.
        /// </summary>
        EntityHintPanel DisplayHint(Player client);

        /// <summary>
        /// Occurs on each tick if the hint is active.
        /// </summary>
        void TextTick(Player player);
    }
}
