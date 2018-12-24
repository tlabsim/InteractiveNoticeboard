using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace InteractiveNoticeboard
{
    public partial class AnimatedControlsStylesHandlers
    {
        public double SpeedRatio = 1.0;

        public void ForeverBlinkerLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                System.Windows.Media.Animation.QuadraticEase QEaseOut = new System.Windows.Media.Animation.QuadraticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                System.Windows.Media.Animation.Storyboard SB = new System.Windows.Media.Animation.Storyboard();
                SB.RepeatBehavior = RepeatBehavior.Forever;
                SB.Duration = System.TimeSpan.FromMilliseconds(500 * SpeedRatio);

                System.Windows.Media.Animation.DoubleAnimation fadeout = new System.Windows.Media.Animation.DoubleAnimation(0, System.TimeSpan.FromMilliseconds(100 * SpeedRatio));
                fadeout.BeginTime = new System.TimeSpan(0, 0, 0, 0, 0);
                fadeout.EasingFunction = QEaseOut;
                System.Windows.Media.Animation.Storyboard.SetTarget(fadeout, fe);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeout, new PropertyPath(TextBlock.OpacityProperty));
                SB.Children.Add(fadeout);

                System.Windows.Media.Animation.DoubleAnimation fadein = new System.Windows.Media.Animation.DoubleAnimation(1, System.TimeSpan.FromMilliseconds(100 * SpeedRatio));
                fadein.BeginTime = new System.TimeSpan(0, 0, 0, 0, 200);
                fadein.EasingFunction = QEaseOut;
                System.Windows.Media.Animation.Storyboard.SetTarget(fadein, fe);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadein, new PropertyPath(TextBlock.OpacityProperty));
                SB.Children.Add(fadein);

                SB.Begin();
            }
        }

    }
}
