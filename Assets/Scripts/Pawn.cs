

using UnityEngine;
using UnityEngine.UI;

public class Pawn : Piece
{
    [SerializeField] private GameObject promotionPrefab;

    protected override void Start()
    {
        base.Start();
        pieceData.hasPostPlayAction = true;
        promotionPrefab.SetActive(false);
    }

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

    public override void SetPostPlayAction(int previousTileIdx, int targetTileIdx)
    {
        bool reachedOtherBoardSide = pieceData.playerIdx == 1 ? (targetTileIdx >= (TilesMap.totalColomns * (TilesMap.totalRows - 1))) : (targetTileIdx < TilesMap.totalColomns);

        if(reachedOtherBoardSide)
        {
            promotionPrefab.SetActive(true);
            string currentPlayerName = BoardController.Instance.GetCurrentPlayerName();
            Transform currentPlayerSelection = promotionPrefab.transform.Find(currentPlayerName);
            currentPlayerSelection.gameObject.SetActive(true);

            Button button;
            foreach (Transform child in currentPlayerSelection)
            {
                button = child.gameObject.GetComponent<Button>();
                button.onClick.AddListener(() => OnPromotionPicked(button, currentPlayerName, currentPlayerSelection));
            }
        }
        // No post play action, set the next player's turn.
        else
        {
            BoardController.Instance.SetNextPlayerTurn();
        }
    }

    private void OnPromotionPicked(Button button, string currentPlayerName, Transform currentPlayerSelection)
    {
        print("OnPromotionPicked " + button.gameObject.name);
        promotionPrefab.SetActive(false);
        currentPlayerSelection.gameObject.SetActive(false);

        // destroy the current pawn and create an instance of the selected item and set it in the hiearchy.
        GameObject newPiece = Instantiate(Resources.Load(currentPlayerName + "/" + button.gameObject.name) as GameObject);
        GameObject parentObj = BoardController.Instance.GetParentObjByPlayerIdx(pieceData.playerIdx);
        newPiece.name = button.gameObject.name + BoardController.Instance.MoveCount;
        newPiece.transform.SetParent(parentObj.transform);

        newPiece.GetComponent<Piece>().SetPieceData(pieceData);
        gameObject.SetActive(false);
        BoardController.Instance.SetNextPlayerTurn();
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
