using System.Windows;
using GameOfLife;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// Holds Conway's rules, B3/S23 on a bounded grid. The patterns used here
    /// are the standard ones with known behaviour, so a wrong rule cannot pass:
    /// a block never moves, a blinker has period two, a glider travels one cell
    /// diagonally every four generations.
    /// </summary>
    public class LifeRulesTests
    {
        private const int Width = 20;
        private const int Height = 20;

        private static HashSet<Point> Cells(params (int x, int y)[] cells)
        {
            var set = new HashSet<Point>();
            foreach (var (x, y) in cells)
            {
                set.Add(new Point(x, y));
            }
            return set;
        }

        private static HashSet<Point> Step(HashSet<Point> cells, int generations = 1)
        {
            var current = cells;
            for (int i = 0; i < generations; i++)
            {
                current = LifeRules.Step(current, Width, Height);
            }
            return current;
        }

        [Fact]
        public void EmptyGridStaysEmpty()
        {
            Assert.Empty(Step(Cells()));
        }

        [Fact]
        public void LoneCellDiesOfUnderpopulation()
        {
            Assert.Empty(Step(Cells((5, 5))));
        }

        [Fact]
        public void PairDiesOfUnderpopulation()
        {
            Assert.Empty(Step(Cells((5, 5), (5, 6))));
        }

        [Fact]
        public void BlockIsAStillLife()
        {
            var block = Cells((5, 5), (6, 5), (5, 6), (6, 6));
            Assert.Equal(block, Step(block));
            Assert.Equal(block, Step(block, 10));
        }

        [Fact]
        public void BlinkerOscillatesWithPeriodTwo()
        {
            var horizontal = Cells((4, 5), (5, 5), (6, 5));
            var vertical = Cells((5, 4), (5, 5), (5, 6));

            Assert.Equal(vertical, Step(horizontal));
            Assert.Equal(horizontal, Step(horizontal, 2));
        }

        [Fact]
        public void CellWithFourNeighboursDiesOfOvercrowding()
        {
            // Centre has four live neighbours, so it dies even though it is alive.
            var cross = Cells((5, 5), (4, 5), (6, 5), (5, 4), (5, 6));
            Assert.DoesNotContain(new Point(5, 5), Step(cross));
        }

        [Fact]
        public void DeadCellWithExactlyThreeNeighboursIsBorn()
        {
            var threeNeighbours = Cells((4, 4), (5, 4), (4, 5));
            Assert.Contains(new Point(5, 5), Step(threeNeighbours));
        }

        [Fact]
        public void DeadCellWithTwoNeighboursStaysDead()
        {
            var twoNeighbours = Cells((4, 4), (5, 4));
            Assert.DoesNotContain(new Point(5, 5), Step(twoNeighbours));
        }

        [Fact]
        public void GliderMovesOneCellDiagonallyEveryFourGenerations()
        {
            var glider = Cells((2, 1), (3, 2), (1, 3), (2, 3), (3, 3));
            var moved = Cells((3, 2), (4, 3), (2, 4), (3, 4), (4, 4));

            Assert.Equal(moved, Step(glider, 4));
        }

        [Fact]
        public void GliderKeepsItsCellCountWhileTravelling()
        {
            var glider = Cells((2, 1), (3, 2), (1, 3), (2, 3), (3, 3));
            for (int generation = 1; generation <= 16; generation++)
            {
                Assert.Equal(5, Step(glider, generation).Count);
            }
        }

        [Fact]
        public void NothingIsBornOutsideTheGrid()
        {
            // Three cells in the corner would give birth at (-1, 0) on an
            // infinite plane. The grid is bounded, so that cell cannot exist.
            var corner = Cells((0, 0), (0, 1), (1, 0));
            var next = Step(corner);

            Assert.All(next, cell =>
                Assert.True(LifeRules.IsWithinGrid((int)cell.X, (int)cell.Y, Width, Height)));
        }

        [Fact]
        public void EdgeCellsLoseTheNeighboursOutsideTheGrid()
        {
            // A blinker lying against the top edge: on an infinite plane it
            // would oscillate, here the missing row changes nothing for it,
            // but the cell above the centre cannot be born outside the grid.
            var blinker = Cells((4, 0), (5, 0), (6, 0));
            var next = Step(blinker);

            Assert.Contains(new Point(5, 0), next);
            Assert.Contains(new Point(5, 1), next);
            Assert.DoesNotContain(new Point(4, 0), next);
            Assert.DoesNotContain(new Point(6, 0), next);
        }

        [Fact]
        public void StepDoesNotModifyTheInput()
        {
            var blinker = Cells((4, 5), (5, 5), (6, 5));
            var before = new HashSet<Point>(blinker);

            LifeRules.Step(blinker, Width, Height);

            Assert.Equal(before, blinker);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(10, 0)]
        [InlineData(-1, 10)]
        public void DegenerateGridProducesNothing(int width, int height)
        {
            Assert.Empty(LifeRules.Step(Cells((1, 1), (2, 1), (3, 1)), width, height));
        }
    }
}
