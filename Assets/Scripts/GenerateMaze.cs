using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    private MazeDataGenerator dataGenerator;
    [SerializeField] private GameObject wall, wallTresure, pillar, sarcophagus, barricade;
    [SerializeField] private float wallChance, treasureChance, sarcophhagusChance, barricadeChance, pillarChance;
    int[,] data;
    [SerializeField] private GameObject parentCollection;
    [SerializeField] private float cellSize = 3f;
    [SerializeField] private float cellHeight = 3f;
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
                    if(i == 0 || j == 0 || i == rMax || j == cMax)
                        Instantiate(wall,
                                new Vector3(i * cellSize - length/2, terrain.transform.localScale.y/2, j * cellSize - width/2),
                                randomCardinalRotation(),
                                parentCollection.transform);
                    
                    Instantiate(RandomObstacle(),
                        new Vector3(i * cellSize - length/2, terrain.transform.localScale.y/2, j * cellSize - width/2),
                        randomCardinalRotation(),
                        parentCollection.transform);
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

    private GameObject RandomObstacle()
    {
        GameObject prefab;
        float rng = Random.Range(0, treasureChance + barricadeChance + sarcophhagusChance + pillarChance + wallChance);
        if (rng < treasureChance)
            prefab = wallTresure;
        else if (rng < barricadeChance)
            prefab = barricade;
        else if (rng < sarcophhagusChance)
            prefab = sarcophagus;
        else if (rng < pillarChance)
            prefab = pillar;
        else
            prefab = wall;

        return prefab;
    }


}
