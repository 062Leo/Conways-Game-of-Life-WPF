using System.Windows.Controls;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// The zoom slider counts upwards to the right while the cell size in pixels
    /// runs the other way: further right means smaller cells, so more of them fit.
    /// The slider's range says how finely that is divided, not how large a cell may
    /// get, so widening the range must not push the cell size out of its bounds, and
    /// a slider that cannot move at all still has to yield a usable cell size.
    /// </summary>
    public class CellSizeTests
    {
        [Fact]
        public void TheLeftEndOfTheSliderGivesTheLargestCells()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                var slider = probe.Field<Slider>("ZoomSlider");
                slider.Value = slider.Minimum;

                Assert.Equal(30, probe.Field<double>("cellSize"));
            });
        }

        [Fact]
        public void TheRightEndOfTheSliderGivesTheSmallestCells()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                var slider = probe.Field<Slider>("ZoomSlider");
                slider.Value = slider.Maximum;

                Assert.Equal(13, probe.Field<double>("cellSize"));
            });
        }

        [Fact]
        public void AWiderSliderRangeKeepsTheCellSizeWithinItsPixelBounds()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                var slider = probe.Field<Slider>("ZoomSlider");
                slider.Maximum = 100;
                slider.Minimum = 0;

                slider.Value = 0;
                Assert.Equal(30, probe.Field<double>("cellSize"));

                slider.Value = 100;
                Assert.Equal(13, probe.Field<double>("cellSize"));

                slider.Value = 50;
                double middle = probe.Field<double>("cellSize");
                Assert.InRange(middle, 13, 30);
            });
        }

        [Fact]
        public void ASliderThatCannotMoveStillYieldsAUsableCellSize()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                var slider = probe.Field<Slider>("ZoomSlider");
                slider.Minimum = 5;
                slider.Maximum = 5;

                probe.Call("UpdateCellSizeFromSlider");

                Assert.InRange(probe.Field<double>("cellSize"), 13, 30);
            });
        }
    }
}
