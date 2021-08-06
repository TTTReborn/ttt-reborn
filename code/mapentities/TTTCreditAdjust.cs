using Sandbox;

using TTTReborn.Player;

namespace TTTReborn.MapEntities
{
    [Library("ttt_credit_adjust", Description = "Changes the amount of credits upon the activator.")]
    public partial class TTTChangeCredits : Entity
    {
        [Property("Credits", "Removes amount of credits. Use a negative number to add credits to the activator.")]
        public int Credits { get; set; } = 0;

        [Input]
        public void ExchangeCredits(Entity activator)
        {

            if (activator is TTTPlayer player)
            {

                if (player.Credits >= Credits)
                {
                    player.Credits -= Credits;

                    OnSuccess.Fire(activator);
                }
                else
                {
                    OnFailure.Fire(activator);
                }
            }
        }

        /// <summary>
        /// Fires when credits are successfully added or removed from activator.
        /// </summary>
        protected Output OnSuccess { get; set; }

        /// <summary>
        /// Fires if credits cannot be removed or added to activator. Such as not having enough credits for removal as a player cannot have 'negative' credits.
        /// </summary>
        protected Output OnFailure { get; set; }
    }
}
