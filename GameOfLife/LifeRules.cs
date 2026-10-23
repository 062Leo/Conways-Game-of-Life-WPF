using System.Collections.Generic;
using System.Windows;

namespace GameOfLife
{
    /// <summary>
    /// Conway's rules, B3/S23, on a bounded grid: cells outside the grid are
    /// permanently dead, so a pattern touching the edge loses the neighbours it
    /// would have on an infinite plane.
    ///
    /// Kept free of UI state on purpose. This is the only place the rules live,
    /// and LifeRulesTests is what holds them.
    /// </summary>
    public static class LifeRules
    {
        /// <summary>
        /// One generation. Returns a new set; the input is not modified.
        /// </summary>
        public static HashSet<Point> Step(IReadOnlyCollection<Point> liveCells, int gridWidth, int gridHeight)
        {
            var next = new HashSet<Point>();

            if (gridWidth <= 0 || gridHeight <= 0 || liveCells.Count == 0)
            {
                return next;
            }

            var live = liveCells as HashSet<Point> ?? new HashSet<Point>(liveCells);
            var neighborCounts = CountNeighbors(live, gridWidth, gridHeight);

            // Survival: a live cell with two or three live neighbours stays.
            foreach (var cell in live)
            {
                int liveNeighbors = neighborCounts.TryGetValue(cell, out int count) ? count : 0;
                if (liveNeighbors == 2 || liveNeighbors == 3)
                {
                    next.Add(cell);
                }
            }

            // Birth: a dead cell with exactly three live neighbours comes alive.
            foreach (var kvp in neighborCounts)
            {
                if (kvp.Value == 3 && !live.Contains(kvp.Key))
                {
                    next.Add(kvp.Key);
                }
            }

            return next;
        }

        /// <summary>
        /// Live neighbours per cell, counted only for positions inside the grid.
        /// Cells with no live neighbour never appear in the result.
        /// </summary>
        private static Dictionary<Point, int> CountNeighbors(HashSet<Point> live, int gridWidth, int gridHeight)
        {
            var counts = new Dictionary<Point, int>();

            foreach (var cell in live)
            {
                int cellX = (int)cell.X;
                int cellY = (int)cell.Y;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        int neighborX = cellX + dx;
                        int neighborY = cellY + dy;

                        if (!IsWithinGrid(neighborX, neighborY, gridWidth, gridHeight))
                        {
                            continue;
                        }

                        var neighbor = new Point(neighborX, neighborY);
                        counts[neighbor] = counts.TryGetValue(neighbor, out int count) ? count + 1 : 1;
                    }
                }
            }

            return counts;
        }

        public static bool IsWithinGrid(int x, int y, int gridWidth, int gridHeight)
        {
            return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
        }

        /// <summary>
        /// How many cells fit along one axis. Always odd, so the grid has a single
        /// center cell, and never below 1: a window narrow enough to fit fewer
        /// cells than one used to produce a count of -1, and the negative canvas
        /// size that followed made WPF throw.
        /// </summary>
        public static int FitOddCells(double available, double cellSize)
        {
            if (cellSize <= 0 || available <= 0)
            {
                return 1;
            }

            int count = (int)System.Math.Floor(available / cellSize);
            if (count % 2 == 0)
            {
                count--;
            }

            return count < 1 ? 1 : count;
        }
    }
}
