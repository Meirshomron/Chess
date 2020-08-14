using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance { get; private set; }
    public GameModel GameModel { get; private set; }
    public Simulator Simulator { get; private set; }


    [SerializeField] private TextMeshProUGUI userTxt = null;
    [SerializeField] private Material originalMaterial = null;
    [SerializeField] private Material selectedMaterial = null;
    private Color markedMoveTile;
    private Color markedAttackMoveTile;
    private bool turnDisabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameModel = GetComponent<GameModel>();
        Simulator = GetComponent<Simulator>();
        GameModel.Init();
    }

    void Start()
    {
        // The colors of the tiles the currently selected piece can move/attack.
        ColorUtility.TryParseHtmlString("#2FFF4D", out markedMoveTile);
        ColorUtility.TryParseHtmlString("#B32323", out markedAttackMoveTile);

        // Set the turn text.
        SetUserTurnText();

        turnDisabled = false;
    }

    private void Update()
    {
        if (turnDisabled || GameModel.GameIsOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Bit shift the index of the layer (9) to get a bit mask, this layer only includes the pieces.
            int layerMask = 1 << 9;

            if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
            {
                print("Pressed piece " + hit.transform.gameObject.name);

                Piece pieceClicked = hit.transform.gameObject.GetComponent<Piece>();

                // Only enable clicking a piece if it's this palyer's turn.
                if (GameModel.PlayerTurnIdx == pieceClicked.pieceData.playerIdx)
                {
                    // Disable re-clicking the currently picked piece.
                    if (GameModel.CurrentClickedPiece != pieceClicked)
                    {
                        bool playerInCheck = GameModel.GetCheckStatus(GameModel.PlayerTurnIdx);
                        pieceClicked.SetMoves(true);

                        // Only moves to get out of the check status are valid.
                        if (playerInCheck && pieceClicked.moves.Count == 0 && pieceClicked.attackMoves.Count == 0)
                        {
                            print("Player " + GameModel.PlayerTurnIdx + " in check with no moves on piece clicked, check if check mate.");
                            bool isCheckMate = true;
                            List<Piece> pieces = GameModel.GetPlayerPieces(GameModel.PlayerTurnIdx);

                            foreach (Piece piece in pieces)
                            {
                                if (piece.pieceData.isDead)
                                    continue;

                                // In case the previous simulations changed the current piece's moves.
                                piece.SetMoves(true);

                                if (piece.moves.Count != 0 || piece.attackMoves.Count != 0)
                                {
                                    isCheckMate = false;
                                }
                            }

                            // No piece can move to save the king = check mate.
                            if (isCheckMate)
                            {
                                print("----------------------- isCheckMate -----------------------");
                                GameModel.GameIsOver = true;
                                return;
                            }
                            // In case the simulations changed the picked piece's moves.
                            else
                            {
                                pieceClicked.SetMoves(true);
                            }
                        }
                        OnPieceClicked(pieceClicked);

                        // If we've clicked a valid piece - don't check for a tile clicked in this iteration of Update().
                        return;
                    }
                }
            }

            // Bit shift the index of the layer (8) to get a bit mask, this layer only includes the tiles.
            layerMask = 1 << 8;

            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {
                //print("Pressed tile " + hit.transform.gameObject.name);

                int.TryParse(hit.transform.gameObject.name, out int targetTileName);
                if (GameModel.CurrentClickedPiece)
                {
                    if (GameModel.CurrentClickedPiece.moves.Contains(targetTileName))
                    {
                        SetPlayAction(targetTileName);
                    }
                    else if (GameModel.CurrentClickedPiece.attackMoves.Contains(targetTileName))
                    {
                        Piece attackedPiece = GetPieceAtTileIdx(-1 * GameModel.CurrentClickedPiece.pieceData.playerIdx, targetTileName);
                        SetPlayAction(targetTileName, attackedPiece);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Callback once clicking a piece. Unpick the previous selected piece and mark the movement of the currently clicked piece.
    /// </summary>
    /// <param name="pieceClicked"> The currently picked piece. </param>
    private void OnPieceClicked(Piece pieceClicked)
    {
        print("BoardPieces: OnPieceClicked " + pieceClicked.pieceData.gameObjectName);

        // Unpick previous piece.
        if (GameModel.CurrentClickedPiece && GameModel.CurrentClickedPiece.pieceData.currentIndex != pieceClicked.pieceData.currentIndex)
        {
            UnpickCurrentPieceMarkers();
            UnpickCurrentPieceMaterial();
        }

        GameModel.CurrentClickedPiece = pieceClicked;

        SetSelectedMaterial();

        foreach (int move in pieceClicked.moves)
        {
            TilesMap.Instance.SetTileColor(move, markedMoveTile);
        }

        foreach (int attackMove in pieceClicked.attackMoves)
        {
            TilesMap.Instance.SetTileColor(attackMove, markedAttackMoveTile);
        }
    }

    /// <summary>
    /// Un-mark the current markers of the CurrentClickedPiece.
    /// </summary>
    private void UnpickCurrentPieceMarkers()
    {
        //print("BoardPieces: UnpickCurrentPieceMarkers " + CurrentClickedPiece.currentIndex);

        foreach (int move in GameModel.CurrentClickedPiece.moves)
        {
            TilesMap.Instance.SetTileOrigColor(move);
        }

        foreach (int attackMove in GameModel.CurrentClickedPiece.attackMoves)
        {
            TilesMap.Instance.SetTileOrigColor(attackMove);
        }
    }

    /// <summary>
    /// Set the given piece on given tile.
    /// </summary>
    /// <param name="tileIdx"> The tile index to set on. </param>
    /// <param name="piece"> The piece to set on the given tile. </param>
    public void SetPieceOnTile(int tileIdx, Piece piece)
    {
        MovePiece(0, tileIdx, piece);
    }

    /// <summary>
    /// Perform the player's turn action. 
    /// </summary>
    /// <param name="targetTileIdx"> The target tile index of the currently selected piece. </param>
    /// <param name="attackedPiece"> [optional] If this was an attack move, this is the attacked piece. </param>
    private void SetPlayAction(int targetTileIdx, Piece attackedPiece = null)
    {
        print("SetPlayAction: from " + GameModel.CurrentClickedPiece.pieceData.currentIndex + " to " + targetTileIdx);

        // Disable anymore player actions intill we set the other user's turn.
        turnDisabled = true;

        if (attackedPiece)
        {
            attackedPiece.gameObject.SetActive(false);
            if (attackedPiece.pieceData.isKing)
            {
                SetUserEndGameText();
                GameModel.GameIsOver = true;
            }
        }

        // Complete the current piece's move.
        GameModel.CurrentClickedPiece.pieceData.madeFirstMove = true;
        UnpickCurrentPieceMarkers();
        UnpickCurrentPieceMaterial();
        int previousTileIdx = GameModel.CurrentClickedPiece.pieceData.currentIndex;
        MovePiece(previousTileIdx, targetTileIdx, GameModel.CurrentClickedPiece);
        
        // If the current piece has post play action, like a pawn promotion then play it. Otheriwse set the next player's turn. 
        if (GameModel.CurrentClickedPiece.pieceData.hasPostPlayAction)
        {
            GameModel.CurrentClickedPiece.SetPostPlayAction(previousTileIdx, targetTileIdx);
        }
        else
        {
            SetNextPlayerTurn();
        }
    }

    /// <summary>
    /// Called once a player completed his turn. Update the moves of all the pieces and set the next player's turn.
    /// </summary>
    public void SetNextPlayerTurn()
    {
        GameModel.SetAllPiecesMoves(true);
        bool isPlayerChecked = CheckIfPlayerIsChecked(GameModel.PlayerTurnIdx * -1);
        
        // If this player that just moved was in check, then he must have made a valid move - to not be in check anymore.
        GameModel.SetCheckStatus(GameModel.PlayerTurnIdx, false);

        print("isPlayerChecked = " + isPlayerChecked.ToString());
        GameModel.SetCheckStatus(GameModel.PlayerTurnIdx * -1, isPlayerChecked);
        GameModel.CurrentClickedPiece = null;

        if (!GameModel.GameIsOver)
        {
            GameModel.MoveCount++;
            turnDisabled = false;
            GameModel.PlayerTurnIdx *= -1;
            SetUserTurnText();
        }
    }

    /// <summary>
    /// Move a piece from it's current tile to the new tile.
    /// </summary>
    /// <param name="previousTileIdx"> The preivous tile that the given piece was on. </param>
    /// <param name="newTileIdx"> The new tile to set the given piece on. </param>
    /// <param name="piece"> The piece that is moving. </param>
    private void MovePiece(int previousTileIdx, int newTileIdx, Piece piece)
    {
        TilesMap.Instance.UpdateTile(previousTileIdx, true);
        TilesMap.Instance.UpdateTile(newTileIdx, false);
        TilesMap.Instance.UpdateTile(previousTileIdx, 0);
        TilesMap.Instance.UpdateTile(newTileIdx, piece.pieceData.playerIdx);

        Tile newTile = TilesMap.Instance.GetTile(newTileIdx);
        piece.transform.position = newTile.tile.transform.position;
        piece.pieceData.currentIndex = newTileIdx;
        piece.moves.Clear();
        piece.attackMoves.Clear();
    }

    /// <summary>
    /// Getter for the piece on the given tile index.
    /// </summary>
    /// <param name="playerIdx"> The player index of the desired piece. </param>
    /// <param name="tileIdx"> The tile index to check the piece on. </param>
    /// <returns> The piece on the given tile index or null. </returns>
    public Piece GetPieceAtTileIdx(int playerIdx, int tileIdx)
    {
        Transform parentTransform = GameModel.GetPlayerParentObj(playerIdx).transform;
        foreach (Transform child in parentTransform)
        {
            if (child.GetComponent<Piece>().pieceData.currentIndex == tileIdx)
            {
                return child.GetComponent<Piece>();
            }
        }

        return null;
    }

    /// <summary>
    /// Set the user's turn text.
    /// </summary>
    public void SetUserTurnText()
    {
        userTxt.text = GameModel.GetCurrentPlayerName() + "'s  turn";
    }

    /// <summary>
    /// Set the game over text.
    /// </summary>
    private void SetUserEndGameText()
    {
        userTxt.text = GameModel.GetCurrentPlayerName() + " WON!";
    }

    /// <summary>
    /// Reset the currently picked piece's material to it's original material.
    /// </summary>
    private void UnpickCurrentPieceMaterial()
    {
        // We change the first child, because a piece only contains 1 child with the graphic representation of the piece.
        GameObject child = GameModel.CurrentClickedPiece.gameObject.transform.GetChild(0).gameObject;
        child.GetComponent<Renderer>().material = originalMaterial;
    }

    /// <summary>
    /// Set the currently picked piece's material to the 'selected' material of a piece.
    /// </summary>
    private void SetSelectedMaterial()
    {
        // We change the first child, because a piece only contains 1 child with the graphic representation of the piece.
        GameObject child = GameModel.CurrentClickedPiece.gameObject.transform.GetChild(0).gameObject;
        child.GetComponent<Renderer>().material = selectedMaterial;
    }

    /// <summary>
    /// Reset the positions of all the pieces and the status of all the tiles.
    /// </summary>
    public void ResetBoardPieces()
    {
        // Set all the tiles as empty.
        for (int tileIdx = 0; tileIdx < TilesMap.totalColomns * TilesMap.totalRows; tileIdx++)
        {
            TilesMap.Instance.UpdateTile(tileIdx, true);
        }

        Piece currentPiece;
        Tile currentTile;
        foreach (Transform child in GameModel.GetPlayerParentObj(GameModel.PlayerTurnIdx).transform)
        {
            currentPiece = child.GetComponent<Piece>();
            if (currentPiece.pieceData.isDead)
                return;

            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, false);
            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, currentPiece.pieceData.playerIdx);

            currentTile = TilesMap.Instance.GetTile(currentPiece.pieceData.currentIndex);
            currentPiece.transform.position = currentTile.tile.transform.position;
            currentPiece.moves.Clear();
            currentPiece.attackMoves.Clear();
            currentPiece.SetMoves(true);
        }

        foreach (Transform child in GameModel.GetPlayerParentObj(GameModel.PlayerTurnIdx).transform)
        {
            currentPiece = child.GetComponent<Piece>();
            if (currentPiece.pieceData.isDead)
                return;

            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, false);
            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, currentPiece.pieceData.playerIdx);

            currentTile = TilesMap.Instance.GetTile(currentPiece.pieceData.currentIndex);
            currentPiece.transform.position = currentTile.tile.transform.position;
            currentPiece.moves.Clear();
            currentPiece.attackMoves.Clear();
            currentPiece.SetMoves(true);
        }

        GameModel.CurrentClickedPiece = null;
    }

    /// <summary>
    /// Check if the given player's king in check.
    /// </summary>
    /// <param name="playerIdx"> The player to handle its check status. </param>
    public bool CheckIfPlayerIsChecked(int playerIdx)
    {
        print("CheckIfPlayerIsChecked");

        List<Piece> threatPieces = GameModel.GetPlayerPieces(playerIdx * -1);
        foreach (Piece threatPiece in threatPieces)
        {
            if (!threatPiece.pieceData.isDead)
            {
                foreach (int attackMove in threatPiece.attackMoves)
                {
                    Piece pieceUnderAttack = GetPieceAtTileIdx(playerIdx, attackMove);
                    if (pieceUnderAttack.pieceData.isKing)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
