using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject settingsPage = null;

    private SavedGame lastSavedGame;
    private bool settingsOpen;

    void Start()
    {
        settingsOpen = false;
        settingsPage.SetActive(settingsOpen);
        lastSavedGame = new SavedGame();
    }

    /// <summary>
    /// Callback called on 'settings' button clicked. Toggle the settings activity.
    /// </summary>
    public void OnSettingsClicked()
    {
        print("Settings: OnSettingsClicked");

        settingsOpen = !settingsOpen;
        settingsPage.SetActive(settingsOpen);
    }

    /// <summary>
    /// Callback called on 'save' button clicked. Save the game.
    /// </summary>
    public void OnSaveClicked()
    {
        print("Settings: OnSaveClicked");

        // Save all the piece's data and the current turn.
        lastSavedGame.allPiecesData = BoardController.Instance.GetAllPiecesData();
        lastSavedGame.playerTurnIdx = BoardController.Instance.PlayerTurnIdx;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, lastSavedGame);
        file.Close();

        CloseSettings();
    }

    /// <summary>
    /// Callback called on 'load' button clicked. Load the last saved game.
    /// </summary>
    public void OnLoadClicked()
    {
        print("Settings: OnLoadClicked");

        // Load from the cache the saved data.
        BinaryFormatter bf = new BinaryFormatter();
        string filePath = Application.persistentDataPath + "/gamesave.save";
        if (!File.Exists(filePath))
        {
            return;
        }

        FileStream file = File.Open(filePath, FileMode.Open);
        SavedGame lastSavedGame = (SavedGame)bf.Deserialize(file);
        file.Close();

        // All the pieces in hierarchy.
        // If the saved piece is in the hierarchy - then just update its data.
        // Else - Create a new piece and set under the hierarchy.
        List<Piece> allPiecesInHierarchy = BoardController.Instance.GetAllPieces();

        foreach (Piece piece in allPiecesInHierarchy)
        {
            // Set all the pieces in the hierarchy as dead.
            piece.gameObject.SetActive(false);

            // Try to find the piece from the hierarchy in the saved game. If we've found - then update its data.
            PieceData currentPieceData;
            for (int i = 0; i < lastSavedGame.allPiecesData.Count; i++)
            {
                currentPieceData = lastSavedGame.allPiecesData[i];
                if (currentPieceData.gameObjectName == piece.gameObject.name)
                {
                    if (!currentPieceData.isDead)
                    {
                        piece.gameObject.SetActive(true);
                        piece.SetPieceData(currentPieceData);
                    }

                    // Remove it so we keep track of the saved pieces that aren't in the hierarchy.
                    lastSavedGame.allPiecesData.RemoveAt(i);
                    i--;
                }
            }
        }

        // Create all the saved pieces that don't exist in the hierarchy.
        string currentPlayerName;
        GameObject newPiece, parentObj;
        foreach (PieceData pieceData in lastSavedGame.allPiecesData)
        {
            currentPlayerName = BoardController.Instance.GetPlayerName(pieceData.playerIdx);
            print("Instantiate " + currentPlayerName + "/" + pieceData.prefabName);
            newPiece = Instantiate(Resources.Load(currentPlayerName + "/" + pieceData.prefabName) as GameObject);
            parentObj = BoardController.Instance.GetParentObjByPlayerIdx(pieceData.playerIdx);
            newPiece.name = pieceData.gameObjectName;
            newPiece.transform.SetParent(parentObj.transform);

            newPiece.GetComponent<Piece>().SetPieceData(pieceData);
        }

        // Reset the board based on the current data of all the pieces.
        BoardController.Instance.ResetBoardPieces();

        // Set the turn according to the saved state turn.
        BoardController.Instance.PlayerTurnIdx = lastSavedGame.playerTurnIdx;
        BoardController.Instance.SetUserTurnText();
        
        CloseSettings();
    }

    /// <summary>
    /// Callback called on 'close' button clicked. Close the settings activity.
    /// </summary>
    public void OnCloseSettingsClicked()
    {
        print("Settings: OnCloseSettingsClicked");

        CloseSettings();
    }

    /// <summary>
    /// Close the settings activity.
    /// </summary>
    private void CloseSettings()
    {
        settingsOpen = false;
        settingsPage.SetActive(settingsOpen);
    }
}
