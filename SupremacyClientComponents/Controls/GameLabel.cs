using System.Windows;
using System.Windows.Markup;

namespace Supremacy.Client.Controls
{
    [ContentProperty("Label")]
    public class GameLabel : GameControlBase
    {
        static GameLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameLabel),
                new FrameworkPropertyMetadata(typeof(GameLabel)));
        }
    }
}