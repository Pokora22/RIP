using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter_Custom : MonoBehaviour
	{
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		private CharacterController m_CharacterController;
		bool m_IsGrounded;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;

		CapsuleCollider m_Capsule;
		bool m_Crouching;
		private Vector3 m_CamForward;
		private Transform m_Cam;


		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CharacterController = GetComponent<CharacterController>();
			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_Cam = Camera.main.transform;
		}


		public void Move(Vector3 move)
		{
			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
//			if (move.magnitude > 1f) move.Normalize();
//			move = transform.InverseTransformDirection(move);
//			CheckGroundStatus();
//			move = Vector3.ProjectOnPlane(move, m_GroundNormal);
//			m_TurnAmount = Mathf.Atan2(move.x, move.z);
//			m_ForwardAmount = move.z;
			
//			ApplyExtraTurnRotation();

			if (move != Vector3.zero)
				transform.forward = move;
			m_Rigidbody.MovePosition(m_Rigidbody.position + move * m_MoveSpeedMultiplier * Time.fixedDeltaTime);
		}


//		private void Update()
//		{
//			float h = CrossPlatformInputManager.GetAxis("Horizontal");
//			float v = CrossPlatformInputManager.GetAxis("Vertical");
//			m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
//			Vector3 move = v*m_CamForward + h*m_Cam.right;
//			m_CharacterController.Move(move * m_MoveSpeedMultiplier * Time.fixedDeltaTime);
//			if (move != Vector3.zero)
//				transform.forward = move;
//		}


		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}
	}
}
