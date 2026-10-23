using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// Zeichnen gehoert zu einem Zug, der auf dem Canvas begonnen hat. Ein Druck auf
    /// die Toolbar, gehalten und auf das Canvas gezogen, hat nie eine Zelle gewaehlt
    /// und darf keine setzen: die gedrueckte Taste allein ist kein Zug.
    ///
    /// Die echten MouseEventArgs sind nicht baubar (Position und Tastenzustand kommen
    /// vom echten Geraet), darum fahren die Tests die Entscheidung mit dem Zustand,
    /// den die Handler lesen.
    /// </summary>
    public class DragOriginTests
    {
        [Fact]
        public void AMoveWithoutAPressOnTheCanvasPaintsNothing()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("HandlePointerMove", probe.Pixel(5, 3), true, false);

                Assert.Empty(probe.LiveCells);
            });
        }

        [Fact]
        public void AMoveWithoutAPressOnTheCanvasErasesNothing()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("HandlePointerDown", probe.Pixel(5, 3), MouseButton.Left);
                probe.Call("Canvas_MouseUp", null, null);

                probe.Call("HandlePointerMove", probe.Pixel(5, 3), false, true);

                Assert.Contains(new Point(5, 3), probe.LiveCells);
            });
        }

        [Fact]
        public void ADragThatStartedOnTheCanvasPaintsTheLine()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("HandlePointerDown", probe.Pixel(3, 3), MouseButton.Left);
                probe.Call("HandlePointerMove", probe.Pixel(5, 3), true, false);

                Assert.Equal(
                    new HashSet<Point> { new Point(3, 3), new Point(4, 3), new Point(5, 3) },
                    probe.LiveCells);
            });
        }

        [Fact]
        public void ReleasingTheButtonEndsTheDrag()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("HandlePointerDown", probe.Pixel(3, 3), MouseButton.Left);
                probe.Call("Canvas_MouseUp", null, null);

                probe.Call("HandlePointerMove", probe.Pixel(8, 8), true, false);

                Assert.Equal(new HashSet<Point> { new Point(3, 3) }, probe.LiveCells);
            });
        }

        [Fact]
        public void LosingTheMouseCaptureEndsTheDrag()
        {
            WindowProbe.Run(1000, 620, probe =>
            {
                probe.Call("HandlePointerDown", probe.Pixel(3, 3), MouseButton.Left);
                probe.Call("Canvas_LostMouseCapture", null, null);

                probe.Call("HandlePointerMove", probe.Pixel(8, 8), true, false);

                Assert.Equal(new HashSet<Point> { new Point(3, 3) }, probe.LiveCells);
            });
        }
    }
}
