using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    //This class has a very naive and basic algorithm for checking if the is a run of 4 in a connect 4 grid. Don't judge me, it's Sunday night..

    public class WinLogic
    {
        public static bool CheckWin(Tile[,] grid, Tile changedTile, out IEnumerable<Tile> winningTiles)
        {
            if (CheckHorizontal(grid, changedTile, out winningTiles))
            {
                return true;
            }

            if (CheckVertical(grid, changedTile, out winningTiles))
            {
                return true;
            }

            if (CheckDiagonalDown(grid, changedTile, out winningTiles))
            {
                return true;
            }

            if (CheckDiagonalUp(grid, changedTile, out winningTiles))
            {
                return true;
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckHorizontal(Tile[,] grid, Tile changedTile, out IEnumerable<Tile> winningTiles)
        {
            var expectedMarker = grid[changedTile.Row, changedTile.Col].PlayerMarker;
            var col = changedTile.Col;

            // Find earliest index that could include this tile in a winning run of for along the row it's contained in
            var beginIndex = changedTile.Row - 3;
            if (beginIndex < 0)
            {
                beginIndex = 0;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of 4
            for (var i = beginIndex; i <= changedTile.Row && i + 3 < grid.GetLength(0); i++)
            {
                // Check the 4 markers against what we expect
                if (grid[i ,col].PlayerMarker == expectedMarker && grid[i + 1, col].PlayerMarker == expectedMarker && grid[i + 2, col].PlayerMarker == expectedMarker && grid[i + 3, col].PlayerMarker == expectedMarker)
                {
                    winningTiles = new[]
                    {
                        grid[i, col],
                        grid[i + 1, col],
                        grid[i + 2, col],
                        grid[i + 3, col]
                    };

                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckVertical(Tile[,] grid, Tile changedTile, out IEnumerable<Tile> winningTiles)
        {
            var expectedMarker = grid[changedTile.Row, changedTile.Col].PlayerMarker;
            var row = changedTile.Row;

            // Find earliest index that could include this tile in a winning run of for along the column it's contained in
            var beginIndex = changedTile.Col - 3;
            if (beginIndex < 0)
            {
                beginIndex = 0;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of 4
            for (var i = beginIndex; i <= changedTile.Col && i + 3 < grid.GetLength(1); i++)
            {
                // Check the 4 markers against what we expect
                if (grid[row, i].PlayerMarker == expectedMarker && grid[row, i + 1].PlayerMarker == expectedMarker && grid[row, i + 2].PlayerMarker == expectedMarker && grid[row, i + 3].PlayerMarker == expectedMarker)
                {
                    winningTiles = new[]
                    {
                        grid[row, i],
                        grid[row, i + 1],
                        grid[row, i + 2],
                        grid[row, i + 3]
                    };

                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckDiagonalDown(Tile[,] grid, Tile changedTile, out IEnumerable<Tile> winningTiles)
        {
            var expectedMarker = grid[changedTile.Row, changedTile.Col].PlayerMarker;
            
            // Find earliest indices that could include this tile in a winning run of for along the downward row it's contained in
            var beginIndexRow = changedTile.Row - 3;
            var beginIndexCol = changedTile.Col - 3;

            // If eaither index is below the bounds we need to push ourselves down the diagonal line back to the bounds
            if (beginIndexCol < 0 || beginIndexRow < 0)
            {
                var furthest = Math.Min(beginIndexCol, beginIndexRow);
                beginIndexRow += furthest * -1;
                beginIndexCol += furthest * -1;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of 4
            for (var i = 0; beginIndexRow + i + 3 < grid.GetLength(0) && beginIndexCol +i + 3 < grid.GetLength(1); i++)
            {
                // Check the 4 markers against what we expect
                if (
                    grid[beginIndexRow + i,     beginIndexCol + i].PlayerMarker == expectedMarker && 
                    grid[beginIndexRow + i + 1, beginIndexCol + i + 1].PlayerMarker == expectedMarker && 
                    grid[beginIndexRow + i + 2, beginIndexCol + i + 2].PlayerMarker == expectedMarker && 
                    grid[beginIndexRow + i + 3, beginIndexCol + i + 3].PlayerMarker == expectedMarker
                )
                {
                    winningTiles = new[]
                    {
                        grid[beginIndexRow + i,     beginIndexCol + i],
                        grid[beginIndexRow + i + 1, beginIndexCol + i + 1],
                        grid[beginIndexRow + i + 2, beginIndexCol + i + 2],
                        grid[beginIndexRow + i + 3, beginIndexCol + i + 3]
                    };

                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckDiagonalUp(Tile[,] grid, Tile changedTile, out IEnumerable<Tile> winningTiles)
        {
            var expectedMarker = grid[changedTile.Row, changedTile.Col].PlayerMarker;

            // Find earliest indices that could include this tile in a winning run of for along the downward row it's contained in
            var beginIndexRow = changedTile.Row + 3;
            var beginIndexCol = changedTile.Col - 3;

            // If eaither index is outside the bounds we need to push ourselves up the diagonal line back to the bounds
            if (beginIndexCol < 0 || beginIndexRow > grid.GetLength(0) - 1) // The row is pushed forwards. Find out if it's pushed past the last possible row index
            {
                var rowOutDist = grid.GetLength(0) - 1 - beginIndexRow;

                var furthest = Math.Min(beginIndexCol, rowOutDist);
                beginIndexRow -= furthest * -1;
                beginIndexCol += furthest * -1;
            }
            
            // Loop from the earliest index up to the tiles actual index checking the runs of 4
            for (var i = 0; beginIndexRow - i - 3 > -1 && beginIndexCol + i + 3 < grid.GetLength(1); i++)
            {
                // Check the 4 markers against what we expect
                if (
                    grid[beginIndexRow - i, beginIndexCol + i].PlayerMarker == expectedMarker &&
                    grid[beginIndexRow - i - 1, beginIndexCol + i + 1].PlayerMarker == expectedMarker &&
                    grid[beginIndexRow - i - 2, beginIndexCol + i + 2].PlayerMarker == expectedMarker &&
                    grid[beginIndexRow - i - 3, beginIndexCol + i + 3].PlayerMarker == expectedMarker
                )
                {
                    winningTiles = new[]
                    {
                        grid[beginIndexRow - i,     beginIndexCol + i],
                        grid[beginIndexRow - i - 1, beginIndexCol + i + 1],
                        grid[beginIndexRow - i - 2, beginIndexCol + i + 2],
                        grid[beginIndexRow - i - 3, beginIndexCol + i + 3]
                    };

                    return true;
                }
            }











            var smallestIndex = Math.Min(beginIndexRow, beginIndexCol);

            // Loop from the earliest index up to the tiles actual index checking the runs of 4
            for (var i = smallestIndex; i <= changedTile.Col && i + 3 < grid.GetLength(1) && i + 3 < grid.GetLength(0); i++)
            {
                // Check the 4 markers against what we expect
                if (grid[i, i].PlayerMarker == expectedMarker && grid[i + 1, i + 1].PlayerMarker == expectedMarker && grid[i + 2, i + 2].PlayerMarker == expectedMarker && grid[i + 3, i + 3].PlayerMarker == expectedMarker)
                {
                    winningTiles = new[]
                    {
                        grid[i, i],
                        grid[i + 1, i + 1],
                        grid[i + 2, i + 2],
                        grid[i + 3, i + 3]
                    };

                    return true;
                }
            }

            winningTiles = null;
            return false;
        }
    }
}
