using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController_scr : MonoBehaviour
{
    public GameObject minionNPC;
    public float summonRadius = 10f;
    public float minionRunDistance = 10f;
    public float minionCollisionCheckRadius = .5f;
    public LayerMask bodiesMask;
    public LayerMask obstaclesMask;
    public List<SummonAIControl> minions, minionsAway;
    public UnityEvent newMinionEvent;
    
    private bool consumeSummonInput = false;
    private pAttributes_scr playerAttributes;
    private Attributes_scr characterAttributes;
    private Vector3 m_CamForward, move;
    private Transform m_Cam;
    private Rigidbody m_Rigidbody;
    private float inputTimeDelay = .2f;
    private float inputTimeStamp;

    // Start is called before the first frame update
    void Start()
    {
        minions = new List<SummonAIControl>();
        minionsAway = new List<SummonAIControl>();
        playerAttributes = GameObject.FindWithTag("GameManager").GetComponent<pAttributes_scr>();
        characterAttributes = GetComponent<Attributes_scr>();
        m_Cam = Camera.main.transform;
        m_Rigidbody = GetComponent<Rigidbody>();

        inputTimeStamp = Time.time + inputTimeDelay;
    }

    // Update is called once per frame
    void Update()
    {
        HandleControlInput();
        summonMinionCheck();
        sendMinionCheck();
    }

    private void summonMinionCheck()
    {
        //Display on screen help -- need reverse conditions?
        if(minions.Count + minionsAway.Count >= playerAttributes.summonsLimit)
            return;

        if (Input.GetButton("Fire2") && inputTimeStamp < Time.time)
        {
            inputTimeStamp = Time.time + inputTimeDelay;
            Collider[] nearbyBodies = Physics.OverlapSphere(transform.position, summonRadius, bodiesMask);
            
            //secondary mouse button
            if (nearbyBodies.Length > 0 && minions.Count + minionsAway.Count < playerAttributes.summonsLimit)
            {
                summonMinion(nearbyBodies[0].gameObject);
                consumeSummonInput = true;
            }
            else if (minionsAway.Count > 0)
            {
                SummonAIControl minionToRecall = minionsAway[0];
                StartCoroutine(minionToRecall.recall());
            }
        }
    }

    private void sendMinionCheck()
    {
        if (Input.GetButton("Fire1") && minions.Count > 0  && inputTimeStamp < Time.time)
        {
            inputTimeStamp = Time.time + inputTimeDelay;
            RaycastHit hit;
            SummonAIControl minionToSend = minions[0];                         

            //Fix: start the raycast behind the player character to make sure we don't bypass first obstacle
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z) - transform.forward; 
            Vector3 direction = new Vector3(m_CamForward.x, 0f, m_CamForward.z);
            bool obstacleHit = Physics.SphereCast(origin, minionCollisionCheckRadius, direction, out hit,
                minionRunDistance, obstaclesMask);
            
            

            Vector3 destination = hit.transform
            ? hit.point
            : transform.position + direction * minionRunDistance;

            minionToSend.SendToDestination(destination, obstacleHit, hit);
        }
    }

    private void summonMinion(GameObject body)
    {
        float distance = Vector3.Distance(transform.position, body.transform.position);

        if (!Physics.Raycast(transform.position, body.transform.position - transform.position, distance, obstaclesMask))
        {
            NavMeshHit hit;
            Vector3 summonPos = body.transform.position;
            NavMesh.SamplePosition(summonPos, out hit, 10.0f, NavMesh.AllAreas);

            //Summon minion
            GameObject newMinion = Instantiate(minionNPC, hit.position, transform.rotation);

            //Destroy body
            Destroy(body.transform.gameObject);
            
            newMinionEvent.Invoke();
        }
    }

    public void minionLeave(SummonAIControl minion)
    {
//        Debug.Log(minion.name + " leaving");
        minions.Remove(minion);
        if(!minionsAway.Contains(minion))
            minionsAway.Add(minion);
    }

    public void minionRemove(SummonAIControl minion)
    {
        minions.Remove(minion);
        minionsAway.Remove(minion);
        playerAttributes.updateHud();
    }

    public void minionReturn(SummonAIControl minion)
    {
//        Debug.Log(minion.name + " returning");
        minionsAway.Remove(minion);
        if(!minions.Contains(minion))
            minions.Add(minion);
        playerAttributes.updateHud();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit"))
            SceneManager.LoadScene(2);
    }
    
    private void HandleControlInput()
    {
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
        move = v*m_CamForward + h*m_Cam.right;
    }

    private void FixedUpdate()
    {
        if (move != Vector3.zero)
            transform.forward = move;
        m_Rigidbody.MovePosition(m_Rigidbody.position + move * characterAttributes.moveSpeedMultiplier * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
        
        //Camera ray
        Gizmos.color = Color.blue;
        RaycastHit hit;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); //TODO: +1 hardcoded for now, change to player height/2 later? 
        if (Physics.SphereCast(origin, minionCollisionCheckRadius, m_CamForward, out hit,
            minionRunDistance, obstaclesMask))
            Gizmos.DrawWireSphere(hit.point, .2f);
        else
            Gizmos.DrawWireSphere(transform.position + m_CamForward * minionRunDistance, .2f);
        
//        Gizmos.color = Color.green;
//        Vector3 direction = new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z);
//        Gizmos.DrawRay(origin, direction * 10f);
        
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Barricade") && other.GetContact(0).normal == other.transform.right
        ) //transform.right is the spiky side
        {
            playerAttributes.damage();
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.AddForce((other.transform.right + (Vector3.up/20)) * 50, ForceMode.Impulse); //TODO: Push player back? Adjust force
        }
    }
}