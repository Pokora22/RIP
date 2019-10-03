using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling

        public GameObject player;

        public float playerFollowowDistance = 2f;
        public float stoppingDistance = 0f;
        
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK}

        private MINION_STATE currentState;

        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                currentState = value;

                StopAllCoroutines();
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
        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;

            player = GameObject.FindWithTag("Player");
            
            CurrentState = MINION_STATE.FOLLOW;
        }

        private IEnumerator minionFollow()
        {
            player.GetComponent<SummonControl_scr>().minionReturn(this.gameObject); //TODO: Why is it not visible (when in std assets folder) ?
            agent.stoppingDistance = playerFollowowDistance;
            
            while (currentState == MINION_STATE.FOLLOW)
            {
            if(player != null)
                agent.SetDestination(player.transform.position);

//            if (agent.remainingDistance > agent.stoppingDistance)
//                character.Move(agent.desiredVelocity, false, false);
//            else
//                character.Move(Vector3.zero, false, false);         

                yield return null;
            }
        }

        private IEnumerator minionAdvance()
        {
            agent.stoppingDistance = stoppingDistance;
            
            while (currentState == MINION_STATE.ADVANCE)
            {
                while (agent.pathPending)
                    yield return null;
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    CurrentState = MINION_STATE.FOLLOW;
                }

                yield return null;
            }
        }

        private IEnumerator minionChase()
        {
            yield return null;
        }

        private IEnumerator minionAttack()
        {
            yield return null;
        }


        private void Update()
        {
            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Barricade"))
            {
                float damageReceive = other.gameObject.GetComponent<Attributes_scr>().attackDamage;
                
                Debug.Log("Received dmg: " + damageReceive);
                GetComponent<Attributes_scr>().damage(damageReceive);
            }

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
    }
}
