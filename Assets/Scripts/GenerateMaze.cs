using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GenerateMaze : MonoBehaviour
{
    private MazeDataGenerator dataGenerator;
    [SerializeField] private GameObject wall, wallTresure, pillar, sarcophagus, barricade, light;
    [SerializeField] private int wallChance, treasureChance, sarcophhagusChance, barricadeChance, pillarChance, lightingDensity;
    [SerializeField] private bool generateMaze;
    int[,] data;
    [SerializeField] private float cellSize = 3f;
    private GameObject terrain;
    private GameObject[] weightedObstacles, restrictedZones;

    // Start is called before the first frame update
    void Awake(){
        dataGenerator = new MazeDataGenerator();
        terrain = GameObject.FindWithTag("Terrain");
    }

    private void Start()
    {
        float length = terrain.GetComponent<Renderer>().bounds.max.x;
        float width = terrain.GetComponent<Renderer>().bounds.max.z;

        weightedObstacles = new GameObject[wallChance + treasureChance + sarcophhagusChance + barricadeChance + pillarChance];
        prepObstacles();
        restrictedZones = GameObject.FindGameObjectsWithTag("Restricted");

        if (generateMaze)
        {
            GenerateNewMaze(length, width);
            PlaceLights();
        }

        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private void PlaceLights()
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        //Ignore edges
        for (int i = 1; i < rMax; i++)
        {
            for (int j = 1; j < cMax; j++)
            {
                if (data[i, j] != 0)
                {
                    Transform wallTransform = null;
                    Collider[] colliders = Physics.OverlapSphere(new Vector3(i * cellSize, 1.5f, j * cellSize), .1f);
                    if (colliders.Length > 0 && colliders[0].CompareTag("Wall"))
                    {
                        wallTransform = colliders[0].transform;
                        bool lightInRange = false;
                        colliders = Physics.OverlapSphere(wallTransform.position, lightingDensity);
                        foreach (Collider c in colliders)
                        {
                            if (c.CompareTag("Light"))
                            {
                                lightInRange = true;
                                break;
                            }
                        }

                        if (!lightInRange)
                        {
                            Quaternion rotation = ForwardFreeRotation(i, j);
                            Instantiate(light,
                                wallTransform.position,
                                rotation,
                                wallTransform);
                        }
                    }
                }
            }
        }
    }

    private void prepObstacles()
    {
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
    
    private bool IsRestricted(Vector3 point)
    {
        foreach (GameObject zone in restrictedZones)
        {
            Collider c = zone.GetComponent<Collider>();
            Vector3 closest = c.ClosestPoint(point);
            // Because closest=point if point is inside - not clear from docs I feel
            if(closest == point) return true;
        }

        return false;
    }

    public void GenerateNewMaze(float length, float width){
        data = dataGenerator.FromDimensions((int)(length/cellSize), (int)(width/cellSize));
        bool playerPlaced = false;
        
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
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
                //Predetermined player position in restricted zones instead
//                else if (!playerPlaced)
//                    {
//                        playerPlaced = true;
//                        GameObject.FindWithTag("Player").transform.position = new Vector3(i * cellSize, 0, j * cellSize);
//                    }
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
    
    private Quaternion ForwardFreeRotation(int i, int j)
    {
        List<int> rotations = new List<int>();
        
        //Check left
        if(data[i - 1, j] == 0)
            rotations.Add(270);
        //Check up
        if(data[i, j - 1] == 0)
            rotations.Add(0);
        //Check right
        if(data[i + 1, j] == 0)
            rotations.Add(90);
        //Check down
        if(data[i, j + 1] == 0)
            rotations.Add(180);

        int rotation = rotations[Random.Range(0, rotations.Count)];
        return Quaternion.Euler(new Vector3(0, rotation, 0));
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
        Quaternion obstacleRotation = obstacle == barricade ? randomFreeformRotation() : obstacle == wallTresure ? ForwardFreeRotation(i, j) : randomCardinalRotation();
        
        if(!IsRestricted(obstacleLocation))
            Instantiate(obstacle,
                obstacleLocation,
                obstacleRotation,
                transform);
    }


}
