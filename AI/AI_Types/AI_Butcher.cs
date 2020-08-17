using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//The Butcher is a Tier 3 Ranged AI, it switch between ranged and melee combat based on the distance from the player,
// is faster, deal huge damage and has more health than others.
public class AI_Butcher : AI_Main
{
    
    [Header("Main Variables")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float     _runSpeed;
    [SerializeField] private float     _stoppingDistance;
    [Space]

    [Header("Combat Variables")]
    public GameObject damageTriggerRight;
    public GameObject damageTriggerThorns;
    public GameObject projectile;
    public Transform  firePoint;
    public float      projectileSpeed;
    

    private GameObject _tmpProjectile;
    private bool       _projectileSpawned = false;
    private bool       _soundPlayed       = false;
    private bool       _isDead            = false;

    //DamageTriggersParameters
    private int   _rightHash       = -1;
    private int   _thornsHash      = -1;
    private int   _rangedHash      = -1;

    [SerializeField] private string _rightParameter  = "";
    [SerializeField] private string _thornsParameter = "";
    [SerializeField] private string _rangedParameter = "";

    //Audio
    [SerializeField] private AudioClip[] _attackSound;
    [SerializeField] private AudioClip   _butcherFireball;
    [SerializeField] private AudioClip   _deathSound;



    void Start()
    {
        _rangedMeleeTrigger = GetComponentInChildren<RangedMeleeTrigger>();
        _animator           = GetComponent<Animator>();
        _navAgent           = GetComponent<NavMeshAgent>();
        _collider           = GetComponent<CapsuleCollider>();
        _gameManager        = FindObjectOfType<GameManager>();
        _audioSource        = GetComponent<AudioSource>();

        InitializeAISystem();
    }

    //Secondary function made to keep the Start function clean.
    private void InitializeAISystem()
    {
        _rightHash  = Animator.StringToHash(_rightParameter);
        _thornsHash = Animator.StringToHash(_thornsParameter);
        _rangedHash = Animator.StringToHash(_rangedParameter);

        _currentHealthPoints = _healthPoints;

        damageTriggerRight.SetActive(false);
        damageTriggerThorns.SetActive(false);

        EngageCombat();
    }

    //____________Updates_____________\\

    private void FixedUpdate()
    {
        switch (_stateMachine)
        {
            default:
                break;
            case AI_StateMachine.Idle:
                Idle();
                break;
            case AI_StateMachine.Chase:
                Chase();
                break;
            case AI_StateMachine.RangedAttack:
                RangedAttack();
                break;
            case AI_StateMachine.Attack:
                Attack();
                break;
            case AI_StateMachine.Die:
                Die();
                break;
        }
    }

    void Update()
    {
        ThrowFireBall();
        ExecuteDeath();
    }

    //____________States_____________\\

    private void Idle()
    {
        _animator.SetFloat("Speed", 0f);
        _animator.SetFloat("Attack", 0f);
        _navAgent.speed = 0;

        _stateMachine = AI_StateMachine.Chase;
    }

    private void Chase()
    {
        if (_animator.GetFloat(_rightParameter) < 1 && _animator.GetFloat(_thornsParameter) < 1 && _animator.GetFloat(_rangedParameter) < 1f)
        {
            _animator.SetFloat("Speed", 2f);
            _animator.SetFloat("Attack", 0f);
            _animator.SetFloat("Fireball", 0f);

            _navAgent.stoppingDistance = _stoppingDistance;
            _navAgent.speed = _runSpeed;

            SetNewDestination(_gameManager._playerPosition);

            _soundPlayed = false;
        }

        if (Vector3.Distance(_gameManager._playerPosition.transform.position, this.transform.position) < 35.0f && !_rangedMeleeTrigger._targetInRangeForMelee)
        {
            _stateMachine = AI_StateMachine.RangedAttack;
        }
    }

    private void Attack()
    {
        _navAgent.speed = 0;

        _animator.SetFloat("Speed", 0f);
        _animator.SetFloat("Attack", 1f);

        //The damage triggers are enabled only when the ai is in the correct animation position.
        if (_animator.GetFloat(_rightParameter) > .5f)
        { 
            damageTriggerRight.SetActive(true);
            if (!_soundPlayed)
            {
                _soundPlayed = true;
                PlayAttackSound();
            }
        }
        else if (_animator.GetFloat(_rightParameter) < .5f)
        { damageTriggerRight.SetActive(false); }

        if (_animator.GetFloat(_thornsParameter) > .5f)
        {
            damageTriggerThorns.SetActive(true);
            if (!_soundPlayed)
            {
                _soundPlayed = true;
                PlayAttackSound();
            }
        }
        else if (_animator.GetFloat(_thornsParameter) < .5f)
        { damageTriggerThorns.SetActive(false); }

    }

    private void RangedAttack()
    {
        Ray ray = new Ray(this.transform.position, _gameManager._playerPosition.position - this.transform.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30f, _layerMask))
        {
            if (hit.transform.tag == "Player")
            {

                _navAgent.speed = 0;
                _animator.SetFloat("Speed", 0f);
                _animator.SetFloat("Attack", 0f);
                _animator.SetFloat("Fireball", 1f);
            }
            else
            { SetNewDestination(_gameManager._playerPosition); _stateMachine = AI_StateMachine.Chase; }
        }

        if (Vector3.Distance(_gameManager._playerPosition.transform.position, this.transform.position) > 35.0f && !_projectileSpawned)
        {
            _stateMachine = AI_StateMachine.Chase;
        }
        else if (_rangedMeleeTrigger._targetInRangeForMelee && !_projectileSpawned && _animator.GetFloat(_rangedParameter) < 1f)
        {
            _stateMachine = AI_StateMachine.Chase;
        }
    }

