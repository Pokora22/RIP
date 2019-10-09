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
        private Attributes_scr minionAttributes;
        
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK}

        [SerializeField] private MINION_STATE currentState;

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
            StopCoroutine(dealDamage());
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
        }

        private IEnumerator minionFollow()
        {
            Debug.Log(gameObject.name + " now following");

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
                    if (target.CompareTag("Destructible"))
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
            Debug.Log(gameObject.name + " starting attack..");
            StartCoroutine(dealDamage());
            while (currentState == MINION_STATE.ATTACK)
            {                
                if(!target)
                {
                    Debug.Log(gameObject.name + " destroyed target, now returning to follow...");
                    target = player;
                    CurrentState = MINION_STATE.FOLLOW;
                    yield return null;
                }

                if (target != player && agent.remainingDistance > agent.stoppingDistance * 1.2f) //Extra space to keep attacking without switching between states constantly
                {                    
                    CurrentState = MINION_STATE.CHASE;
                }

                yield return null;
            }
        }


        private void Update()
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, enemies);            

            if (nearbyEnemies.Length > 0 && !recalled) //Target enemies first
            {
                if(target == player)
                    target = nearbyEnemies[Random.Range(0, nearbyEnemies.Length - 1)].gameObject; //Acquire new target if old is gone

                agent.SetDestination(target.transform.position);

                if (!recalled && (CurrentState == MINION_STATE.FOLLOW || CurrentState == MINION_STATE.ADVANCE)) //If following player, start combat
                {                    
                    CurrentState = MINION_STATE.CHASE;
                }
            }            

            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
        }

        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
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

        private IEnumerator dealDamage(){ //TODO: Link with animation instead later
            Attributes_scr enemyAttr = target.GetComponent<Attributes_scr>();
            float dmg = minionAttributes.attackDamage;
            float aspd = minionAttributes.attackSpeed;

            while(target && (target.CompareTag("Destructible") || target.CompareTag("Enemy"))) //Infinite for now
            {
                if(!gameObject){
                    StopAllCoroutines();
                    yield return null;
                }

                Debug.Log(gameObject.name + " attacks " + enemyAttr.gameObject.name);
                enemyAttr.damage(dmg, minionAttributes);
                yield return new WaitForSeconds(1f/aspd);
            }
        }
    }
}
