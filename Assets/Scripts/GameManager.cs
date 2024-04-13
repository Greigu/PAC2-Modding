using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    private PlayerController playerController;
    //private Tilemap map;
    private int currentLvL = 1;
    private float time;
    private int pushes;
    private int moves = 0;
    private GameObject currentLevel;
    [SerializeField] private GameObject[] levels = new GameObject[10];
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text lvLText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text pushesText;
    private GameObject[] scoreTiles;

    private Vector3 playerPos;
    private List<Vector3> boxesPos = new List<Vector3>();

    private bool end = false;
    // Start is called before the first frame update
    void Start()
    {
        DeactivateLevels();
        LoadNextLevel();
        GetScores();
        GetActualPlayerController();
        GetResetPositions();
    }

    // Update is called once per frame
    void Update()
    {
        CheckScores();
        moves = playerController.GetMoves();
        movesText.text = "Moves: " + moves.ToString();
        pushes = playerController.GetPushes();
        pushesText.text = "Pushes:" + pushes.ToString();
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }
    }

    private void FixedUpdate()
    {
        UpdateTime();
        UpdateLevel();
    }

    private void GetScores()
    {
        scoreTiles = GameObject.FindGameObjectsWithTag("Score");
        foreach (var item in scoreTiles)
        {
            //Debug.Log("+1Score");
        }
    }

    private void UpdateTime()
    {
        time += Time.deltaTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
        timeText.text = timeString;
    }

    private void UpdateLevel()
    {
        lvLText.text = currentLvL.ToString();
    }
    private void DeactivateLevels()
    {
        foreach (var level in levels)
        {
            if (level != null)
            {
                //Debug.Log("+1LvL");
                level.SetActive(false);
            }
        }
    }

    private void CheckScores()
    {
        int totalScore = 0;
        foreach(var score in scoreTiles)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(score.gameObject.transform.position);
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    if (collider.gameObject.tag == "Box")
                        totalScore++;
                    //Debug.Log(collider.gameObject.name);
                }
            }
        }
        if(totalScore == scoreTiles.Length) { 
            NextLevel();
        }
       
    }

    private void GetActualPlayerController()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void NextLevel()
    {
        if (currentLvL >= levels.Length)
        {
            EndGame();
        } 
        else
        {
            Debug.Log("Win");
            currentLvL++;
            DeactivateLevels();
            LoadNextLevel();
            GetScores();
            GetActualPlayerController();
            GetResetPositions();
        }
    }

    private void EndGame()
    {
        Debug.Log("End Game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        end = true;
    }

    private void LoadNextLevel()
    {
        if(currentLvL - 1 <= levels.Length)
        {
            if(levels[currentLvL - 1] != null)
            {
                currentLevel = levels[currentLvL - 1];
                currentLevel.SetActive(true);
            } else
            {
                EndGame();
            }

        }
        else
        {
            EndGame();
        }
        
    }

    private void GetResetPositions()
    {
        boxesPos.Clear();
        foreach(Transform child in currentLevel.transform)
        {
            if(child.tag == "Box")
            {
                Debug.Log(child.gameObject.name);
                boxesPos.Add(child.gameObject.transform.position);
            }
        }
        playerPos = playerController.gameObject.transform.position;
    }

    private void ResetLevel()
    {
        int i = 0;
        foreach (Transform child in currentLevel.transform)
        {
            if (child.tag == "Box")
            {
                Debug.Log("a"+boxesPos.Count);
                Debug.Log(i);
                Debug.Log(boxesPos[i]);
                child.gameObject.transform.position = boxesPos[i];
                i++;
            }
            
        }
        playerController.gameObject.transform.position = playerPos;
        playerController.CheckSurroundings(Vector3Int.FloorToInt(playerPos));
    }

}
