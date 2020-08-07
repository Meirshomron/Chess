

public class Knight : Piece
{
    public override void SetMoves()
    {
        //print("Knight: SetMoves");

        int tileIdx;
        int currentRow = pieceData.currentIndex / TilesMap.totalRows;
        int currentColomn = pieceData.currentIndex % TilesMap.totalColomns;
        
        // Handle possible moves above the current position.
        if (currentRow + 1 < TilesMap.totalRows)
        {
            if (currentColomn + 2 < TilesMap.totalColomns)
            {
                tileIdx = currentColomn + 2 + (currentRow + 1) * TilesMap.totalRows;
                AddMoveToList(tileIdx);
            }

            if (currentColomn - 2 >= 0)
            {
                tileIdx = currentColomn - 2 + (currentRow + 1) * TilesMap.totalRows;
                AddMoveToList(tileIdx);
            }


            if (currentRow + 2 < TilesMap.totalRows)
            {
                if (currentColomn + 1 < TilesMap.totalColomns)
                {
                    tileIdx = currentColomn + 1 + (currentRow + 2) * TilesMap.totalRows;
                    AddMoveToList(tileIdx);
                }

                if (currentColomn - 1 >= 0)
                {
                    tileIdx = currentColomn - 1 + (currentRow + 2) * TilesMap.totalRows;
                    AddMoveToList(tileIdx);
                }
            }
        }

        // Handle possible moves under the current position.
        if (currentRow - 1 >= 0)
        {
            if (currentColomn + 2 < TilesMap.totalColomns)
            {
                tileIdx = currentColomn + 2 + (currentRow - 1) * TilesMap.totalRows;
                AddMoveToList(tileIdx);
            }

            if (currentColomn - 2 >= 0)
            {
                tileIdx = currentColomn - 2 + (currentRow - 1) * TilesMap.totalRows;
                AddMoveToList(tileIdx);
            }

            if (currentRow - 2 >= 0)
            {
                if (currentColomn + 1 < TilesMap.totalColomns)
                {
                    tileIdx = currentColomn + 1 + (currentRow - 2) * TilesMap.totalRows;
                    AddMoveToList(tileIdx);
                }

                if (currentColomn - 1 >= 0)
                {
                    tileIdx = currentColomn - 1 + (currentRow - 2) * TilesMap.totalRows;
                    AddMoveToList(tileIdx);
                }
            }
        }
    }
}
