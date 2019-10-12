using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class SummonAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling

        public GameObject player;
        public LayerMask enemies;
        public int debugDmgRoutineCntr = 0;
        public float playerFollowowDistance = 2f;
        public float stoppingDistance = 0f;
        public float enemyFollowDistance = 1f;
        public float enemyDetectionRange = 10f;        
        public float playerLeashRange = 15f;
        public float recallDelay = 3f;
        public bool debug = true;
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK}

        [SerializeField] private MINION_STATE currentState;
        private Coroutine currentCoroutine;

        private NpcAudio_scr audioPlayer;
        private bool recalled = false;
        private bool dmgRoutineRunning = false;
        private SummonControl_scr summoner;
        private GameObject target;
        private Attributes_scr minionAttributes;

        private Transform debugTarget;


        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                if(debug)
                    Debug.Log(gameObject.name + " coming from: " + currentState);
                
                currentState = value;

                if(currentCoroutine != null)
                    StopCoroutine(currentCoroutine);
                
                switch (currentState)
                {
                    case MINION_STATE.FOLLOW:
                        currentCoroutine = StartCoroutine(minionFollow());
                        break;
                    case MINION_STATE.ADVANCE:
                        currentCoroutine = StartCoroutine(minionAdvance());
                        break;
                    case MINION_STATE.CHASE:
                        currentCoroutine = StartCoroutine(minionChase());
                        break;
                    case MINION_STATE.ATTACK:
                        currentCoroutine = StartCoroutine(minionAttack());
                        break;
                }
                
                if(debug)
                    Debug.Log(gameObject.name + " going into: " + currentState);
            }
        }

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;

            player = GameObject.FindWithTag("Player");
            summoner = player.GetComponent<SummonControl_scr>();
            minionAttributes = gameObject.GetComponent<Attributes_scr>();

            gameObject.name = "Minion " + (player.GetComponent<SummonControl_scr>().minions.Count  + player.GetComponent<SummonControl_scr>().minionsAway.Count);

            // StartCoroutine(recall());
            CurrentState = MINION_STATE.FOLLOW;

//            StartCoroutine(stateDebug());

            audioPlayer = GetComponent<NpcAudio_scr>();
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.RAISE);
        }

        private IEnumerator minionFollow()
        {
//            Debug.Log(gameObject.name + " now following");

            target = player;
            summoner.minionReturn(this.gameObject);
            agent.stoppingDistance = playerFollowowDistance;
            
            while (currentState == MINION_STATE.FOLLOW)
            {
                agent.SetDestination(target.transform.position);
                yield return null;
            }
        }

        private IEnumerator minionAdvance()
        {
            summoner.minionLeave(gameObject);
            
            while (currentState == MINION_STATE.ADVANCE)
            {
                while (agent.pathPending)
                    yield return null;
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    //If there's a target?
                    if (target.CompareTag("Destructible") || target.CompareTag("Barricade"))
                        CurrentState = MINION_STATE.ATTACK;                     
                    else
                        CurrentState = MINION_STATE.FOLLOW;
                }

                yield return null;
            }
        }

        private IEnumerator minionChase()
        {
            agent.stoppingDistance = enemyFollowDistance;
            
            summoner.minionLeave(gameObject);
            
            while (currentState == MINION_STATE.CHASE)
            {
                //                while (agent.pathPending)
                //                    yield return null;
                //Return to player if enemies run out too far from player
                if (Vector3.Distance(transform.position, player.transform.position) > playerLeashRange)
                {
                    CurrentState = MINION_STATE.FOLLOW;
                    yield return null;                 
                }
                
                if(!target)
                {                    
                    CurrentState = MINION_STATE.FOLLOW;
                    yield return null;                    
                }

                agent.destination = target.transform.position;
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {                    
                    CurrentState = MINION_STATE.ATTACK;
                }

                yield return null;
            }
        }

        private IEnumerator minionAttack()
        {
            Attributes_scr enemyAttr = target.GetComponent<Attributes_scr>();
            float dmg = minionAttributes.attackDamage;
            float aspd = minionAttributes.attackSpeed;
            
            if (target.CompareTag("Enemy"))
                agent.stoppingDistance = enemyFollowDistance;
            else if (target.CompareTag("Destructible") || target.CompareTag("Barricade"))
                agent.stoppingDistance = 1.6f; //Destructible attack distance
            
            while (currentState == MINION_STATE.ATTACK)
            {
                yield return new WaitForSeconds(1f/aspd);
                
                if(!target)
                {
                    target = player;
                    CurrentState = MINION_STATE.FOLLOW;
                    yield break;
                }

                if (target != player && agent.remainingDistance > agent.stoppingDistance * 1.2f) //Extra space to keep attacking without switching between states constantly
                {                    
                    CurrentState = MINION_STATE.CHASE;
                    yield break;
                }

                if (target && target != player)
                {
                    audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.ATTACK);
                    enemyAttr.damage(dmg, minionAttributes);
                }

                yield return null;
            }
        }


        private void Update()
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, enemies);

            if (nearbyEnemies.Length > 0 && !recalled) //Target enemies first
                target = selectTarget(nearbyEnemies);

            //If following player, start combat
            if (target != player && !recalled &&
                (CurrentState == MINION_STATE.FOLLOW || CurrentState == MINION_STATE.ADVANCE))
            {
                CurrentState = MINION_STATE.CHASE;
            }

            agent.SetDestination(target.transform.position);

            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
        }

        private GameObject selectTarget(Collider[] nearbyEnemies)
        {
            GameObject newTarget;
            RaycastHit hit;
            List<Collider> enemyList = new List<Collider>(nearbyEnemies);
            if (target == player)
            {
                do //TODO: Make a list instead and choose closest target and switch if not currently attacking instead
                {
                    int index = Random.Range(0, enemyList.Count - 1);
                    newTarget = enemyList[index].gameObject; //Acquire new target if old is gone
                    enemyList.RemoveAt(index);
                    
                    Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
                    Vector3 destination = new Vector3(newTarget.transform.position.x, 1.5f, newTarget.transform.position.z) -
                                          origin;
                    
                    Physics.Raycast(origin, destination, out hit);
                    
                    if (hit.transform.CompareTag("Enemy"))
                    {
                        return newTarget; 
                    }
                } while (enemyList.Count > 0);

                return player;
            }
            return target;
        }

        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
            recalled = false;
            
            NavMeshHit hit;

            if (obstacleHit)
                target = rayHit.transform.gameObject; //This is still pointing to object, not hit point.

            NavMesh.SamplePosition(destination, out hit, 2.0f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);

            CurrentState = MINION_STATE.ADVANCE;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(agent.destination, 1f);
            
            Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
            Vector3 destination = new Vector3(target.transform.position.x, 1.5f, target.transform.position.z) - origin;
            
            Gizmos.DrawRay(origin, destination);
        }

        public IEnumerator recall()
        {
            if (recalled)
                yield break;
            recalled = true;
//            Debug.Log(gameObject.name +" : Coroutine start recall: " + recalled);
            CurrentState = MINION_STATE.FOLLOW;
            yield return new WaitForSeconds(recallDelay);
            recalled = false;
//            Debug.Log(gameObject.name + "Coroutine end recall: " + recalled);
        }
    }
}
