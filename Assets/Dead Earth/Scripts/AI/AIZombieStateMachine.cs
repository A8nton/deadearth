using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }

public class BodyPartSnapshot {
	public Transform transform;
	public Vector3 position;
	public Quaternion rotation;
}

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine {
	[SerializeField] [Range(10.0f, 360.0f)] float _fieldOfView = 50.0f;
	[SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
	[SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
	[SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
	[SerializeField] [Range(0, 100)] int _health = 100;
	[SerializeField] [Range(0, 100)] int _lowerBodyDamage = 0;
	[SerializeField] [Range(0, 100)] int _upperBodyDamage = 0;
	[SerializeField] [Range(0, 100)] int _upperBodyThreshold = 30;
	[SerializeField] [Range(0, 100)] int _limpThreshold = 30;
	[SerializeField] [Range(0, 100)] int _crawlThreshold = 90;
	[SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
	[SerializeField] [Range(0.0f, 1.0f)] float _satisfaction = 1.0f;

	[SerializeField] float _replenishRate = 0.5f;
	[SerializeField] float _depletionRate = 0.1f;
	[SerializeField] float _reanimationBlendTime = 0.5f;
	[SerializeField] float _reanimationWaitTime = 3.0f;
	[SerializeField] LayerMask _geometryLayers = 0;

	private int _seeking = 0;
	private bool _feeding = false;
	private bool _crawling = false;
	private int _attackType = 0;
	private float _speed = 0.0f;

	private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
	private List<BodyPartSnapshot> _bodyPartSnapshots = new List<BodyPartSnapshot>();
	private float _ragdollEndTime = float.MinValue;
	private Vector3 _ragdollHipPosition;
	private Vector3 _ragdollFeetPosition;
	private Vector3 _ragdollHeadPosition;
	private IEnumerator _reanimationCoroutine;
	private float _mecanimTransitionTime = 0.1f;

	private int _speedHash = Animator.StringToHash("Speed");
	private int _seekingHash = Animator.StringToHash("Seeking");
	private int _feedingHash = Animator.StringToHash("Feeding");
	private int _attackHash = Animator.StringToHash("Attack");
	private int _crawlingHash = Animator.StringToHash("Crawling");
	private int _hitTriggerHash = Animator.StringToHash("Hit");
	private int _hitTypeHash = Animator.StringToHash("HitType");
	private int _lowerBodyHash = Animator.StringToHash("Lower Body Damage");
	private int _upperBodyHash = Animator.StringToHash("Upper Body Damage");
	private int _reanimateFromBackHash = Animator.StringToHash("Reanimate From Back");
	private int _reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
	private int _stateHash = Animator.StringToHash("State");

	public float replenishRate { get => _replenishRate; }
	public float fieldOfView { get { return _fieldOfView; } }
	public float hearing { get { return _hearing; } }
	public float sight { get { return _sight; } }
	public bool crawling { get { return _crawling; } }
	public float intelligence { get { return _intelligence; } }
	public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
	public float aggression { get { return _aggression; } set { _aggression = value; } }
	public int health { get { return _health; } set { _health = value; } }
	public int attackType { get { return _attackType; } set { _attackType = value; } }
	public bool feeding { get { return _feeding; } set { _feeding = value; } }
	public int seeking { get { return _seeking; } set { _seeking = value; } }
	public float speed {
		get { return _speed; }
		set { _speed = value; }
	}

	public bool isCrawling {
		get { return (_lowerBodyDamage >= _crawlThreshold); }
	}

	protected override void Start() {
		base.Start();

		if (_rootBone != null) {
			Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
			foreach (Transform trans in transforms) {
				BodyPartSnapshot snapShot = new BodyPartSnapshot();
				snapShot.transform = trans;
				_bodyPartSnapshots.Add(snapShot);
			}
		}

		UpdateAnimatorDamage();
	}

	protected override void Update() {
		base.Update();

		if (_animator != null) {
			_animator.SetFloat(_speedHash, _speed);
			_animator.SetBool(_feedingHash, _feeding);
			_animator.SetInteger(_seekingHash, _seeking);
			_animator.SetInteger(_attackHash, _attackType);
			_animator.SetInteger(_stateHash, (int)_currentStateType);
		}

		_satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(_speed, 3.0f));
	}

	protected void UpdateAnimatorDamage() {
		if (_animator != null) {
			_animator.SetBool(_crawlingHash, isCrawling);
			_animator.SetInteger(_lowerBodyHash, _lowerBodyDamage);
			_animator.SetInteger(_upperBodyHash, _upperBodyDamage);
		}
	}

	public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0) {
		if (GameSceneManager.instance != null && GameSceneManager.instance.bloodParticles != null) {
			ParticleSystem system = GameSceneManager.instance.bloodParticles;

			system.transform.position = position;
			var settings = system.main;
			settings.simulationSpace = ParticleSystemSimulationSpace.World;
			system.Emit(60);
		}
		float hitStrength = force.magnitude;

		if (_boneControlType == AIBoneControlType.Ragdoll) {
			if (bodyPart != null) {
				if (hitStrength > 1.0f)
					bodyPart.AddForce(force, ForceMode.Impulse);
				if (bodyPart.CompareTag("Head")) {
					_health = Mathf.Max(_health - damage, 0);
				} else if (bodyPart.CompareTag("Upper Body")) {
					_upperBodyDamage += damage;
				} else if (bodyPart.CompareTag("Lower Body")) {
					_lowerBodyDamage += damage;
				}
				UpdateAnimatorDamage();

				if (_health > 0) {
					//TODO: Reanimate Zombie
				}
			}
			return;
		}

		Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);

		Vector3 hitLocPos = transform.InverseTransformPoint(position);

		bool shouldRagdoll = hitStrength > 1.0f;

		if (bodyPart != null) {
			if (bodyPart.CompareTag("Head")) {
				_health = Mathf.Max(_health - damage, 0);
				if (health == 0)
					shouldRagdoll = true;

			} else if (bodyPart.CompareTag("Upper Body")) {
				_upperBodyDamage += damage;
				UpdateAnimatorDamage();

			} else if (bodyPart.CompareTag("Lower Body")) {
				_lowerBodyDamage += damage;
				UpdateAnimatorDamage();
				shouldRagdoll = true;
			}

			if (_health > 0) {
				if (_reanimationCoroutine != null)
					StopCoroutine(_reanimationCoroutine);

				_reanimationCoroutine = Reanimate();
				StartCoroutine(_reanimationCoroutine);
			}
		}
		if (_boneControlType != AIBoneControlType.Animated || isCrawling || cinematicEnabled || attackerLocPos.z < 0)
			shouldRagdoll = true;
		if (!shouldRagdoll) {

			float angle = 0.0f;
			if (hitDirection == 0) {
				Vector3 vecToHit = (position - transform.position).normalized;
				angle = AIState.FindSignedAngle(vecToHit, transform.forward);
			}
			int hitType = 0;
			if (bodyPart.gameObject.CompareTag("Head")) {
				if (angle < -10 || hitDirection == -1)
					hitType = 1;
				else if (angle > 10 || hitDirection == 1)
					hitType = 3;
				else
					hitType = 2;
			} else if (bodyPart.gameObject.CompareTag("Upper Body")) {
				if (angle < -20 || hitDirection == -1)
					hitType = 4;
				else if (angle > 20 || hitDirection == 1)
					hitType = 6;
				else
					hitType = 5;
			}

			if (_animator) {
				_animator.SetInteger(_hitTypeHash, hitType);
				_animator.SetTrigger(_hitTriggerHash);
			}

			return;

		} else {

			if (_currentState) {
				_currentState.OnExitState();
				_currentState = null;
				_currentStateType = AIStateType.None;
			}

			if (_navAgent)
				_navAgent.enabled = false;
			if (_animator)
				_animator.enabled = false;
			if (_collider)
				_collider.enabled = false;

			inMeleeRange = false;

			foreach (Rigidbody body in _bodyParts) {
				if (body) {
					body.isKinematic = false;
				}
			}

			if (hitStrength > 1.0f)
				bodyPart.AddForce(force, ForceMode.Impulse);

			_boneControlType = AIBoneControlType.Ragdoll;
			if (_health > 0) {
				if (_reanimationCoroutine != null)
					StopCoroutine(_reanimationCoroutine);

				_reanimationCoroutine = Reanimate();
				StartCoroutine(_reanimationCoroutine);
			}
		}
	}

	protected IEnumerator Reanimate() {
		if (_boneControlType != AIBoneControlType.Ragdoll || _animator == null)
			yield break;

		yield return new WaitForSeconds(_reanimationWaitTime);

		_ragdollEndTime = Time.time;

		foreach (Rigidbody body in _bodyParts) {
			body.isKinematic = true;
		}
		_boneControlType = AIBoneControlType.RagdollToAnim;

		foreach (BodyPartSnapshot snapShot in _bodyPartSnapshots) {
			snapShot.position = snapShot.transform.position;
			snapShot.rotation = snapShot.transform.rotation;
		}

		_ragdollHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;
		_ragdollFeetPosition = (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;
		_ragdollHipPosition = _rootBone.position;

		_animator.enabled = true;

		float forwardTest;

		if (_rootBone != null) {
			switch (_rootBoneAlignment) {
				case AIBoneAlignmentType.ZAxis:
					forwardTest = _rootBone.forward.y;
					break;
				case AIBoneAlignmentType.ZAxisInverted:
					forwardTest = -_rootBone.forward.y;
					break;
				case AIBoneAlignmentType.YAxis:
					forwardTest = _rootBone.up.y;
					break;
				case AIBoneAlignmentType.YAxisInverted:
					forwardTest = -_rootBone.up.y;
					break;
				case AIBoneAlignmentType.XAxis:
					forwardTest = _rootBone.right.y;
					break;
				case AIBoneAlignmentType.XAxisInverted:
					forwardTest = -_rootBone.right.y;
					break;
				default:
					forwardTest = _rootBone.forward.y;
					break;
			}

			if (forwardTest >= 0)
				_animator.SetTrigger(_reanimateFromBackHash);
			else
				_animator.SetTrigger(_reanimateFromFrontHash);
		}
	}

	protected virtual void LateUpdate() {
		if (_boneControlType == AIBoneControlType.RagdollToAnim) {
			if (Time.time <= _ragdollEndTime + _mecanimTransitionTime) {
				Vector3 animatedToRagdol = _ragdollHipPosition - _rootBone.position;
				Vector3 newRootPosition = transform.position + animatedToRagdol;

				RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, _geometryLayers);
				newRootPosition.y = float.MinValue;

				foreach (RaycastHit hit in hits) {
					if (!hit.transform.IsChildOf(transform)) {
						newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);
					}
				}

				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 2.0f, NavMesh.AllAreas)) {
					transform.position = navMeshHit.position;
				}

				Vector3 ragdollDirection = _ragdollHeadPosition - _ragdollFeetPosition;
				ragdollDirection.y = 0.0f;

				Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y = 0;

				transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDirection.normalized);
			}

			float blendAmount = Mathf.Clamp01((Time.time - _ragdollEndTime - _mecanimTransitionTime) / _reanimationBlendTime);

			foreach (BodyPartSnapshot snapshot in _bodyPartSnapshots) {
				if (snapshot.transform == _rootBone) {
					snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);
				}
				snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);
			}

			if (blendAmount == 1.0f) {
				_boneControlType = AIBoneControlType.Animated;
				if (_navAgent)
					_navAgent.enabled = true;
				if (_collider)
					_collider.enabled = true;

				AIState newState = null;
				if (_states.TryGetValue(AIStateType.Alerted, out newState)) {
					if (_currentState != null)
						_currentState.OnExitState();

					newState.OnEnterState();
					_currentState = newState;
					_currentStateType = AIStateType.Alerted;
				}
			}
		}
	}
}
