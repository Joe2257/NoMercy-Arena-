using UnityEngine.AI;
using UnityEngine;
using System.Collections;


//Ineritance Test(This script has been created to test class inheritance feature for educational purposes).
public enum AI_Type {Null, Melee, Ranged, Butcher, Spawner, SpawnerEgg, Ogre, Araknid}
public enum AI_StateMachine { Idle, Chase, Attack, RangedAttack, Die }

public class AI_Main : MonoBehaviour
{
    public AI_Type         _ai_Type;
    public AI_StateMachine _stateMachine;

    public bool  _isSpawnedBySummoners = false;
    [Space]
    public float _healthPoints;
    public float currentHealthPoints;

    public float _currentHealthPoints
    { get { return currentHealthPoints; } set { currentHealthPoints = value; }}

    private bool isDead = false;
    private bool isPathSet = true;

    [SerializeField]private bool _playerInRange = false;

    //Components
    private Animator            animator;
    private NavMeshAgent        navAgent;
    private GameManager         gameManager;
    private RangedMeleeTrigger  rangedMeleeTrigger;
    private new CapsuleCollider     collider;
    private AudioSource         audioSource;


    public RangedMeleeTrigger _rangedMeleeTrigger
    { get { return rangedMeleeTrigger; } set { rangedMeleeTrigger = value; } }
    public Animator _animator
    {get { return animator; } set { animator = value; } }
    public NavMeshAgent _navAgent
    { get { return navAgent; } set { navAgent = value; } }
    public GameManager _gameManager
    { get { return gameManager; } set { gameManager = value; } }
    public CapsuleCollider _collider
    { get { return collider; } set { collider = value; } }
    public AudioSource _audioSource
    { get { return audioSource; } set { audioSource = value; } }

    //Navigation
    public GameObject          _targetTrigger;
    public GameObject          _omniAI;
    public int _newDestination = 0;

    //____________SecondaryFunctions_____________\\

    //This functions are called by the various AI classes and will be the same for all the classes
    // so they are stored in a single script.
    public void SetNewDestination(Transform Destination)
    {
        _navAgent.SetDestination(Destination.position);
        NavMesh.pathfindingIterationsPerFrame = 300;
    }

    public void TakeDamage(float amount)
    {
        _currentHealthPoints -= amount;
    }

    public void ChasePlayer()
    {
        if (_currentHealthPoints > 0)
        {
            _stateMachine = AI_StateMachine.Chase;
        }
    }

    public void ExecuteDeath()
    {
        if (_currentHealthPoints <= 0)
        {
            if (!isDead && !_isSpawnedBySummoners)
            {
               _gameManager._mobsCount--;
                isDead = true;
            }
              
            _stateMachine = AI_StateMachine.Die;
            Destroy(_omniAI, 3f);
        }
    }

    public void InRangeForAttack()
    {
        if (_playerInRange)
        {
            if (_navAgent.isActiveAndEnabled)
                _navAgent.isStopped = true;

            _stateMachine = AI_StateMachine.Attack;
        }
        else if (!_playerInRange)
        {
            if (_navAgent.isActiveAndEnabled)
                _navAgent.isStopped = false;

            _animator.SetFloat("Attack", 0);

            _stateMachine = AI_StateMachine.Chase;
        }
    }


    //____________Triggers_____________\\
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "AttackTrigger")
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "AttackTrigger")
        {
            _playerInRange = false;
        }
    }
}
