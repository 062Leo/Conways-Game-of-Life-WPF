using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// A prefab is placed as a whole or not at all. Dropping one near the right or
    /// bottom edge used to cut off the cells that no longer fit, so the user got a
    /// fragment of the pattern they picked.
    /// </summary>
    public class StampPlacementTests
    {
        // Glider inside its 3x3 box.
        private static readonly List<(int X, int Y)> Glider = new()
        {
            (1, 0),
            (2, 1),
            (0, 2), (1, 2), (2, 2)
        };

        [Fact]
        public void PlacesEveryCellOfTheStampAwayFromTheEdges()
        {
            var cells = LifeRules.StampCells(5, 5, Glider, 33, 19);

            Assert.Equal(new HashSet<Point>
            {
                new Point(6, 5),
                new Point(7, 6),
                new Point(5, 7), new Point(6, 7), new Point(7, 7)
            }, new HashSet<Point>(cells));
        }

        [Fact]
        public void PullsTheStampBackFromTheRightEdge()
        {
            var cells = LifeRules.StampCells(32, 5, Glider, 33, 19);

            Assert.Equal(Glider.Count, cells.Count);
            Assert.Equal(new HashSet<Point>
            {
                new Point(31, 5),
                new Point(32, 6),
                new Point(30, 7), new Point(31, 7), new Point(32, 7)
            }, new HashSet<Point>(cells));
        }

        [Fact]
        public void PullsTheStampBackFromTheBottomRightCorner()
        {
            var cells = LifeRules.StampCells(32, 18, Glider, 33, 19);

            Assert.Equal(Glider.Count, cells.Count);
            foreach (var cell in cells)
            {
                Assert.True(LifeRules.IsWithinGrid((int)cell.X, (int)cell.Y, 33, 19));
            }
            Assert.Contains(new Point(30, 18), cells);
            Assert.Contains(new Point(31, 16), cells);
        }

        [Fact]
        public void CutsOffOnlyAStampThatIsLargerThanTheGrid()
        {
            // Nothing can make a 3x3 stamp fit on a 2x2 grid: it sits in the corner
            // and the rest falls off.
            var cells = LifeRules.StampCells(1, 1, Glider, 2, 2);

            Assert.Equal(new HashSet<Point> { new Point(1, 0) }, new HashSet<Point>(cells));
        }

        [Fact]
        public void PlacesNothingWithoutAStamp()
        {
            Assert.Empty(LifeRules.StampCells(5, 5, new List<(int X, int Y)>(), 33, 19));
        }
    }
}
