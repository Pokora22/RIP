using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class SummonAnimator_scr : MonoBehaviour
{
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;

    [SerializeField] private float m_GroundCheckDistance = 0.1f;
    [SerializeField] private GameObject hitBox;
    private Vector3 m_GroundNormal;
    private bool m_IsGrounded;

    // Start is called before the first frame update
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        hitBox = findChildByTag(transform, "AttackHitbox");
        hitBox.SetActive(false);
    }

    public void Move(Vector3 move){
            // convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f)
				move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, m_GroundNormal);

			// send input and other state parameters to the animator
			UpdateAnimator(move);
    }

    public bool CompareCurrentState(string name)
    {
	    return m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public float SetAttackAnim(bool attacking, float speed)
    {
	    m_Animator.SetBool("Attacking", attacking);
	    m_Animator.SetFloat("AttackSpeed", speed);
	    
	    float animationLength = m_Animator.GetCurrentAnimatorStateInfo(0).length;

	    return animationLength;
    }
    
    public void setDeadAnim()
    {
	    toggleHitBox();
	    m_Animator.SetTrigger("Dead");
	    gameObject.GetComponent<SummonAIControl>().CurrentState = SummonAIControl.MINION_STATE.NONE;
    }

    private void UpdateAnimator(Vector3 move){
			m_Animator.SetFloat("Forward", move.z, 0.1f, Time.deltaTime);
    }

    private void toggleHitBox()
    {
	    hitBox.SetActive(!hitBox.activeSelf);
    }

    private GameObject findChildByTag(Transform parent, string tag)
    {
	    Debug.Log(parent.childCount);
	    for (int i = 0; i < parent.childCount; i++)
	    {
		    Debug.Log("From children of " + parent.name);
		    Transform child = parent.GetChild(i);
		    Debug.Log("Checking " + child.name);
		    if (child.CompareTag(tag))
			    return child.gameObject;
		    
		    if (child.childCount > 0)
		    {
			    findChildByTag(child, tag);
		    }
	    }
	    
	    return null;
    }
    
    private void CheckGroundStatus()
    {
	    RaycastHit hitInfo;
#if UNITY_EDITOR
	    // helper to visualise the ground check ray in the scene view
	    Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
	    // 0.1f is a small offset to start the ray from inside the character
	    // it is also good to note that the transform position in the sample assets is at the base of the character
	    if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
	    {
		    m_GroundNormal = hitInfo.normal;
		    m_IsGrounded = true;
		    m_Animator.applyRootMotion = true;
	    }
	    else
	    {
		    m_IsGrounded = false;
		    m_GroundNormal = Vector3.up;
		    m_Animator.applyRootMotion = false;
	    }
    }
}
