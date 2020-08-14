using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The data structure of a piece.
/// This structure is what we save and load, so it must hold all the status data of a piece.
/// </summary>
[System.Serializable]
public class PieceData
{
    public int playerIdx;
    public int currentIndex;
    public bool madeFirstMove;
    public bool hasPostPlayAction;
    public bool isKing;
    public bool movesAreHarmless;
    public string prefabName;
    public string gameObjectName;
    public bool isDead;
}

/// <summary>
/// The parent of all the piece types.
/// </summary>
public class Piece : MonoBehaviour
{
    public List<int> moves;
    public List<int> attackMoves;
    public PieceData pieceData;

    protected virtual void Start()
    {
        // Initial default values of the piece.
        pieceData.madeFirstMove = false;
        pieceData.hasPostPlayAction = false;
        pieceData.isKing = false;
        pieceData.movesAreHarmless = false;
        pieceData.isDead = false;
        pieceData.gameObjectName = gameObject.name;

        // Set this piece at this index in the tilesMap.
        BoardController.Instance.SetPieceOnTile(pieceData.currentIndex, this);
    }

    /// <summary>
    /// Given a target tile - add the move to that tile to the moves/attackMoves list.
    /// </summary>
    /// <param name="tileIdx"> The target tile move.</param>
    /// <returns> Return true if the tile target was added to the attackMoves. </returns>
    protected bool AddMoveToList(int tileIdx)
    {
        bool addedToAttackMoves = false;
        Tile targetTile = TilesMap.Instance.GetTile(tileIdx);
        if (targetTile.isEmpty)
        {
            moves.Add(tileIdx);
        }
        else
        {
            if (targetTile.playerIdxOnTile == -1 * pieceData.playerIdx)
            {
                attackMoves.Add(tileIdx);
            }
            addedToAttackMoves = true;
        }

        return addedToAttackMoves;
    }

    /// <summary>
    /// Inherit classes MUST implement this function.
    /// </summary>
    public virtual void SetMoves(bool validateMoves) {}

    /// <summary>
    /// Validate that the moves of this piece dont set the piece's king in check.
    /// </summary>
    public virtual void ValidateMoves()
    {
        //print("Piece: ValidateMoves name = " + pieceData.gameObjectName);

        bool moveNotValid;

        // We duplicate the moves because they change throughout the simulations.
        List<int> movesCopy = DuplicateMovesList(moves);

        for (int i = 0; i < movesCopy.Count; i++)
        {
            BoardController.Instance.Simulator.SimulateMove(pieceData.currentIndex, movesCopy[i], this);
            moveNotValid = BoardController.Instance.CheckIfPlayerIsChecked(pieceData.playerIdx);
            if (moveNotValid)
            {
                movesCopy.RemoveAt(i);
                i--;
            }
            BoardController.Instance.Simulator.UnSimulateMove();
        }
        moves.Clear();
        moves = movesCopy;
    }

    /// <summary>
    /// Validate that the attack moves of this piece dont set the piece's king in check.
    /// </summary>
    public virtual void ValidateAttackMoves()
    {
        //print("Piece: ValidateAttackMoves name = " + pieceData.gameObjectName);

        bool moveNotValid;

        // We duplicate the attackMoves because they change throughout the simulations.
        List<int> attackMovesCopy = DuplicateMovesList(attackMoves);
        List<int> movesCopy = DuplicateMovesList(moves);

        for (int i = 0; i < attackMovesCopy.Count; i++)
        {
            BoardController.Instance.Simulator.SimulateAttackMove(pieceData.currentIndex, attackMovesCopy[i], BoardController.Instance.GetPieceAtTileIdx(pieceData.playerIdx * -1, attackMovesCopy[i]), this);
            moveNotValid = BoardController.Instance.CheckIfPlayerIsChecked(pieceData.playerIdx);
            if (moveNotValid)
            {
                attackMovesCopy.RemoveAt(i);
                i--;
            }
            BoardController.Instance.Simulator.UnSimulateAttackMove();
        }
        attackMoves.Clear();
        moves.Clear();
        attackMoves = attackMovesCopy;
        moves = movesCopy;
    }

    public virtual void SetPostPlayAction(int previousTileIdx, int targetTileIdx) {}

    void OnDisable()
    {
        pieceData.isDead = true;
    }

    void OnEnable()
    {
        pieceData.isDead = false;
    }

    /// <summary>
    /// Setter for the pieceData.
    /// </summary>
    /// <param name="pieceDataToSet"> The pieceData to override the current data. </param>
    public void SetPieceData(PieceData pieceDataToSet)
    {
        pieceData.playerIdx = pieceDataToSet.playerIdx;
        pieceData.currentIndex = pieceDataToSet.currentIndex;
        pieceData.madeFirstMove = pieceDataToSet.madeFirstMove;
        pieceData.isKing = pieceDataToSet.isKing;
        pieceData.isDead = pieceDataToSet.isDead;
        pieceData.hasPostPlayAction = pieceDataToSet.hasPostPlayAction;
    }

    /// <summary>
    /// Duplicate the moves list, used to duplicate the moves/attackMoves list.
    /// </summary>
    protected List<int> DuplicateMovesList(List<int> moves)
    {
        List<int> movesCopy = new List<int>();
        foreach (var move in moves)
        {
            movesCopy.Add(move);
        }
        return movesCopy;
    }
}
