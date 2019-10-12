using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class SummonControl_scr : MonoBehaviour
{
    public GameObject camera;
    
    public GameObject minionNPC;
    public float summonRadius = 10f;
    public float minionRunDistance = 10f;
    public float minionCollisionCheckRadius = .5f;
    public LayerMask bodiesMask;
    public LayerMask obstaclesMask;

    public List<GameObject> minions;
    public List<GameObject> minionsAway; //public for debugging purpose TODO: Change to private later?
    private GameObject minionTarget;
    private bool consumeSummonInput = false;

    // Start is called before the first frame update
    void Start()
    {
        minions = new List<GameObject>();
        minionsAway = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        summonMinionCheck();
        sendMinionCheck();
        recallMinionCheck();
    }

    private void summonMinionCheck()
    {
        //Display on screen help -- need reverse conditions? 

        Collider[] nearbyBodies = Physics.OverlapSphere(transform.position, summonRadius, bodiesMask);

        if (Input.GetMouseButtonDown(1))
        {
            //secondary mouse button
            if (nearbyBodies.Length > 0)
            {
                summonMinion(nearbyBodies[0].gameObject);
                consumeSummonInput = true;
            }
        }
    }

    private void sendMinionCheck()
    {
        if (Input.GetMouseButtonDown(0) && minions.Count > 0)
        {
            RaycastHit hit;
            GameObject minionToSend = minions[0];                         

            Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); //TODO: +1 hardcoded for now, change to player height/2 later? 
            bool obstacleHit = Physics.SphereCast(origin, minionCollisionCheckRadius, camera.transform.forward, out hit,
                minionRunDistance, obstaclesMask);
            
            Vector3 direction = new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z);

            Vector3 destination = hit.transform
            ? hit.point
            : transform.position + direction * minionRunDistance;

            minionToSend.GetComponent<SummonAIControl>()
                .SendToDestination(destination, obstacleHit, hit);
        }
    }

    private void recallMinionCheck()
    {
        if (Input.GetMouseButtonDown(1) && minionsAway.Count > 0)
        {
            if(!consumeSummonInput){
            GameObject minionToRecall = minionsAway[0];
            StartCoroutine(minionToRecall.GetComponent<SummonAIControl>().recall());
            }

            consumeSummonInput = false; //Don't recall minion if summoned
        }
    }

    private void summonMinion(GameObject body)
    {
        NavMeshHit hit;

        Vector3 summonPos = body.transform.position;
        NavMesh.SamplePosition(summonPos, out hit, 2.0f, NavMesh.AllAreas);
        

        //Summon minion
        GameObject newMinion = Instantiate(minionNPC, hit.position, transform.rotation);

        //Destroy body
        Destroy(body.transform.gameObject);
    }

    public void minionLeave(GameObject minion)
    {
//        Debug.Log(minion.name + " leaving");
        minions.Remove(minion);
        if(!minionsAway.Contains(minion))
            minionsAway.Add(minion);
    }

    public void minionRemove(GameObject minion)
    {
        minions.Remove(minion);
        minionsAway.Remove(minion);
    }

    public void minionReturn(GameObject minion)
    {
//        Debug.Log(minion.name + " returning");
        minionsAway.Remove(minion);
        if(!minions.Contains(minion))
            minions.Add(minion);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
        
        //Camera ray
        Gizmos.color = Color.blue;
        RaycastHit hit;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); //TODO: +1 hardcoded for now, change to player height/2 later? 
        if (Physics.SphereCast(origin, minionCollisionCheckRadius, camera.transform.forward, out hit,
            minionRunDistance, obstaclesMask))
            Gizmos.DrawWireSphere(hit.point, .2f);
        else
            Gizmos.DrawWireSphere(transform.position + camera.transform.forward * minionRunDistance, .2f);
        
//        Gizmos.color = Color.green;
//        Vector3 direction = new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z);
//        Gizmos.DrawRay(origin, direction * 10f);
        
    }
}