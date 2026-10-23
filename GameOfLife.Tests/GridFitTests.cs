using GameOfLife;
using Xunit;

namespace GameOfLife.Tests
{
    /// <summary>
    /// The grid has to stay odd so there is one center cell, and it has to stay
    /// positive: the window has no minimum size, and a canvas narrower than a
    /// single cell used to give a count of -1 and a negative canvas size, which
    /// WPF rejects with an exception.
    /// </summary>
    public class GridFitTests
    {
        [Theory]
        [InlineData(1000.0, 13.0, 75)]  // 76 fit, one comes off to make it odd
        [InlineData(260.0, 13.0, 19)]   // 20 fit exactly, so one comes off
        [InlineData(39.0, 13.0, 3)]     // fits exactly and is already odd
        public void CountsTheCellsThatFitAndKeepsItOdd(double available, double cellSize, int expected)
        {
            int count = LifeRules.FitOddCells(available, cellSize);
            Assert.Equal(expected, count);
            Assert.Equal(1, count % 2);
        }

        [Theory]
        [InlineData(12.0, 13.0)]   // narrower than one cell
        [InlineData(26.0, 13.0)]   // exactly two cells, so the odd rule takes one off
        [InlineData(0.0, 13.0)]    // the container has not been laid out yet
        [InlineData(-5.0, 13.0)]
        [InlineData(1000.0, 0.0)]  // no cell size yet
        [InlineData(1000.0, -1.0)]
        public void NeverFallsBelowOneCell(double available, double cellSize)
        {
            Assert.Equal(1, LifeRules.FitOddCells(available, cellSize));
        }
    }
}
