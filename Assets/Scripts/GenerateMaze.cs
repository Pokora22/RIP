using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    private MazeDataGenerator dataGenerator;
    [SerializeField] private GameObject wall, wallTresure, pillar, sarcophagus, barricade;
    [SerializeField] private float wallChance, treasureChance, sarcophhagusChance, barricadeChance, pillarChance;
    int[,] data;
    [SerializeField] private float cellSize = 3f;
    private GameObject terrain;

    // Start is called before the first frame update
    void Awake(){
        dataGenerator = new MazeDataGenerator();
        terrain = GameObject.FindWithTag("Terrain");
    }

    private void Start()
    {
        GenerateNewMaze(terrain.transform.localScale.x, terrain.transform.localScale.z);
    }

    public void GenerateNewMaze(float length, float width){
        data = dataGenerator.FromDimensions((int)(length/cellSize), (int)(width/cellSize));
        
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
        {
            float rng = Random.Range(0,
                treasureChance + barricadeChance + sarcophhagusChance + pillarChance + wallChance);
            if (rng < treasureChance)
                obstacle = wallTresure;
            else if (rng < barricadeChance)
                obstacle = barricade;
            else if (rng < sarcophhagusChance)
                obstacle = sarcophagus;
            else if (rng < pillarChance)
                obstacle = pillar;
            else
                obstacle = wall;
        }


        float length = terrain.transform.localScale.x;
        float width = terrain.transform.localScale.z;
        float height = terrain.transform.localScale.y;
        float obstacleHeight = obstacle.GetComponent<Renderer>().bounds.max.y;
        
        Vector3 obstacleLocation = new Vector3(i * cellSize - length / 2,
            height/2 + obstacleHeight, j * cellSize - width / 2);
        Quaternion obstacleRotation = obstacle == barricade ? randomFreeformRotation() : randomCardinalRotation();
        
        if(obstacle == barricade)
            Debug.Log("Height: " + obstacle.GetComponent<Renderer>().bounds.max.y);
        
        Instantiate(obstacle,
            obstacleLocation,
            obstacleRotation,
            transform);
    }


}