    private void Die()
    {
        _collider.enabled = false;
        _navAgent.enabled = false;

        _animator.SetTrigger("Death");
        _animator.SetFloat("Speed", 0);
        _animator.SetFloat("Attack", 0);

        if (!_isDead)
        {
            _audioSource.PlayOneShot(_deathSound);
            _isDead = true;
        }
    }

    //____________Combat_____________\\

    private void EngageCombat()
    {
        _stateMachine = AI_StateMachine.Chase;
    }

    private void PlayAttackSound()
    {
        _audioSource.PlayOneShot(_attackSound[Random.Range(0, _attackSound.Length)]);
    }

    //Throw a fireball that explode on impact.
    private void ThrowFireBall()
    {
        if (_currentHealthPoints > 0)
        {
            if (!_projectileSpawned && _animator.GetFloat(_rangedParameter) > 1f)
            {
                transform.LookAt(_gameManager._playerPosition, Vector3.up);
            }

            if (!_projectileSpawned && _animator.GetFloat(_rangedParameter) == 2f)
            {
                _projectileSpawned = true;
                _soundPlayed = false;

                _tmpProjectile = Instantiate(projectile, firePoint.transform.position, projectile.transform.rotation);

                _tmpProjectile.transform.parent = firePoint;
            }

            if (_animator.GetFloat(_rangedParameter) > 2f)
            {
                _targetTrigger.transform.position = _gameManager._playerPosition.position;

                if (_tmpProjectile != null)
                {
                    _tmpProjectile.transform.parent = null;

                    Rigidbody projectileRGB = _tmpProjectile.GetComponent<Rigidbody>();
                    SphereCollider projectileCollider = _tmpProjectile.GetComponent<SphereCollider>();


                    projectileRGB.useGravity = true;
                    projectileRGB.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);

                    projectileCollider.enabled = true;

                    if (!_soundPlayed)
                    { _audioSource.PlayOneShot(_butcherFireball); _soundPlayed = true; }

                }

                _projectileSpawned = false;
            }
        }
        else if (_tmpProjectile != null)
            Destroy(_tmpProjectile);
        
    }

    //____________Triggers_____________\\

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "AttackTrigger")
        {
            if (_navAgent)
                _navAgent.isStopped = true;

            _stateMachine = AI_StateMachine.Attack;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "AttackTrigger")
        {
            if (_navAgent)
                _navAgent.isStopped = false;

            _stateMachine = AI_StateMachine.Chase;
        }
    }
}
