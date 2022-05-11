using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using Cinemachine;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Crouch speed of the character in m/s")]
		public float CrouchSpeed = 2.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _fallTimeoutDelta;

		private PlayerInput _playerInput;
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.001f;

		[Header("Camera Bob")]
		public bool CameraBob = true;
		private float timer;
		private float defaultYPos = 0;
		[SerializeField] private GameObject PlayerCameraRoot;
		[SerializeField] private float walkBobSpeed;
		[SerializeField] private float walkBobAmaunt;
		[SerializeField] private float sprintBobSpeed;
		[SerializeField] private float sprintBobAmaunt;
		[SerializeField] private float crouchBobSpeed;
		[SerializeField] private float crouchBobAmaunt;

		private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

		private Vector3 crouchPos;
		private Vector3 normalPos;

		[Header("Interact")]
		public float range;
		public float angle;
		public LayerMask mask;

		public bool hide;

		[Header("SeeEnemyEvent")]
		[SerializeField]
		private float rangeEnemyEvent;
		[SerializeField]
		private float angleEnemyEvent;
		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			defaultYPos = PlayerCameraRoot.transform.localPosition.y;
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();

			// reset our timeouts on start
			_fallTimeoutDelta = FallTimeout;
			normalPos = PlayerCameraRoot.transform.localPosition;
			crouchPos = PlayerCameraRoot.transform.localPosition + new Vector3(0, -0.75f, 0);
		}

		private void Update()
		{
			GravityPlayer();
			GroundedCheck();
			Move();
			if (CameraBob)
				HandleHeadBob();
			if(_input.interact)
			{
				var interactable = FindObjectsOfType<Interactable>();
				foreach(Interactable interact in interactable)
					if (IsInSight(interact.transform, range, 0))
						interact.Interact();
				_input.interact = false;
			}
			var enemysE = FindObjectsOfType<EnemyEvent>();
			foreach (EnemyEvent enemy in enemysE)
				if (IsInSight(enemy.transform, rangeEnemyEvent, angleEnemyEvent))
					enemy.Event();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : _input.crouch ? CrouchSpeed : MoveSpeed;

			if (_input.crouch && !_input.sprint)
				PlayerCameraRoot.transform.localPosition = crouchPos;
			else PlayerCameraRoot.transform.localPosition = normalPos;
			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void GravityPlayer()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
			}
			else
			{
				// reset the jump timeout timer

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		void HandleHeadBob()
        {
			if (!Grounded)
				return;
			if (Mathf.Abs(_input.move.x) > 0.1f || Mathf.Abs(_input.move.y) > 0.1f)
            {
				var playerCameraPos = PlayerCameraRoot.transform.localPosition;
				var DefaulY = (_input.sprint ? defaultYPos : _input.crouch ? crouchPos.y : defaultYPos);
				timer += Time.deltaTime * (_input.sprint ? sprintBobSpeed : _input.crouch ? crouchBobSpeed : walkBobSpeed);
				PlayerCameraRoot.transform.localPosition = new Vector3(
					playerCameraPos.x, DefaulY + Mathf.Sin(timer) * 
					(_input.sprint ? sprintBobAmaunt : _input.crouch ? crouchBobAmaunt : walkBobAmaunt)
					, playerCameraPos.z);
			}
        }

        private void OnTriggerEnter(Collider other)
        {
			if (other.gameObject.layer == 8)
				hide = true;
        }

        private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer == 8)
				hide = false;
		}

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		public bool IsInSight(Transform target, float range, float angle)
		{
			var pos = FindObjectOfType<Camera>().transform;
			Vector3 diff = (target.position - pos.position);
			//A--->B
			//B-A
			float distance = diff.magnitude;
			if (distance > range)
				return false;
			if (angle > 0)
				if (Vector3.Angle(pos.forward, diff) > angle / 2) return false;
			if (Physics.Raycast(pos.position, pos.up, distance, mask))
				return false;
			return true;
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
			Gizmos.color = Color.blue;
			var pos = FindObjectOfType<Camera>().transform;
			Gizmos.DrawRay(pos.position, pos.forward * range);
			Gizmos.DrawWireSphere(pos.position, range);
			Gizmos.DrawRay(pos.position, Quaternion.Euler(0, angle / 2, 0) * pos.forward * range);
			Gizmos.DrawRay(pos.position, Quaternion.Euler(0, -angle / 2, 0) * pos.forward * range);
		}
	}
}