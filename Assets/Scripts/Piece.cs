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
        pieceData.madeFirstMove = false;
        pieceData.hasPostPlayAction = false;
        pieceData.isKing = false;
        pieceData.isDead = false;
        pieceData.gameObjectName = gameObject.name;
        BoardController.Instance.SetPieceOnTile(pieceData.currentIndex, this);
    }

    /// <summary>
    /// Given a target tile - add the move to that tile to the moves/attackMoves list.
    /// </summary>
    /// <param name="tileIdx"> THe target tile move.</param>
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
    public virtual void SetMoves(){}


    public virtual void SetPostPlayAction(int previousTileIdx, int targetTileIdx) {}

    void OnDisable()
    {
        pieceData.isDead = true;
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
}
