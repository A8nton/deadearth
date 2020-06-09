using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

public class AIZombieStateMachine : AIStateMachine {

    [SerializeField]
    [Range(10, 360)]
    float _fieldOfView = 50;

    [SerializeField]
    [Range(0, 1)]
    float _sight = 0.5f;

    [SerializeField]
    [Range(0, 1)]
    float _hearing = 1;

    [SerializeField]
    [Range(0, 1)]
    float _aggression = 0.5f;

    [SerializeField]
    [Range(0, 100)]
    int _health = 100;

    [SerializeField]
    [Range(0, 1)]
    float _intelligence = 0.5f;

    [SerializeField]
    [Range(0, 1)]
    float _satisfaction = 0.5f;


    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;

    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");

    public float fieldOfView { get => _fieldOfView; }
    public float sight { get => _sight; }
    public float hearing { get => _hearing; }
    public int health { get => _health; }
    public float intelligence { get => _intelligence; }
    public float aggression { get => _aggression; set => _aggression = value; }
    public float satisfaction { get => _satisfaction; set => _satisfaction = value; }
    public bool feeding { get => _feeding; set => _feeding = value; }
    public bool crawling { get => _crawling; set => _crawling = value; }
    public int attackType { get => _attackType; set => _attackType = value; }
    public int seeking { get => _seeking; set => _seeking = value; }

    public float speed {
        get { return _navAgent != null ? _navAgent.speed : 0.0f; }
        set { if (_navAgent != null) _navAgent.speed = value; }
    }

    protected override void Update() {
        base.Update();

        if (_animator != null) {

            _animator.SetFloat(_speedHash, _navAgent.speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger (_seekingHash, _seeking);
            _animator.SetInteger (_attackHash, _attackType);

        }
    }
}
