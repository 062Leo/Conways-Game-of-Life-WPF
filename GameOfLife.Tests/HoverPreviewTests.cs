using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// The hover preview promises where the next click will land. It is kept in grid
    /// coordinates, so a grid that changes under the pointer, by a resize or by the
    /// zoom slider, leaves the promise pointing at a cell that is no longer there.
    /// </summary>
    public class HoverPreviewTests
    {
        [Fact]
        public void DropsTheHoverPreviewWhenAResizeShrinksTheGrid()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("UpdateHoverCells", new Point(30, 18));
                Assert.NotEmpty(probe.Field<List<Point>>("hoverCells"));

                probe.Layout(200, 200);

                AssertPreviewIsOnTheGrid(probe);
            });
        }

        [Fact]
        public void DropsTheHoverPreviewWhenTheZoomChangesTheGrid()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("UpdateHoverCells", new Point(30, 18));
                Assert.NotEmpty(probe.Field<List<Point>>("hoverCells"));

                probe.Field<Slider>("ZoomSlider").Value = 30; // smaller cells, so a wider grid
                probe.Call("ZoomRedrawTimer_Tick", null, null);

                AssertPreviewIsOnTheGrid(probe);
            });
        }

        private static void AssertPreviewIsOnTheGrid(WindowProbe probe)
        {
            int gridWidth = probe.Field<int>("GridWidth");
            int gridHeight = probe.Field<int>("GridHeight");

            Assert.All(probe.Field<List<Point>>("hoverCells"), cell =>
                Assert.True(LifeRules.IsWithinGrid((int)cell.X, (int)cell.Y, gridWidth, gridHeight),
                    $"preview cell {cell} is outside the {gridWidth}x{gridHeight} grid"));
            Assert.Null(probe.Field<Point?>("hoverCenterCell"));
        }
    }
}
