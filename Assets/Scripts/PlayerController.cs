using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    private Tilemap map;
    private Vector3Int tileSize = new Vector3Int(1,1);
    private float moveSpeed = 5f;
    private Vector3 targetPosition;
    private Vector3Int mapMovement = new Vector3Int(-10, 6, 0);
    private bool isMoving;
    private GameObject[] surr = new GameObject[4];
    Vector3Int[] offsets = new Vector3Int[]
            {
            new Vector3Int(1, 0, 0), // Right 0
            new Vector3Int(-1, 0, 0), // Left 1
            new Vector3Int(0, 1, 0), // Up 2
            new Vector3Int(0, -1, 0) // Down 3
            };
    private bool[] canGo = new bool[4];

    private int moves;
    private int pushes;

    private List<Vector3> boxPos = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<Tilemap>();
        for (int i = 0; i < canGo.Length; i++)
        {
            canGo[i] = true;
        }
        CheckSurroundings(Vector3Int.FloorToInt(targetPosition));

    }

    // Update is called once per frame
    void Update()
    {
        CheckMovementPressed();
    }

    private void CheckMovementPressed()
    {

        if (!isMoving)
        {
            //_ = new Vector3Int();
            Vector3Int dir;
            if (Input.GetKeyDown(KeyCode.A) && canGo[1])
            {
                dir = Vector3Int.left;
                Move(dir);
            }
            else if (Input.GetKeyDown(KeyCode.D) && canGo[0])
            {
                dir = Vector3Int.right;
                Move(dir);
            }

            else if (Input.GetKeyDown(KeyCode.W) && canGo[2])
            {
                dir = Vector3Int.up;
                Move(dir);
            }

            else if (Input.GetKeyDown(KeyCode.S) && canGo[3])
            {
                dir = Vector3Int.down;
                Move(dir);
            }
            
        }
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (transform.position == targetPosition)
            {
                moves++;
                isMoving = false;
                if(boxPos.Count > 0)
                {
                    foreach (var pos in boxPos)
                    {
                        if ( map.WorldToCell(pos) == map.WorldToCell(targetPosition))
                        {
                            pushes++;
                        }
                    }
                    boxPos.Clear();
                }
                
                CheckSurroundings(Vector3Int.FloorToInt(targetPosition));
            }
           
        }
    }

    private void Move(Vector3Int direction)
    {
        // Calculate target position based on tile size and input direction
        targetPosition = transform.position + direction * tileSize;
        
        isMoving = true;
    }

    public void CheckSurroundings( Vector3Int playerPos)
    {
            Vector3Int[] offsets = new Vector3Int[]
            {
            new Vector3Int(1, 0, 0), // Right 0
            new Vector3Int(-1, 0, 0), // Left 1
            new Vector3Int(0, 1, 0), // Up 2
            new Vector3Int(0, -1, 0) // Down 3
            };

        // Iterate through the neighboring tile positions
        int i = 0;
        Array.Clear(surr, 0, surr.Length);
        foreach (Vector3Int offset in offsets)
            {
            
                Vector3Int neighborTilePosition = playerPos + offset - mapMovement;
                // Get the tile at the neighbor position
                TileBase neighborTile = map.GetTile(neighborTilePosition);
                // Check for game objects in the same position as the neighbor tile
                Vector3 neighborWorldPosition = map.GetCellCenterWorld(neighborTilePosition);
                Collider2D[] colliders = Physics2D.OverlapPointAll(neighborWorldPosition);
                canGo[i] = true;
                foreach (Collider2D collider in colliders)
                    {
                        surr[i] = collider.gameObject;
                if (surr[i].tag == "Wall") {
                    canGo[i] = false;
                } else if(surr[i].tag == "Box")
                {
                    canGo[i] = checkNextPos(i, neighborWorldPosition);
                    Debug.Log(transform.position);
                    Debug.Log(surr[i].transform.position);
                    boxPos.Add(surr[i].transform.position);
                }
                    }
                i++;
                }
    }
    private bool checkNextPos(int dir, Vector3 pos)
    {
        Vector3 newCheckPos = pos + offsets[dir];
        Collider2D[] colliders = Physics2D.OverlapPointAll(newCheckPos);
        foreach (Collider2D collider in colliders)
        {
            if (collider.tag == "Wall" || collider.tag =="Box")
            {
                return false;
            } else { 
                return true; 
            }
        }
        return true;
    }
    public int GetMoves() {
        return moves;
    }
    public int GetPushes()
    {
        return pushes;
    }
    
}
