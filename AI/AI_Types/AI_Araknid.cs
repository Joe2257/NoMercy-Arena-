using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//The Araknid is a Tier 3 AI, it has its own special attack, is faster, deal more damage and has more health than others.
public class AI_Araknid : AI_Main
{
    private bool _soundPlayed = false;
    private bool _laserActive = false;
    private bool _isDead      = false;


    [Header("Main Variables")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [Space]

    [Header("Combat Variables")]
    public GameObject              _damageTriggerRight;
    public GameObject              _damageTriggerLeft;
    public GameObject              _projectile;
    public Transform               _firePoint;
    [SerializeField] private float _stoppingDistance;

    [Header("Laser")]
    [SerializeField] private GameObject _laserBeam;
    public bool _activateLaser = true;

    private GameObject _tmpProjectile;

    //DamageTriggersParameters
    private int _rightHash    = -1;
    private int _leftHash     = -1;
    private int _laserHash    = -1;
    private int _pushBackHash = -1;

    [SerializeField] private string _rightParameter  = "";
    [SerializeField] private string _leftParameter   = "";
    [SerializeField] private string _laserParameter  = "";
    [SerializeField] private string _pushParameter   = "";

    //Audio
    [SerializeField] private AudioClip[] _attackSound;
    [SerializeField] private AudioClip   _araknidLaser;
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
        _rightHash    = Animator.StringToHash(_rightParameter);
        _leftHash     = Animator.StringToHash(_leftParameter);
        _pushBackHash = Animator.StringToHash(_pushParameter);
        _laserHash    = Animator.StringToHash(_laserParameter);

        _currentHealthPoints = _healthPoints;

        _damageTriggerRight.SetActive(false);
        _damageTriggerLeft.SetActive(false);
        _laserBeam.SetActive(false);


        _stateMachine = AI_StateMachine.Chase;
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
        ActivateLaser();
        ExecuteDeath();
    }

    //____________States_____________\\

    private void Idle()
    {
        _animator.SetFloat("Speed", 0f);
        _animator.SetFloat("Attack", 0f);
        _navAgent.speed = 0;

        EngageCombat();
    }

    private void Chase()
    {
        if (_animator.GetFloat(_rightParameter) < 1 && _animator.GetFloat(_leftParameter) < 1 && _animator.GetFloat(_laserParameter) < 1)
        {
            _animator.SetFloat("Speed", 2f);
            _animator.SetFloat("Attack", 0f);

            _navAgent.stoppingDistance = _stoppingDistance;
            _navAgent.speed            = _runSpeed;

            SetNewDestination(_gameManager._playerPosition);

            _soundPlayed = false;
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
            _damageTriggerRight.SetActive(true);
            if (!_soundPlayed)
            {
                _soundPlayed = true;
                PlayAttackSound();
            }
        }
        else if (_animator.GetFloat(_rightParameter) < .5f)
        { _damageTriggerRight.SetActive(false); }

        if (_animator.GetFloat(_leftParameter) > .5f)
        { _damageTriggerLeft.SetActive(true); }
        else if (_animator.GetFloat(_leftParameter) < .5f)
        { _damageTriggerLeft.SetActive(false); }

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

    //This is the Araknid special attack a laser beam that will pierce through any object but it has a short range.
    private void ActivateLaser()
    {
        if (_gameManager._playerPosition)
            if (Vector3.Distance(this.transform.position, _gameManager._playerPosition.transform.position) < 15.0f && _activateLaser && _currentHealthPoints > 0)
            {
                _activateLaser = false;
                _animator.SetTrigger("LaserBeam");
            }

        if (_animator.GetFloat(_laserParameter) > 1)
        {
            transform.LookAt(_gameManager._playerPosition, Vector3.up);
            _navAgent.speed = 0;
            _laserBeam.SetActive(true);
            StartCoroutine(LaserReset());

            if (!_laserActive)
            { _audioSource.PlayOneShot(_araknidLaser); _laserActive = true; }
        }
        else
        {
            _laserBeam.SetActive(false);
            _laserActive = false;
        }
    }

    //Reset the laserTrigger in the animator and start a timer before another laser beam can be shoot.
    private IEnumerator LaserReset()
    {
        _animator.ResetTrigger("LaserBeam");

        yield return new WaitForSeconds(10f);

        _activateLaser = true;
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
