using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitVolume : MonoBehaviour
{
    private bool intersecting;

    private void OnTriggerEnter(Collider other)
    {
        intersecting = true;
    }

    private void OnTriggerExit(Collider other)
    {
        intersecting = false;
    }

    public bool intersects()
    {
        return intersecting;
    }
}
