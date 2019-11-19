using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GenerateMaze generator;

    // Start is called before the first frame update
    void Start()
    {
        generator = GetComponent<GenerateMaze>();
        generator.GenerateNewMaze(13,15);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
