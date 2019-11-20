using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateMaze : MonoBehaviour
{
    private MazeDataGenerator dataGenerator;
    [SerializeField] private GameObject wall, wallTresure, pillar, sarcophagus, barricade, player;
    [SerializeField] private int wallChance, treasureChance, sarcophhagusChance, barricadeChance, pillarChance;
    int[,] data;
    [SerializeField] private float cellSize = 3f;
    private GameObject terrain;
    private GameObject[] weightedObstacles;

    // Start is called before the first frame update
    void Awake(){
        dataGenerator = new MazeDataGenerator();
        terrain = GameObject.FindWithTag("Terrain");
    }

    private void Start()
    {
        float width = terrain.GetComponent<Renderer>().bounds.max.x;
        float length = terrain.GetComponent<Renderer>().bounds.max.z;

        weightedObstacles = new GameObject[wallChance + treasureChance + sarcophhagusChance + barricadeChance + pillarChance];
        prepObstacles();
        
        GenerateNewMaze(length, width);
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private void prepObstacles()
    {
        Debug.Log(weightedObstacles);
        int index = 0;
        for (int i = 0; i < treasureChance; i++)
            weightedObstacles[index++] = wallTresure;
        for (int i = 0; i < barricadeChance; i++)
            weightedObstacles[index++] = barricade;
        for (int i = 0; i < sarcophhagusChance; i++)
            weightedObstacles[index++] = sarcophagus;
        for (int i = 0; i < pillarChance; i++)
            weightedObstacles[index++] = pillar;
        for (int i = 0; i < wallChance; i++)
            weightedObstacles[index++] = wall;
    }

    public void GenerateNewMaze(float length, float width){
        data = dataGenerator.FromDimensions((int)(length/cellSize), (int)(width/cellSize));
        bool playerPlaced = false;
        
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] != 0)
                {
                    
                    if (i == 0 || j == 0 || i == rMax || j == cMax)
                        PlaceObstacle(i, j, wall);
                    else
                        PlaceObstacle(i, j);
                }
                else
                {
                    if (!playerPlaced)
                    {
                        playerPlaced = true;
                        GameObject.FindWithTag("Player").transform.position = new Vector3(i * cellSize, 0, j * cellSize);
                    }
                    
                }
            }
            
        }
    }

    private Quaternion randomCardinalRotation()
    {
        int rotation = Random.Range(0, 3);
        return Quaternion.Euler(0, 90 * rotation, 0);
    }
    
    private Quaternion randomFreeformRotation()
    {
        int rotation = Random.Range(0, 360);
        return Quaternion.Euler(0, rotation, 0);
    }

    private void PlaceObstacle(int i, int j, GameObject obstacle = null)
    {
        if (obstacle == null)
            obstacle = weightedObstacles[Random.Range(0, weightedObstacles.Length)];

        float length = terrain.transform.localScale.x;
        float width = terrain.transform.localScale.z;
        
        float obstacleHeight = obstacle.GetComponent<Renderer>().bounds.max.y;
        
        Vector3 obstacleLocation = new Vector3(i * cellSize - length / 2,
            obstacleHeight, j * cellSize - width / 2);
        Quaternion obstacleRotation = obstacle == barricade ? randomFreeformRotation() : randomCardinalRotation();
        
        Instantiate(obstacle,
            obstacleLocation,
            obstacleRotation,
            transform);
    }


}
