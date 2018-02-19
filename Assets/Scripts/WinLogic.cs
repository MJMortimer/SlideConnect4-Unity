using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    //This class has a very naive and basic algorithm for checking if the is a run of a given win length in a connect 4 style grid. Don't judge me, it's Sunday night..

    public class WinLogic
    {
        public static bool CheckWin(Tile[,] grid, Tile changedTile, int winLength, out IEnumerable<Tile> winningTiles)
        {
            if (CheckHorizontal(grid, changedTile, winLength, out winningTiles))
            {
                return true;
            }

            if (CheckVertical(grid, changedTile, winLength, out winningTiles))
            {
                return true;
            }

            if (CheckDiagonalDown(grid, changedTile, winLength, out winningTiles))
            {
                return true;
            }

            if (CheckDiagonalUp(grid, changedTile, winLength, out winningTiles))
            {
                return true;
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckHorizontal(Tile[,] grid, Tile changedTile, int winLength, out IEnumerable<Tile> winningTiles)
        {
            var expectedColor = grid[changedTile.Row, changedTile.Col].Color;
            var col = changedTile.Col;

            // Find earliest index that could include this tile in a winning run of for along the row it's contained in
            var beginIndex = changedTile.Row - winLength - 1;
            if (beginIndex < 0)
            {
                beginIndex = 0;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of the win length
            for (var i = beginIndex; i <= changedTile.Row && i + winLength - 1 < grid.GetLength(0); i++)
            {
                // Collect tiles to check
                var tilesToCheck = new List<Tile>();
                for (var j = 0; j < winLength; j++)
                {
                    tilesToCheck.Add(grid[i + j, col]);
                }

                if (tilesToCheck.All(it => it.Color == expectedColor))
                {
                    winningTiles = tilesToCheck;
                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckVertical(Tile[,] grid, Tile changedTile, int winLength, out IEnumerable<Tile> winningTiles)
        {
            var expectedColor = grid[changedTile.Row, changedTile.Col].Color;
            var row = changedTile.Row;

            // Find earliest index that could include this tile in a winning run of for along the column it's contained in
            var beginIndex = changedTile.Col - winLength - 1;
            if (beginIndex < 0)
            {
                beginIndex = 0;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of the win length
            for (var i = beginIndex; i <= changedTile.Col && i + winLength - 1 < grid.GetLength(1); i++)
            {
                // Collect tiles to check
                var tilesToCheck = new List<Tile>();
                for (var j = 0; j < winLength; j++)
                {
                    tilesToCheck.Add(grid[row, i + j]);
                }

                if (tilesToCheck.All(it => it.Color == expectedColor))
                {
                    winningTiles = tilesToCheck;
                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckDiagonalDown(Tile[,] grid, Tile changedTile, int winLength, out IEnumerable<Tile> winningTiles)
        {
            var expectedColor = grid[changedTile.Row, changedTile.Col].Color;
            
            // Find earliest indices that could include this tile in a winning run of for along the downward row it's contained in
            var beginIndexRow = changedTile.Row - winLength - 1;
            var beginIndexCol = changedTile.Col - winLength - 1;

            // If eaither index is below the bounds we need to push ourselves down the diagonal line back to the bounds
            if (beginIndexCol < 0 || beginIndexRow < 0)
            {
                var farthest = Math.Min(beginIndexCol, beginIndexRow);
                beginIndexRow += farthest * -1;
                beginIndexCol += farthest * -1;
            }

            // Loop from the earliest index up to the tiles actual index checking the runs of the win length
            for (var i = 0;  beginIndexRow + i + winLength - 1 < grid.GetLength(0) && beginIndexCol + i + winLength - 1 < grid.GetLength(1); i++)
            {
                // Collect tiles to check
                var tilesToCheck = new List<Tile>();
                for (var j = 0; j < winLength; j++)
                {
                    tilesToCheck.Add(grid[beginIndexRow + i + j, beginIndexCol + i + j]);
                }

                if (tilesToCheck.All(it => it.Color == expectedColor))
                {
                    winningTiles = tilesToCheck;
                    return true;
                }
            }

            winningTiles = null;
            return false;
        }

        private static bool CheckDiagonalUp(Tile[,] grid, Tile changedTile, int winLength, out IEnumerable<Tile> winningTiles)
        {
            var expectedColor = grid[changedTile.Row, changedTile.Col].Color;

            // Find earliest indices that could include this tile in a winning run of for along the downward row it's contained in
            var beginIndexRow = changedTile.Row + winLength - 1;
            var beginIndexCol = changedTile.Col - winLength + 1;

            // If either index is outside the bounds we need to push ourselves up the diagonal line back to the bounds
            if (beginIndexCol < 0 || beginIndexRow > grid.GetLength(0) - 1) // The row is pushed forwards. Find out if it's pushed past the last possible row index
            {
                var rowOutDist = grid.GetLength(0) - 1 - beginIndexRow;

                var farthest = Math.Min(beginIndexCol, rowOutDist);
                beginIndexRow -= farthest * -1;
                beginIndexCol += farthest * -1;
            }
            
            // Loop from the earliest index up to the tiles actual index checking the runs of the win length
            for (var i = 0; beginIndexRow + 1 - i - winLength >= 0 && beginIndexCol + i + winLength - 1 < grid.GetLength(1); i++)
            {
                // Collect tiles to check
                var tilesToCheck = new List<Tile>();
                for (var j = 0; j < winLength; j++)
                {
                    tilesToCheck.Add(grid[beginIndexRow - i - j, beginIndexCol + i + j]);
                }

                if (tilesToCheck.All(it => it.Color == expectedColor))
                {
                    winningTiles = tilesToCheck;
                    return true;
                }
            }

            winningTiles = null;
            return false;
        }
    }
}
