

using UnityEngine;
using UnityEngine.UI;

public class Pawn : Piece
{
    [SerializeField] private GameObject promotionPrefab = null;

    protected override void Start()
    {
        base.Start();

        // The possiblity for post play action enables us to implement the Promotion Rule after the pawn reaches the other side of the board.
        pieceData.hasPostPlayAction = true;

        // Unlike all the other pieces, a pawn's move doesn't mean that if the opposite player's piece was there it would be an attack move for this pawn.
        pieceData.movesAreHarmless = true;

        // Part of the Promotion Rule implementation.
        promotionPrefab.SetActive(false);
    }

    public override void SetMoves(bool validateMoves)
    {
        //print("Pawn: SetMoves");

        // Clear the current moves before re-calculating.
        moves.Clear();
        attackMoves.Clear();

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

        // This flag enables us to control the need to validate this piece's moves on the chack status of the player.
        if (validateMoves)
        {
            ValidateMoves();
            ValidateAttackMoves();
        }
    }

    /// <summary>
    /// Called after the pawn made this move, we can implement the Promotion Rule before the turn passes to the other player.
    /// </summary>
    /// <param name="previousTileIdx"> The preivous tile of the pawn. </param>
    /// <param name="targetTileIdx"> The new tile of the pawn. </param>
    public override void SetPostPlayAction(int previousTileIdx, int targetTileIdx)
    {
        bool reachedOtherBoardSide = pieceData.playerIdx == 1 ? (targetTileIdx >= (TilesMap.totalColomns * (TilesMap.totalRows - 1))) : (targetTileIdx < TilesMap.totalColomns);

        // If we've reached the other side of the board - implement the Promotion Rule.
        if(reachedOtherBoardSide)
        {
            // Show the different promotion options for this pawn.
            promotionPrefab.SetActive(true);
            string currentPlayerName = BoardController.Instance.GameModel.GetCurrentPlayerName();
            Transform currentPlayerSelection = promotionPrefab.transform.Find(currentPlayerName);
            currentPlayerSelection.gameObject.SetActive(true);

            // Listen to the picking of any option from the promotion selection.
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

    /// <summary>
    /// Called with the promotion option picked for this pawn.
    /// </summary>
    /// <param name="button"> The button option selected - must match a game piece prefab. </param>
    /// <param name="currentPlayerName"> The name of the player selecting. </param>
    /// <param name="currentPlayerSelection"> The selection child of the promotionPrefab. </param>
    private void OnPromotionPicked(Button button, string currentPlayerName, Transform currentPlayerSelection)
    {
        print("OnPromotionPicked " + button.gameObject.name);

        // Disable the promotionPrefab.
        promotionPrefab.SetActive(false);
        currentPlayerSelection.gameObject.SetActive(false);

        // Destroy the current pawn and create an instance of the selected item and set it in the hiearchy.
        GameObject newPiece = Instantiate(Resources.Load(currentPlayerName + "/" + button.gameObject.name) as GameObject);
        GameObject parentObj = BoardController.Instance.GameModel.GetPlayerParentObj(pieceData.playerIdx);
        newPiece.name = button.gameObject.name + BoardController.Instance.GameModel.MoveCount;
        newPiece.transform.SetParent(parentObj.transform);

        newPiece.GetComponent<Piece>().SetPieceData(pieceData);
        gameObject.SetActive(false);
        BoardController.Instance.GameModel.CurrentClickedPiece = newPiece.GetComponent<Piece>();

        // Promotion completed. Set the next player's turn.
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
