using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance { get => _instance; }
    public int PlayerTurnIdx { get => playerTurnIdx; set => playerTurnIdx = value; }
    public Piece CurrentClickedPiece { get => currentClickedPiece; set => currentClickedPiece = value; }
    public bool GameIsOver { get => gameIsOver; set => gameIsOver = value; }
    public int MoveCount { get => moveCount; set => moveCount = value; }

    // The parents of all the game pieces by player index.
    [SerializeField] private GameObject playerOne = null;
    [SerializeField] private GameObject playerTwo = null;

    [SerializeField] private TextMeshProUGUI userTxt = null;
    [SerializeField] private Material originalMaterial = null;
    [SerializeField] private Material selectedMaterial = null;

    private static BoardController _instance;

    private Color markedMoveTile;
    private Color markedAttackMoveTile;
    private Piece currentClickedPiece;
    private int playerTurnIdx;
    private bool gameIsOver;
    private bool turnDisabled;
    private int moveCount;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    void Start()
    {
        // The colors of the tiles the currently selected piece can move/attack.
        ColorUtility.TryParseHtmlString("#2FFF4D", out markedMoveTile);
        ColorUtility.TryParseHtmlString("#B32323", out markedAttackMoveTile);

        // The starting player's index.
        PlayerTurnIdx = 1;
        SetUserTurnText();

        GameIsOver = false;
        turnDisabled = false;
        MoveCount = 1;
    }

    private void Update()
    {
        if (turnDisabled)
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
                if (PlayerTurnIdx == pieceClicked.pieceData.playerIdx)
                {
                    // Disable re-clicking the currently picked piece.
                    if (CurrentClickedPiece != pieceClicked)
                    {
                        pieceClicked.SetMoves();
                        OnPieceClicked(pieceClicked);

                        // If we've clicked a valid piece - don't check for a tile clicked.
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
                if (CurrentClickedPiece)
                {
                    if (CurrentClickedPiece.moves.Contains(targetTileName))
                    {
                        SetPlayAction(targetTileName);
                    }
                    else if (CurrentClickedPiece.attackMoves.Contains(targetTileName))
                    {
                        Piece attackedPiece = GetPieceAtTileIdx(-1 * CurrentClickedPiece.pieceData.playerIdx, targetTileName);
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
        //print("BoardPieces: OnPieceClicked " + pieceClicked.currentIndex);

        if (CurrentClickedPiece && CurrentClickedPiece.pieceData.currentIndex != pieceClicked.pieceData.currentIndex)
        {
            UnpickCurrentPieceMarkers();
            UnpickCurrentPieceMaterial();
        }

        CurrentClickedPiece = pieceClicked;

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
    /// Un-mark the current markers of the currentClickedPiece.
    /// </summary>
    private void UnpickCurrentPieceMarkers()
    {
        //print("BoardPieces: UnpickCurrentPieceMarkers " + currentClickedPiece.currentIndex);

        foreach (int move in CurrentClickedPiece.moves)
        {
            TilesMap.Instance.SetTileOrigColor(move);
        }

        foreach (int attackMove in CurrentClickedPiece.attackMoves)
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
    private void SetPlayAction(int targetTileIdx, Piece attackedPiece = null)
    {
        print("SetPlayAction: from " + CurrentClickedPiece.pieceData.currentIndex + " to " + targetTileIdx);

        // Disable anymore player actions intill we set the other user's turn.
        turnDisabled = true;

        if (attackedPiece)
        {
            attackedPiece.gameObject.SetActive(false);
            if (attackedPiece.pieceData.isKing)
            {
                SetUserEndGameText();
                GameIsOver = true;
            }
        }

        CurrentClickedPiece.pieceData.madeFirstMove = true;
        UnpickCurrentPieceMarkers();
        UnpickCurrentPieceMaterial();
        int previousTileIdx = CurrentClickedPiece.pieceData.currentIndex;
        MovePiece(previousTileIdx, targetTileIdx, CurrentClickedPiece);

        if (CurrentClickedPiece.pieceData.hasPostPlayAction)
        {
            CurrentClickedPiece.SetPostPlayAction(previousTileIdx, targetTileIdx);
        }
        else
        {
            SetNextPlayerTurn();
        }

        CurrentClickedPiece = null;
    }

    public void SetNextPlayerTurn()
    {
        if (!GameIsOver)
        {
            //print("SetNextPlayerTurn");
            MoveCount++;
            turnDisabled = false;
            PlayerTurnIdx *= -1;
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
    private Piece GetPieceAtTileIdx(int playerIdx, int tileIdx)
    {
        Transform parentTransform = (playerIdx == 1) ? playerOne.transform : playerTwo.transform;
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
        userTxt.text = GetCurrentPlayerName() + " player turn";
    }

    /// <summary>
    /// Set the game over text.
    /// </summary>
    private void SetUserEndGameText()
    {
        userTxt.text = GetCurrentPlayerName() + " player WON!";
    }

    /// <summary>
    /// Conversion from the player's index of 1, -1 to a string representations.
    /// </summary>
    public string GetPlayerName(int playerIdx)
    {
        string playerName;

        if (playerIdx == 1)
        {
            playerName = "White";
        }
        else
        {
            playerName = "Black";
        }
        
        return playerName;
    }

    public string GetCurrentPlayerName()
    {
        return GetPlayerName(PlayerTurnIdx);
    }

    /// <summary>
    /// Reset the currently picked piece's material to it's original material.
    /// </summary>
    private void UnpickCurrentPieceMaterial()
    {
        // We change the first child, because a piece only contains 1 child with the graphic representation of the piece.
        GameObject child = CurrentClickedPiece.gameObject.transform.GetChild(0).gameObject;
        child.GetComponent<Renderer>().material = originalMaterial;
    }

    /// <summary>
    /// Set the currently picked piece's material to the 'selected' material of a piece.
    /// </summary>
    private void SetSelectedMaterial()
    {
        // We change the first child, because a piece only contains 1 child with the graphic representation of the piece.
        GameObject child = CurrentClickedPiece.gameObject.transform.GetChild(0).gameObject;
        child.GetComponent<Renderer>().material = selectedMaterial;
    }

    /// <summary>
    /// Get a list of all the pieces.
    /// </summary>
    public List<Piece> GetAllPieces()
    {
        List<Piece> pieces = new List<Piece>();
        Transform parentTransform = playerOne.transform;
        foreach (Transform child in parentTransform)
        {
            pieces.Add(child.GetComponent<Piece>());
        }

        parentTransform = playerTwo.transform;
        foreach (Transform child in parentTransform)
        {
            pieces.Add(child.GetComponent<Piece>());
        }

        return pieces;
    }

    /// <summary>
    /// Get a list of all the pieces data.
    /// </summary>
    public List<PieceData> GetAllPiecesData()
    {
        List<PieceData> piecesData = new List<PieceData>();
        Transform parentTransform = playerOne.transform;
        Piece currentPiece;
        foreach (Transform child in parentTransform)
        {
            currentPiece = child.GetComponent<Piece>();
            piecesData.Add(currentPiece.pieceData);
        }

        parentTransform = playerTwo.transform;
        foreach (Transform child in parentTransform)
        {
            currentPiece = child.GetComponent<Piece>();
            piecesData.Add(currentPiece.pieceData);
        }

        return piecesData;
    }

    public GameObject GetParentObjByPlayerIdx(int playerIdx)
    {
        if (playerIdx == 1)
        {
            return playerOne;
        }
        else
        {
            return playerTwo;
        }
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
        foreach (Transform child in playerOne.transform)
        {
            currentPiece = child.GetComponent<Piece>();

            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, false);
            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, currentPiece.pieceData.playerIdx);

            currentTile = TilesMap.Instance.GetTile(currentPiece.pieceData.currentIndex);
            currentPiece.transform.position = currentTile.tile.transform.position;
            currentPiece.moves.Clear();
            currentPiece.attackMoves.Clear();
        }

        foreach (Transform child in playerTwo.transform)
        {
            currentPiece = child.GetComponent<Piece>();

            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, false);
            TilesMap.Instance.UpdateTile(currentPiece.pieceData.currentIndex, currentPiece.pieceData.playerIdx);

            currentTile = TilesMap.Instance.GetTile(currentPiece.pieceData.currentIndex);
            currentPiece.transform.position = currentTile.tile.transform.position;
            currentPiece.moves.Clear();
            currentPiece.attackMoves.Clear();
        }

        CurrentClickedPiece = null;
    }
}
