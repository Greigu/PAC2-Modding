using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class LevelEditorWindow : EditorWindow
{
    private Texture2D[] selectableIcons = new Texture2D[5];
    private int selectedIconIndex = 0;
    private Texture2D[,] matrix = new Texture2D[12, 20];

    private GameObject player;
    private GameObject wall;
    private GameObject box;
    private GameObject scorePlace;
    private GameObject ground;

    private string lvlName;

    private GameObject[] gameObjects = new GameObject[5];
    private bool showPrefabs = false;
    private bool showMatrix = false;
    private Tile groundTile;


    [MenuItem("Sokoban/Level Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LevelEditorWindow), false, "Level Editor", true);
    }

    private void OnEnable()
    {
        player = Resources.Load("Player", typeof(GameObject)) as GameObject;
        wall = Resources.Load("Wall", typeof(GameObject)) as GameObject;
        box = Resources.Load("Box", typeof(GameObject)) as GameObject;
        scorePlace = Resources.Load("ScorePlace", typeof(GameObject)) as GameObject;
        ground = Resources.Load("Ground", typeof(GameObject)) as GameObject;
       
        gameObjects[0] = player;
        gameObjects[1] = wall;
        gameObjects[2] = box;
        gameObjects[3] = scorePlace;
        gameObjects[4] = ground;

    }

    private void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.padding = new RectOffset(0, 0, 0, 0);
        buttonStyle.margin = new RectOffset(0, 0, 0, 0);
        buttonStyle.border = new RectOffset(0, 0, 0, 0);
       
        showPrefabs = EditorGUILayout.Foldout(showPrefabs, "Prefabs");

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginFadeGroup(showPrefabs ? 1 : 0);
        if (showPrefabs)
        {
            GUILayout.Label("Player", EditorStyles.boldLabel);
            player = EditorGUILayout.ObjectField(player, typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
            GUILayout.Label("Wall", EditorStyles.boldLabel);
            wall = EditorGUILayout.ObjectField(wall, typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
            GUILayout.Label("Box", EditorStyles.boldLabel);
            box = EditorGUILayout.ObjectField(box, typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
            GUILayout.Label("Score Place", EditorStyles.boldLabel);
            scorePlace = EditorGUILayout.ObjectField(scorePlace, typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
            GUILayout.Label("Ground", EditorStyles.boldLabel);
            ground = EditorGUILayout.ObjectField(ground, typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUI.indentLevel--;


        showMatrix = EditorGUILayout.Foldout(showMatrix, "Level Editor");
       
        EditorGUI.indentLevel++;
       
        if (showMatrix)
        {
            EditorGUILayout.BeginFadeGroup(showMatrix ? 1 : 0);
            GUILayout.Label("Name");
            lvlName = EditorGUILayout.TextField(lvlName);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (GUILayout.Button(gameObjects[i].GetComponent<SpriteRenderer>().sprite.texture, buttonStyle, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    selectedIconIndex = i;
                }
                GUILayout.Space(0); 
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("Icon Matrix", EditorStyles.boldLabel);

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                   
                    if (GUILayout.Button(matrix[i,j] ?? gameObjects[4].GetComponent<SpriteRenderer>().sprite.texture, buttonStyle, GUILayout.Height(30), GUILayout.Width(30)))
                    {
                        matrix[i, j] = gameObjects[selectedIconIndex].GetComponent<SpriteRenderer>().sprite.texture;
                    }
                    GUILayout.Space(0);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(0);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Level"))
            {
                ClearLevel();
            }
            if (GUILayout.Button("Export Level"))
            {
                ExportLevel();
            }
            if (GUILayout.Button("Import Level"))
            {
                ImportLevel();
            }
            if (GUILayout.Button("Set Level"))
            {
                GenerateTilemap();
            }

            GUILayout.EndHorizontal();

            
            
           

            GUILayout.Space(10);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUI.indentLevel--;        
    }

    private void ClearLevel()
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = null;
            }
        }
    }

    private void ExportLevel()
    {
        string filePath = EditorUtility.SaveFilePanel("Save Level", "", "Level_.txt", "txt");

        if (!string.IsNullOrEmpty(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] != null)
                        {
                            string textureName = matrix[i, j].name;
                            writer.Write(textureName);
                            Debug.Log(textureName);

                        } else
                        {
                            writer.Write(gameObjects[4].GetComponent<SpriteRenderer>().sprite.name);
                        }
                        
                    }
                    writer.WriteLine();
                }
            }

            Debug.Log("Level saved:" + filePath);
        }
    }
    private void ImportLevel()
    {
        string filePath = EditorUtility.OpenFilePanel("Load Level", "", "txt");

        if (!string.IsNullOrEmpty(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    string line = reader.ReadLine();
                    char[] textureNames = line.ToCharArray();

                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (textureNames[j] != null)
                        {
                            AssignTexture(textureNames[j], i, j);
                        }
                    }
                }
            }

            Debug.Log("Level Loaded" + filePath);
        }
    }

    private void AssignTexture(char letter, int i, int j)
    {
        /* 
        gameObjects[0] = player;
        gameObjects[1] = wall;
        gameObjects[2] = box;
        gameObjects[3] = scorePlace;
        gameObjects[4] = ground;*/
        if (letter.Equals('P')) {       // Player
            matrix[i, j] = Assing(0);
        }
        else if (letter.Equals('W'))    // Wall
        {
            matrix[i, j] = Assing(1);
        }
        else if (letter.Equals('B'))    // Box
        {
            matrix[i, j] = Assing(2);
        }
        else if (letter.Equals('S'))    // Score Place
        {
            matrix[i, j] = Assing(3);
        }
        else if (letter.Equals('G'))    // Ground
        {
            matrix[i, j] = Assing(4);
        }

    }

    private Texture2D Assing(int index)
    {
        return gameObjects[index].gameObject.GetComponent<SpriteRenderer>().sprite.texture;
    }
    private void SetLevel()
    {

    }

    private void GenerateTilemap()
    {
        
        GameObject tilemapGO = new GameObject(lvlName == "" ? "Level" : lvlName);
        tilemapGO.AddComponent<Grid>();
        Tilemap tilemap = tilemapGO.AddComponent<Tilemap>();
        tilemapGO.AddComponent<TilemapRenderer>();
        tilemapGO.transform.position = new Vector3(-10, 6, 0);
        tilemapGO.gameObject.tag = "Map";
        groundTile = CreateInstance<Tile>();
        groundTile.sprite = ground.GetComponent<SpriteRenderer>().sprite;


        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Vector3Int cellPosition = new Vector3Int(j, -i, 0);
                Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
                GameObject instantiatedPrefab = PrefabUtility.InstantiatePrefab(GetObject(i,j)) as GameObject;
                if(instantiatedPrefab != null)
                {
                    instantiatedPrefab.transform.position = worldPosition;
                    instantiatedPrefab.transform.parent = tilemap.transform;
                }
                tilemap.SetTile(tilemap.WorldToCell(worldPosition), groundTile);
                tilemap.SetTile(cellPosition, groundTile);
            }
        }

        Debug.Log("Tilemap generated.");
        tilemapGO.SetActive(false);
    }

    private GameObject GetObject(int i, int j)
    {
        if (matrix[i,j] == null || (matrix[i, j].name.Equals("G")))
        {
            return null;
        }
        else
        {
            Debug.Log(matrix[i,j].name);
            if (matrix[i, j].name.Equals("P")){
                return player;
            } 
            else if (matrix[i, j].name.Equals("W"))
            {
                return wall;
            }
            else if (matrix[i, j].name.Equals("B"))
            {
                return box;
            }
             else if (matrix[i, j].name.Equals("S"))
            {
                return scorePlace;
            }

        }
        return null;
    }

}
