using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    private IEnumerator tensec, stopper;
    private bool stopperRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        tensec = TenSecRoutine();
        stopper = StopperRoutine();

        
//        StartCoroutine(stopper);
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopperRunning)
        {
            StartCoroutine(stopper);
            Debug.Log("Starting stopper");
        }

    }

    IEnumerator TenSecRoutine()
    {
        while (true)
        {
            Debug.Log("10 sec started");
            yield return new WaitForSeconds(3);
            Debug.Log("Stopping 10 sec from inside");
            StopCoroutine(tensec);
            yield return new WaitForSeconds(7);
            Debug.Log("10 sec finished");
        }
    }

    IEnumerator StopperRoutine()
    {
        stopperRunning = true;
        Debug.Log("Stopper started, waiting for 3 sec");
        yield return new WaitForSeconds(3);
        Debug.Log("3 sec passed, stopping TenSecRoutine");
        StopCoroutine(tensec);
        stopperRunning = false;
    }
}
