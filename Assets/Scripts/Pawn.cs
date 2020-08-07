

public class Pawn : Piece
{
    public override void SetMoves()
    {
        //print("Pawn: SetMoves");

        bool canMakeDoubleForwad = false;

        if ((IsOnTheTop() && pieceData.playerIdx == 1) || (IsOnTheBottom() && pieceData.playerIdx == -1))
        {
            return;
        }

        int tileIdx = pieceData.currentIndex + (pieceData.playerIdx * TilesMap.totalColomns);
        Tile forwardTile = TilesMap.Instance.GetTile(tileIdx);
        if (forwardTile.isEmpty)
        {
            moves.Add(tileIdx);
            canMakeDoubleForwad = true;
        }

        if (!IsOnTheLeft())
        {
            int leftTileIdx = pieceData.currentIndex + (pieceData.playerIdx * TilesMap.totalColomns) - 1;
            Tile leftTile = TilesMap.Instance.GetTile(leftTileIdx);
            if (!leftTile.isEmpty && leftTile.playerIdxOnTile == -1 * pieceData.playerIdx)
            {
                attackMoves.Add(leftTileIdx);
            }
        }

        if (!IsOnTheRight())
        {
            int rightTileIdx = pieceData.currentIndex + (pieceData.playerIdx * TilesMap.totalColomns) + 1;
            Tile rightTile = TilesMap.Instance.GetTile(rightTileIdx);
            if (!rightTile.isEmpty && rightTile.playerIdxOnTile == -1 * pieceData.playerIdx)
            {
                attackMoves.Add(rightTileIdx);
            }
        }

        if(!pieceData.madeFirstMove && canMakeDoubleForwad)
        {
            tileIdx = pieceData.currentIndex + (pieceData.playerIdx * 2 *TilesMap.totalColomns);
            Tile doubleForwardTile = TilesMap.Instance.GetTile(tileIdx);
            if (doubleForwardTile.isEmpty)
            {
                moves.Add(tileIdx);
            }
        }
    }

    private bool IsOnTheLeft()
    {
        if (pieceData.currentIndex % TilesMap.totalColomns == 0)
        {
            return true;
        }
        return false;
    }

    private bool IsOnTheRight()
    {
        if ((pieceData.currentIndex + 1) % TilesMap.totalColomns == 0)
        {
            return true;
        }
        return false;
    }

    private bool IsOnTheBottom()
    {
        if (pieceData.currentIndex < TilesMap.totalColomns)
        {
            return true;
        }
        return false;
    }

    private bool IsOnTheTop()
    {
        if (pieceData.currentIndex >= (TilesMap.totalColomns * (TilesMap.totalRows - 1)))
        {
            return true;
        }
        return false;
    }
}
