using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Simulator helps us check what would happen if a piece made a certain move.
/// This is used to check if move will save the player from check or set the player in check.
/// </summary>
public class Simulator : MonoBehaviour
{
    // Attack move simulator properties.
    private Piece attackedPiece;
    private Piece attackerPiece;
    private int attackedPieceOrigTileIdx;
    private int attackerPieceOrigTileIdx;

    // Move simulator properties.
    private Piece piece;
    private int newTileIdx;
    private int previousTileIdx;

    // Only 1 simulation at a time.
    private bool simulationInProgress;

    /// <summary>
    /// Simulate an attack move.
    /// </summary>
    /// <param name="previousTileIdx"> The previous tile that the given attacker piece was on. </param>
    /// <param name="targetTileIdx"> The new tile to set the given attacker piece on. </param>
    /// <param name="attackedPiece"> The piece being attacked in this move. </param>
    /// <param name="AttackerPiece"> The piece making it's attack move on the attackedPiece. </param>
    public void SimulateAttackMove(int previousTileIdx, int targetTileIdx, Piece attackedPiece, Piece AttackerPiece)
    {
        print("SimulateAttackMove from " + previousTileIdx + " to " + targetTileIdx + " by " + AttackerPiece.pieceData.gameObjectName);

        if (!this.simulationInProgress)
        {
            this.simulationInProgress = true;
            this.attackedPiece = attackedPiece;
            this.attackerPiece = AttackerPiece;
            this.attackedPieceOrigTileIdx = targetTileIdx;
            this.attackerPieceOrigTileIdx = previousTileIdx;

            // Simulate the board as thou the user did this move.
            TilesMap.Instance.UpdateTile(previousTileIdx, true);
            TilesMap.Instance.UpdateTile(targetTileIdx, attackerPiece.pieceData.playerIdx);
            this.attackedPiece.gameObject.SetActive(false);
            this.attackerPiece.pieceData.currentIndex = targetTileIdx;
            BoardController.Instance.GameModel.SetAllPiecesMoves(false);
        }
    }

    /// <summary>
    /// Un-simulate the simulated attack move.
    /// </summary>
    public void UnSimulateAttackMove()
    {
        print("UnSimulateAttackMove");

        // Revert the simulated attack move.
        if (simulationInProgress)
        {
            simulationInProgress = false;
            TilesMap.Instance.UpdateTile(attackerPieceOrigTileIdx, false);
            TilesMap.Instance.UpdateTile(attackedPieceOrigTileIdx, attackedPiece.pieceData.playerIdx);
            this.attackedPiece.gameObject.SetActive(true);
            this.attackerPiece.pieceData.currentIndex = attackerPieceOrigTileIdx;
            BoardController.Instance.GameModel.SetAllPiecesMoves(false);
        }
    }

    /// <summary>
    /// Simulate a move.
    /// </summary>
    /// <param name="previousTileIdx"> The previous tile that the given piece was on. </param>
    /// <param name="targetTileIdx"> The new tile to set the given piece on. </param>
    /// <param name="piece"> The moving piece. </param>
    public void SimulateMove(int previousTileIdx, int targetTileIdx, Piece piece)
    {
        print("SimulateMove from " + previousTileIdx + " to " + targetTileIdx + " by " + piece.pieceData.gameObjectName);
        if (!this.simulationInProgress)
        {
            this.simulationInProgress = true;
            this.piece = piece;
            this.newTileIdx = targetTileIdx;
            this.previousTileIdx = previousTileIdx;

            // Simulate the board as thou the user did this move.
            TilesMap.Instance.UpdateTile(previousTileIdx, true);
            TilesMap.Instance.UpdateTile(targetTileIdx, false);
            TilesMap.Instance.UpdateTile(targetTileIdx, piece.pieceData.playerIdx);
            this.piece.pieceData.currentIndex = targetTileIdx;
            BoardController.Instance.GameModel.SetAllPiecesMoves(false);
        }
    }

    /// <summary>
    /// Un-simulate the simulated move.
    /// </summary>
    public void UnSimulateMove()
    {
        print("UnSimulateMove");

        // Revert the simulated attack move.
        if (simulationInProgress)
        {
            simulationInProgress = false;
            TilesMap.Instance.UpdateTile(previousTileIdx, false);
            TilesMap.Instance.UpdateTile(newTileIdx, true);
            this.piece.pieceData.currentIndex = previousTileIdx;
            BoardController.Instance.GameModel.SetAllPiecesMoves(false);
        }
    }
}
