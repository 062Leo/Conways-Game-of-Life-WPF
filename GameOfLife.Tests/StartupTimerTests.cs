using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// The speed slider's ValueChanged fires while InitializeComponent is still
    /// parsing the XAML, long before the constructor body runs. Any line in that
    /// handler that touches the simulation timer therefore has to find the timer
    /// already there, and this test only passes if it does: building the window at
    /// all runs the handler, and the handler writes the interval every time.
    /// </summary>
    public class StartupTimerTests
    {
        [Fact]
        public void TheSimulationTimerExistsBeforeTheSliderCanTouchIt()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                Assert.NotNull(probe.Field<DispatcherTimer>("simulationTimer"));
                Assert.NotNull(probe.Field<DispatcherTimer>("zoomRedrawTimer"));
            });
        }

        [Fact]
        public void TheSliderSetsTheTimerIntervalWhileTheSimulationIsStopped()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Field<Slider>("SpeedSlider").Value = 4;

                Assert.Equal(
                    TimeSpan.FromMilliseconds(60),
                    probe.Field<DispatcherTimer>("simulationTimer").Interval);
            });
        }

        [Fact]
        public void TheSliderSetsTheTimerIntervalWhileTheSimulationRuns()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("StartSimulation");
                probe.Field<Slider>("SpeedSlider").Value = 0;

                Assert.Equal(
                    TimeSpan.FromMilliseconds(600),
                    probe.Field<DispatcherTimer>("simulationTimer").Interval);
            });
        }
    }
}
