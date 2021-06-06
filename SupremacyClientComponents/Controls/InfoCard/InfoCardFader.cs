using System.Windows;
using System.Windows.Media.Animation;

namespace Supremacy.Client.Controls
{
    internal class InfoCardFader
    {
        private InfoCard _infoCard;
        private Storyboard _storyboard;

        private static Storyboard CreateStoryboard(Duration delayDuration, Duration fadeDuration, double targetOpacity)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation opacityAnimation = new DoubleAnimation(targetOpacity, fadeDuration)
                                   {
                                       BeginTime = delayDuration.TimeSpan
                                   };
            Storyboard.SetTargetProperty(
                opacityAnimation, 
                new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnimation);
            return storyboard;
        }

        private void DestroyStoryboard()
        {
            if (_storyboard == null)
                return;
            InfoCard infoCardElement = _infoCard;
            if (infoCardElement != null)
            {
                if (_storyboard.GetCurrentState(infoCardElement) != ClockState.Stopped)
                    _storyboard.Stop(infoCardElement);
            }

            _storyboard = null;
        }

        internal void StartFade(InfoCard infoCardToFade)
        {
            DestroyStoryboard();

            FrameworkElement infoCardElement = infoCardToFade as FrameworkElement;
            if (infoCardElement == null)
                return;

            InfoCardSite infoCardSite = infoCardToFade.InfoCardSite;
            if ((infoCardSite == null) || 
                !infoCardSite.IsInactiveInfoCardFadeEnabled || 
                (infoCardSite.InactiveInfoCardFadeOpacity == 1.0))
            {
                return;
            }

            _infoCard = infoCardToFade;

            _storyboard = CreateStoryboard(
                infoCardSite.InactiveInfoCardFadeDelay,
                infoCardSite.InactiveInfoCardFadeDuration,
                infoCardSite.InactiveInfoCardFadeOpacity);

            _storyboard.Begin(infoCardElement, true);
        }

        internal void StopFade()
        {
            DestroyStoryboard();
        }
    }
}