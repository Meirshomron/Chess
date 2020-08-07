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

        // Update the data and status of every piece.
        List<Piece> allPieces = BoardController.Instance.GetAllPieces();
        foreach (Piece piece in allPieces)
        {
            foreach (PieceData savedPieceData in lastSavedGame.allPiecesData)
            {
                if (savedPieceData.name == piece.gameObject.name)
                {
                    if (savedPieceData.isDead)
                    {
                        piece.gameObject.SetActive(false);
                    }
                    else
                    {
                        piece.gameObject.SetActive(true);
                        piece.SetPieceData(savedPieceData);
                    }
                }
            }
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
