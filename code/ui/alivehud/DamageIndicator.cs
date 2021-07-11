using System;

using Sandbox;
using Sandbox.UI;

using TTTReborn.Player;

namespace TTTReborn.UI
{
    public partial class DamageIndicator : Panel
    {
        private float _maxDamageIndicatorTime = 10f;
        private float _currentRemainingDamageIndicatorTime = 0f;
        private TimeSince _timeSinceLastDamage = 0f;
        private float _lastDamage = 0f;
        private float _additionalDamageIndicatorTime = 0f;

        public DamageIndicator()
        {
            StyleSheet.Load("/ui/alivehud/DamageIndicator.scss");

            Style.SetBackgroundImage(Texture.Load("/ui/damageindicator/default.png"));
        }

        [Event("tttreborn.player.takedamage")]
        private void OnTakeDamage(TTTPlayer player, float damage)
        {
            if (Host.IsServer)
            {
                return;
            }

            _lastDamage = damage;
            _timeSinceLastDamage = 0f;
            _additionalDamageIndicatorTime += _currentRemainingDamageIndicatorTime;
            _currentRemainingDamageIndicatorTime = 0f;
        }

        public override void Tick()
        {
            if (Local.Pawn is not TTTPlayer player)
            {
                return;
            }

            float remainingDamageIndicatorTime = _maxDamageIndicatorTime * (_lastDamage / player.MaxHealth);

            if (_additionalDamageIndicatorTime != 0f)
            {
                remainingDamageIndicatorTime += _additionalDamageIndicatorTime;
            }

            if (_lastDamage > 0f && _timeSinceLastDamage < remainingDamageIndicatorTime)
            {
                _currentRemainingDamageIndicatorTime = remainingDamageIndicatorTime - _timeSinceLastDamage;

                Style.Display = DisplayMode.Flex;
                Style.Opacity = Math.Clamp((_currentRemainingDamageIndicatorTime / remainingDamageIndicatorTime) * (remainingDamageIndicatorTime / _maxDamageIndicatorTime), 0f, 1f);
                Style.Dirty();
            }
            else
            {
                _currentRemainingDamageIndicatorTime = 0f;

                Style.Display = DisplayMode.None;
            }
        }
    }
}
