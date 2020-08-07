﻿

public class Rook : Piece
{
    public override void SetMoves()
    {
        //print("Rook: SetMoves");

        int tileIdx;
        int currentRow = pieceData.currentIndex / TilesMap.totalRows;
        int currentColomn = pieceData.currentIndex % TilesMap.totalColomns;
        bool addedToAttackList;

        // Calculate the tiles above the current Rook's tile.
        for (int row = currentRow + 1; row < TilesMap.totalRows; row++)
        {
            tileIdx = currentColomn + row * TilesMap.totalRows;
            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }
        
        // Calculate the tiles to the right of the current Rook's tile.
        for (int col = currentColomn + 1; col < TilesMap.totalColomns; col++)
        {
            tileIdx = pieceData.currentIndex - currentColomn + col;
            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }

        // Calculate the tiles under the current Rook's tile.
        for (int row = 1; row <= currentRow; row++)
        {
            tileIdx = pieceData.currentIndex - row * TilesMap.totalRows;
            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }

        // Calculate the tiles to the left of the current Rook's tile.
        for (int col = 1; col <= currentColomn; col++)
        {
            tileIdx = pieceData.currentIndex - col;
            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }
    }
}
