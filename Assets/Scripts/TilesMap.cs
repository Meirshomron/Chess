using UnityEngine;

public class Tile
{
    public GameObject tile;
    public bool isEmpty;
    public int playerIdxOnTile;
    public Color origColor;
    public Color currColor;
}

public class TilesMap : MonoBehaviour
{
    private static TilesMap _instance;

    public static TilesMap Instance { get => _instance; }

    [SerializeField] private GameObject tilePrefab = null;
    public static int totalColomns = 8;
    public static int totalRows = 8;

    private Tile[] tilesArr;
    private float startingX;
    private float startingY;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        tilesArr = new Tile[totalRows * totalColomns];
        CreateTileMap();
    }

    /// <summary>
    /// Create the map by initiating all the tiles.
    /// </summary>
    private void CreateTileMap()
    {
        Vector3 tileScale = tilePrefab.transform.localScale;
        float tileX, tileY;

        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        startingX = (screenBounds.x - (tileScale.x * totalColomns)) / 2.0f;
        startingY = (screenBounds.y - (tileScale.z * totalRows)) / 2.0f;

        ColorUtility.TryParseHtmlString("#A6A6A", out Color color);

        for (int row = 0; row < totalRows; row++)
        {
            for (int col = 0; col < totalColomns; col++)
            {
                tileX = startingX + (col * tileScale.x);
                tileY = startingY + (row * tileScale.z);

                int currentTileIdx = (row * totalRows) + col;

                tilesArr[currentTileIdx] = new Tile
                {
                    tile = Instantiate(tilePrefab, new Vector3(tileX, 0, tileY), Quaternion.identity)
                };
                tilesArr[currentTileIdx].tile.transform.SetParent(this.transform);
                tilesArr[currentTileIdx].tile.name = currentTileIdx.ToString();
                tilesArr[currentTileIdx].isEmpty = true;

                if ((col + row) % 2 == 0)
                {
                    SetTileColor(currentTileIdx, color);
                }
                tilesArr[currentTileIdx].origColor = tilesArr[currentTileIdx].tile.GetComponent<MeshRenderer>().material.color;
            }
        }
    }

    /// <summary>
    /// Update the given tile index to hold the given player's index.
    /// </summary>
    public void UpdateTile(int tileIdx, int playerIdx)
    {
        tilesArr[tileIdx].playerIdxOnTile = playerIdx;
    }

    /// <summary>
    /// Update the given tile index's empty status.
    /// </summary>
    public void UpdateTile(int tileIdx, bool isEmpty)
    {
        tilesArr[tileIdx].isEmpty = isEmpty;
    }

    /// <summary>
    /// Set the current color of the tile at the given tile index with the given color.
    /// </summary>
    public void SetTileColor(int tileIdx, Color colorToSet)
    {
        tilesArr[tileIdx].currColor = colorToSet;
        tilesArr[tileIdx].tile.GetComponent<MeshRenderer>().material.color = colorToSet;
    }

    /// <summary>
    /// Set the current color of the tile at the given tile index with the original color of that tile.
    /// </summary>
    public void SetTileOrigColor(int tileIdx)
    {
        SetTileColor(tileIdx, tilesArr[tileIdx].origColor);
    }

    /// <summary>
    /// Getter for the tile at the given tile index.
    /// </summary>
    public Tile GetTile(int tileIdx)
    {
        return tilesArr[tileIdx];
    }
}
