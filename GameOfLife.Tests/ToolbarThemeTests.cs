using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// The toolbar has a surface, so it follows the theme like everything else. The
    /// trap is half a fix: a dark background alone leaves every label at its
    /// inherited black and the toolbar is unreadable instead of merely bright.
    ///
    /// The assertions are about contrast, not about hex values, so the palette can
    /// change without touching the test while black on dark grey still fails.
    /// </summary>
    public class ToolbarThemeTests
    {
        /// <summary>WCAG 2.1 threshold for normal body text.</summary>
        private const double ReadableContrast = 4.5;

        [Fact]
        public void DarkThemeDrivesTheToolbarBackgroundFromTheThemeResource()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.SetField("isDarkMode", true);
                probe.Call("ApplyTheme");

                var toolbar = probe.Field<ToolBar>("MainToolBar");

                Assert.Equal(
                    ((SolidColorBrush)probe.Window.Resources["ToolbarColor"]).Color,
                    ((SolidColorBrush)toolbar.Background).Color);
            });
        }

        [Fact]
        public void EveryToolbarLabelStaysReadableInDarkMode()
        {
            AssertToolbarReadable(true);
        }

        [Fact]
        public void EveryToolbarLabelStaysReadableInLightMode()
        {
            AssertToolbarReadable(false);
        }

        private static void AssertToolbarReadable(bool darkMode)
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.SetField("isDarkMode", darkMode);
                probe.Call("ApplyTheme");

                var toolbar = probe.Field<ToolBar>("MainToolBar");

                foreach (var element in Descendants(toolbar))
                {
                    // A disabled control is greyed out by the platform on purpose,
                    // and contrast rules exempt it. Nothing else is exempt.
                    if (!element.IsEnabled || !TryGetForeground(element, out Color foreground))
                    {
                        continue;
                    }

                    Color background = EffectiveBackground(element, toolbar, foreground);
                    double contrast = Contrast(foreground, background);

                    Assert.True(
                        contrast >= ReadableContrast,
                        $"{Describe(element)}: {foreground} on {background} is {contrast:0.00}:1, " +
                        $"needs {ReadableContrast}:1");
                }
            });
        }

        private static IEnumerable<FrameworkElement> Descendants(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is not DependencyObject node)
                {
                    continue;
                }

                if (node is FrameworkElement element)
                {
                    yield return element;
                }

                foreach (var nested in Descendants(node))
                {
                    yield return nested;
                }
            }
        }

        /// <summary>
        /// The foreground that actually paints this element's text or tick marks.
        /// Elements that paint neither are skipped.
        /// </summary>
        private static bool TryGetForeground(FrameworkElement element, out Color foreground)
        {
            foreground = default;

            Brush? brush = element switch
            {
                TextBlock text => text.Foreground,
                Slider slider => slider.Foreground,
                Control control => control.Foreground,
                _ => null
            };

            if (brush is not SolidColorBrush solid)
            {
                return false;
            }

            foreground = solid.Color;
            return true;
        }

        /// <summary>
        /// The nearest ancestor background that is actually painted, the toolbar's
        /// own being the last resort. A control carrying its own chrome, such as the
        /// combo box with the platform's gradient, is judged against that chrome and
        /// not against the toolbar behind it; a gradient is taken at its worst stop.
        /// </summary>
        private static Color EffectiveBackground(FrameworkElement element, ToolBar toolbar, Color foreground)
        {
            for (DependencyObject? node = element; node != null; node = LogicalTreeHelper.GetParent(node))
            {
                Brush? brush = node switch
                {
                    Control control => control.Background,
                    Panel panel => panel.Background,
                    _ => null
                };

                if (brush is SolidColorBrush solid && solid.Color.A > 0)
                {
                    return solid.Color;
                }

                if (brush is GradientBrush gradient && gradient.GradientStops.Count > 0)
                {
                    Color worst = gradient.GradientStops[0].Color;
                    foreach (var stop in gradient.GradientStops)
                    {
                        if (Contrast(foreground, stop.Color) < Contrast(foreground, worst))
                        {
                            worst = stop.Color;
                        }
                    }
                    return worst;
                }
            }

            return ((SolidColorBrush)toolbar.Background).Color;
        }

        private static double Contrast(Color a, Color b)
        {
            double first = RelativeLuminance(a);
            double second = RelativeLuminance(b);
            double lighter = Math.Max(first, second);
            double darker = Math.Min(first, second);
            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double RelativeLuminance(Color color)
        {
            return 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        }

        private static double Linear(byte channel)
        {
            double value = channel / 255.0;
            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        private static string Describe(FrameworkElement element)
        {
            string name = string.IsNullOrEmpty(element.Name) ? string.Empty : $" '{element.Name}'";
            string content = element switch
            {
                TextBlock text => $" \"{text.Text}\"",
                ContentControl control when control.Content is string label => $" \"{label}\"",
                _ => string.Empty
            };

            return element.GetType().Name + name + content;
        }
    }
}
