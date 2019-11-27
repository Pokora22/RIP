using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Sisus.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GenerateMaze : MonoBehaviour
{
    private MazeDataGenerator dataGenerator;
    [SerializeField] private GameObject[] wall, wallTresure, pillar, sarcophagus, barricade, light, exit;
    [SerializeField] private int wallChance, treasureChance, sarcophhagusChance, barricadeChance, pillarChance, lightingDensity, numberOfExits, exitWidthOffset, exitLengthOffset;
    [SerializeField] private bool generateMaze, randomExits, alwaysTurnTowardsFree;
    [EditorBrowsable] private Dictionary<int, string> aiSpawns;
    int[,] data;
    [SerializeField] private float cellSize = 3f;
    private GameObject terrain;
    private GameObject[] weightedObstacles, restrictedZones;

    private void Awake()
    {
        dataGenerator = new MazeDataGenerator();
        terrain = GameObject.FindWithTag("Terrain");
        
        
        float length = terrain.GetComponent<Renderer>().bounds.max.x;
        float width = terrain.GetComponent<Renderer>().bounds.max.z;

        restrictedZones = GameObject.FindGameObjectsWithTag("Restricted");
        weightedObstacles = new GameObject[wallChance * wall.Length
                                           + treasureChance * wallTresure.Length
                                           + sarcophhagusChance * sarcophagus.Length
                                           + barricadeChance * barricade.Length
                                           + pillarChance * pillar.Length];
        prepObstacles();

        if (generateMaze)
        {
            if (randomExits)
            {
                Debug.Log("?");
                GameObject[] exits = GameObject.FindGameObjectsWithTag("Exit");
                foreach (GameObject exit in exits)
                    //TODO : This hangs the game 
//                    exit.SetActive(false);
                PlaceExits(length, width);
                restrictedZones = GameObject.FindGameObjectsWithTag("Restricted"); //Update after putting down exits
            }

            GenerateNewMaze(length, width);
            
            
            PlaceLights();
        }

        GetComponent<NavMeshSurface>().BuildNavMesh();

        SpawnAis();
    }

    private void SpawnAis()
    {
        foreach (var pair in aiSpawns)
        {
//            Instantiate(pair.Value, pair.Key.position, Quaternion.identity);
        }
    }

    private void PlaceExits(float tLength, float tWidth)
    {
        for (int i = 0; i < numberOfExits; i++)
        {
            GameObject exit = Instantiate(this.exit[Random.Range(0, this.exit.Length)], terrain.transform.position, Quaternion.identity);
            Vector3 exitLocation;
                
            do
            {
                float x = Random.Range(exitWidthOffset + tWidth/numberOfExits*i, (tWidth/numberOfExits + tWidth/numberOfExits*i) - exitWidthOffset);
                float y = 1.25f;
                float z = Random.Range(exitLengthOffset, tWidth - exitLengthOffset);
                
                exitLocation = new Vector3(x, y, z);
                exit.transform.position = exitLocation;
                exit.transform.rotation = randomCardinalRotation();
            } while (IsRestricted(exitLocation) || exit.GetComponentInChildren<ExitVolume>().intersects());
        }
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
                            Instantiate(light[Random.Range(0, light.Length)],
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
            for(int j = 0; j < wallTresure.Length; j++)
                weightedObstacles[index++] = wallTresure[j];
        for (int i = 0; i < barricadeChance; i++)
            for(int j = 0; j < barricade.Length; j++)
                weightedObstacles[index++] = barricade[j];
        for (int i = 0; i < sarcophhagusChance; i++)
            for(int j = 0; j < sarcophagus.Length; j++)
                weightedObstacles[index++] = sarcophagus[j];
        for (int i = 0; i < pillarChance; i++)
            for(int j = 0; j < pillar.Length; j++)
                weightedObstacles[index++] = pillar[j];
        for (int i = 0; i < wallChance; i++)
            for (int j = 0; j < wall.Length; j++)
                weightedObstacles[index++] = wall[j];
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

//    private bool IsRestricted(Bounds bounds)
//    {
//        //TODO: Needs proper testing
//        foreach (GameObject zone in restrictedZones)
//        {
//            Bounds restrictedBounds = zone.GetComponent<Collider>().bounds;
//            if (bounds.Intersects(restrictedBounds))
//                return true;
//        }
//
//        return false;
//    }

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
                        PlaceObstacle(i, j, wall[Random.Range(0, wall.Length)]);
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

        try
        {
            //Check left
            if (data[i - 1, j] == 0)
                rotations.Add(270);
            //Check right
            if (data[i + 1, j] == 0)
                rotations.Add(90);
        }
        catch
        {
            rotations.Add(90);
        }

        try
        {
            //Check up
            if (data[i, j - 1] == 0)
                rotations.Add(0);
            //Check down
            if (data[i, j + 1] == 0)
                rotations.Add(180);
        }
        catch
        {
            rotations.Add(0);
        }

        int rotation = rotations.Count > 0 ? rotations[Random.Range(0, rotations.Count)] : 0;
        return Quaternion.Euler(new Vector3(0, rotation, 0));
    }

    private void PlaceObstacle(int i, int j, GameObject obstacle = null)
    {
        if (obstacle == null)
            obstacle = weightedObstacles[Random.Range(0, weightedObstacles.Length)];

        float length = terrain.transform.localScale.x;
        float width = terrain.transform.localScale.z;
        
        float obstacleHeight = obstacle.GetComponentInChildren<Renderer>().bounds.max.y;
        
        Vector3 obstacleLocation = new Vector3(i * cellSize - length / 2,
            obstacleHeight, j * cellSize - width / 2);
        Quaternion obstacleRotation = obstacle.CompareTag("Barricade") ? randomFreeformRotation() 
            : obstacle == wallTresure[Random.Range(0, wallTresure.Length)] 
              || alwaysTurnTowardsFree ? ForwardFreeRotation(i, j) : randomCardinalRotation();
        
        if(!IsRestricted(obstacleLocation))
            Instantiate(obstacle,
                obstacleLocation,
                obstacleRotation,
                transform);
    }


}
