using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// A drag draws a line from the cell the pointer was last on to the cell it is
    /// on now. That anchor is a grid coordinate, so it has to go when the grid moves
    /// under the pointer: a window resize or a zoom step mid drag would otherwise
    /// draw a line from wherever the pointer used to be, right across the grid.
    /// </summary>
    public class DragAnchorTests
    {
        [Fact]
        public void ForgetsTheDragAnchorWhenAResizeChangesTheGrid()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.SetField("lastCell", new Point(30, 18));

                probe.Layout(200, 200);

                Assert.Null(probe.Field<Point?>("lastCell"));
            });
        }

        [Fact]
        public void ForgetsTheDragAnchorWhenTheZoomChangesTheGrid()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.SetField("lastCell", new Point(30, 18));

                probe.Field<Slider>("ZoomSlider").Value = 30;
                probe.Call("ZoomRedrawTimer_Tick", null, null);

                Assert.Null(probe.Field<Point?>("lastCell"));
            });
        }
    }
}
