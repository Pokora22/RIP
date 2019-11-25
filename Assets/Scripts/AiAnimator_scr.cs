using UnityEngine;

public class AiAnimator_scr : MonoBehaviour
{
    private Animator m_Animator;

    [SerializeField] private float m_GroundCheckDistance = 0.1f;
    [SerializeField] private GameObject hitBox;
    private Vector3 m_GroundNormal;
    private bool m_IsGrounded;

    // Start is called before the first frame update
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        getHitbox(transform, "AttackHitbox");
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

    public float SetAttackAnim(float speed, bool attacking = true)
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
    }

    private void UpdateAnimator(Vector3 move){
			m_Animator.SetFloat("Forward", move.z, 0.1f, Time.deltaTime);
    }

    private void toggleHitBox()
    {
	    hitBox.SetActive(!hitBox.activeSelf);
    }

    private void getHitbox(Transform parent, string tag)
    {
	    for (int i = 0; i < parent.childCount; i++)
	    {
		    Transform child = parent.GetChild(i);
		    if (child.CompareTag(tag))
		    {
			    hitBox = child.gameObject;
		    }
		    if (child.childCount > 0)
		    {
			    getHitbox(child, tag);
		    }
	    }
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
