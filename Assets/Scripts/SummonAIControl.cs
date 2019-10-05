using System;
using System.Collections;
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

        public float playerFollowowDistance = 2f;
        public float stoppingDistance = 0f;
        public float enemyFollowDistance = 1f;
        public float enemyDetectionRange = 10f;
        public float recallDelay = 3f;
        
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK}

        private MINION_STATE currentState;

        private bool recalled = false;
        private SummonControl_scr summoner;
        private GameObject target;
        
        public bool debug = true;

        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                currentState = value;

                StopStateCoroutines();
                switch (currentState)
                {
                    case MINION_STATE.FOLLOW:
                        StartCoroutine(minionFollow());
                        break;
                    case MINION_STATE.ADVANCE:
                        StartCoroutine(minionAdvance());
                        break;
                    case MINION_STATE.CHASE:
                        StartCoroutine(minionChase());
                        break;
                    case MINION_STATE.ATTACK:
                        StartCoroutine(minionAttack());
                        break;
                    default:
                        StartCoroutine(minionFollow());
                        break;
                }
            }
        }

        private void StopStateCoroutines()
        {
            StopCoroutine(minionChase());
            StopCoroutine(minionAttack());
            StopCoroutine(minionFollow());
            StopCoroutine(minionAdvance());
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

            gameObject.name = "Minion " + (player.GetComponent<SummonControl_scr>().minions.Count  + player.GetComponent<SummonControl_scr>().minionsAway.Count);

            StartCoroutine(recall());

//            StartCoroutine(stateDebug());
        }

        private IEnumerator minionFollow()
        {
            if(!recalled)
                StartCoroutine(recall());

            target = null;
            summoner.minionReturn(this.gameObject);
            agent.stoppingDistance = playerFollowowDistance;
            
            while (currentState == MINION_STATE.FOLLOW)
            {
                agent.SetDestination(player.transform.position); 

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
                    StartCoroutine(recall());
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
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    CurrentState = MINION_STATE.ATTACK;
                }

                yield return null;
            }
        }

        private IEnumerator minionAttack()
        {
            while (currentState == MINION_STATE.ATTACK)
            {
                //TODO: Put attack coroutine here
                
                if (agent.remainingDistance > agent.stoppingDistance * 1.2f) //Extra space to keep attacking without switching between states constantly
                {
                    CurrentState = MINION_STATE.CHASE;
                }

                yield return null;
            }
        }


        private void Update()
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, enemies);
            if (nearbyEnemies.Length > 0 && !recalled)
            {
                if(target == null)
                    target = nearbyEnemies[Random.Range(0, nearbyEnemies.Length - 1)].gameObject; //Acquire new target if old is gone
                
                agent.destination = target.transform.position;
                
                if (CurrentState == MINION_STATE.FOLLOW || CurrentState == MINION_STATE.ADVANCE) //If following player, start combat
                    CurrentState = MINION_STATE.CHASE;
            }
            
            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("Structures"))
                CurrentState = MINION_STATE.FOLLOW;
        }

        public void SendToDestination(Vector3 destination)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 2.0f, NavMesh.AllAreas); 
            
            agent.SetDestination(hit.position);
            CurrentState = MINION_STATE.ADVANCE;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(agent.destination, 1f);
        }

        public IEnumerator recall()
        {
            if (recalled)
                yield return null;
            recalled = true;
//            Debug.Log(gameObject.name +" : Coroutine start recall: " + recalled);
            CurrentState = MINION_STATE.FOLLOW;
            yield return new WaitForSeconds(recallDelay);
            recalled = false;
//            Debug.Log(gameObject.name + "Coroutine end recall: " + recalled);
        }

        private IEnumerator stateDebug()
        {
            while (debug)
            {
                Debug.Log(gameObject.name + ": " + CurrentState);
                yield return  new WaitForSeconds(1);
            }
        }
    }
}
