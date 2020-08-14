

public class Bishop : Piece
{
    public override void SetMoves(bool validateMoves)
    {
        //print("Bishop: SetMoves");

        // Clear the current moves before re-calculating.
        moves.Clear();
        attackMoves.Clear();

        int tileIdx;
        int currentRow = pieceData.currentIndex / TilesMap.totalRows;
        int currentColomn = pieceData.currentIndex % TilesMap.totalColomns;
        bool addedToAttackList;

        // Calculate the tiles top right diagonal of the current Rook's tile.
        for (int rowIdx = 1; rowIdx < (TilesMap.totalRows - currentRow); rowIdx++)
        {
            tileIdx = currentColomn + rowIdx + (rowIdx + currentRow) * TilesMap.totalRows;

            // If the tile is the first in a row then break.
            if (tileIdx % TilesMap.totalColomns == 0)
            {
                break;
            }

            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }

        // Calculate the tiles top left diagonal of the current Rook's tile.
        for (int rowIdx = 1; rowIdx < (TilesMap.totalRows - currentRow); rowIdx++)
        {
            tileIdx = currentColomn - rowIdx + (rowIdx + currentRow) * TilesMap.totalRows;

            // If the tile is the last in a row then break.
            if ((tileIdx + 1) % TilesMap.totalColomns == 0)
            {
                break;
            }

            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }

        // Calculate the tiles bottom left diagonal of the current Rook's tile.
        for (int rowIdx = 1; rowIdx <= currentRow; rowIdx++)
        {
            tileIdx = currentColomn - rowIdx + (currentRow - rowIdx) * TilesMap.totalRows;

            // If the tile is the last in a row then break.
            if ((tileIdx + 1) % TilesMap.totalColomns == 0)
            {
                break;
            }

            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
            }
        }

        // Calculate the tiles bottom right diagonal of the current Rook's tile.
        for (int rowIdx = 1; rowIdx <= currentRow; rowIdx++)
        {
            tileIdx = currentColomn + rowIdx + (currentRow - rowIdx) * TilesMap.totalRows;

            // If the tile is the first in a row then break.
            if (tileIdx % TilesMap.totalColomns == 0)
            {
                break;
            }

            addedToAttackList = AddMoveToList(tileIdx);
            if (addedToAttackList)
            {
                break;
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
