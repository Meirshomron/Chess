using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The structure of a saved game data.
/// </summary>
[System.Serializable]
public class SavedGame
{
    // The index of the current player's turn.
    public int playerTurnIdx;

    // All the pieces in the game.
    public List<PieceData> allPiecesData;
}
