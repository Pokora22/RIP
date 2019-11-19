using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    Taken from https://www.raywenderlich.com/82-procedural-generation-of-mazes-with-unity

    + Read http://weblog.jamisbuck.org/2011/2/1/maze-generation-binary-tree-algorithm as well
 */
public class MazeDataGenerator
{
    public float placementThreshold;

    public MazeDataGenerator(){
        placementThreshold = .1f;
    }

    public int[,] FromDimensions(int sizeRows, int sizeCols){
        int[,] maze = new int[sizeRows, sizeCols];

        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                //1
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {
                    maze[i, j] = 1;
                }

                //2
                else if (i % 2 == 0 && j % 2 == 0)
                {
                    if (Random.value > placementThreshold)
                    {
                        //3
                        maze[i, j] = 1;

                        int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                        int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                        maze[i+a, j+b] = 1;
                    }
                }
            }
        }

        return maze;
    }
}
