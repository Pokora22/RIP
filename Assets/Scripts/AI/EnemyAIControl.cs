using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using AI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace AI
{    
    public class EnemyAIControl : AiControl
    {
        [SerializeField] private float targetScanDelay = .25f;

        protected void Start()
        {   
            base.Start();
            
            StartCoroutine(findTarget(targetScanDelay));
        }


        protected IEnumerator findTarget(float targetScanDelay)
        {
            while (true)
            {
                while (target != gameObject)
                {
                    yield return new WaitForSeconds(targetScanDelay);
                }
                
                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, fovDistance, enemiesMask);
                List<Collider> enemyList = nearbyEnemies.OrderBy(
                    x => (this.transform.position - x.transform.position).sqrMagnitude
                ).ToList();

                while (enemyList.Count > 0)
                {
                    GameObject newTarget = enemyList[0].gameObject;
                    enemyList.RemoveAt(0);

                    if (CanHearTarget(newTarget) || CanSeeTarget(newTarget))
                    {
                        target = newTarget;
                        TargetAttr = newTarget.GetComponent<Attributes_scr>();
                        break;
                    }
                }

                //TODO: Decide on delay
                yield return null;
//                yield return new WaitForSeconds(targetScanDelay);
            }
        }
    }
}
