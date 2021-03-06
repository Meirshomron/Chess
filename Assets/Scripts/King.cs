﻿

public class King : Piece
{
    protected override void Start()
    {
        base.Start();
        pieceData.isKing = true;
    }

    public override void SetMoves(bool validateMoves)
    {
        //print("King: SetMoves");

        // Clear the current moves before re-calculating.
        moves.Clear();
        attackMoves.Clear();

        int tileIdx;
        int currentRow = pieceData.currentIndex / TilesMap.totalRows;
        int currentColomn = pieceData.currentIndex % TilesMap.totalColomns;

        // Calculate the tiles immediately surrounding the current tile.
        for (int rowIdx = -1; rowIdx <= 1; rowIdx++)
        {
            for (int colIdx = -1; colIdx <= 1; colIdx++)
            {
                if ((rowIdx != 0 || colIdx != 0) && ((rowIdx + currentRow) < TilesMap.totalRows) && ((rowIdx + currentRow) >= 0) && ((colIdx + currentColomn) < TilesMap.totalColomns) && ((colIdx + currentColomn) >= 0))
                {
                    tileIdx = currentColomn + colIdx + (rowIdx + currentRow) * TilesMap.totalRows;
                    AddMoveToList(tileIdx);
                }
            }
        }

        // This flag enables us to control the need to validate this piece's moves on the chack status of the player.
        if (validateMoves)
        {
            ValidateMoves();
            ValidateAttackMoves();
        }
    }
}