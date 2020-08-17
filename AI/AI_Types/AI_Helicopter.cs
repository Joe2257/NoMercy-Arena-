using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The helicopter is a friendly AI that drop supplies at the end of each round.
public class AI_Helicopter : MonoBehaviour
{
    [SerializeField] private float      _speed;
    [SerializeField] private GameObject _cargoPrefab;
    [SerializeField] private GameObject _cargoToRelease;
    [SerializeField] private Transform  _cargoSpawnPosition;
    [SerializeField] private Transform  _startPosition;

    [SerializeField] private WaypointNetwork _waypointNetwork;

    private Transform _currentWaypointPosition;
    private Transform _nextWaypointPosition;

    [Space]
    public float currentPosition;
    public float distanceToCargoDrop;

    private int  _cargoNumber      = -1;
    private bool _cargoRequested   = false;
    private bool _returnToPosition = false;

    private Rigidbody   _cargoRigidbody;
    private GameManager _gameManager;



    void Start()
    {
        InitializeHelicopter();
    }

    private void InitializeHelicopter()
    {
        _nextWaypointPosition = _waypointNetwork._wayPoints[0];
    }

    void Update()
    {
        MoveToNextWaypoint();
        ReturnToStartPosition();
        ReleaseCargo();
    }

    //____________Navigation_____________\\

    //Pick a randomWaypoint between the waypoints network,
    //repeat the process if the next waypoint has the same position of the current waypoint
    private void CheckCurrentAndNextWaypointPosition()
    {
        if (_currentWaypointPosition.position == _nextWaypointPosition.position)
        {
            _nextWaypointPosition = _waypointNetwork._wayPoints[Random.Range(0, _waypointNetwork._wayPoints.Count)];

            if (_currentWaypointPosition == _nextWaypointPosition)
                CheckCurrentAndNextWaypointPosition();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (_cargoRequested)
        {
            _returnToPosition = false;

            transform.position = Vector3.Lerp(transform.position, _nextWaypointPosition.position, _speed * Time.deltaTime);

            transform.LookAt(_nextWaypointPosition, Vector3.up);
        }
    }

    private void ReturnToStartPosition()
    {
        if (_returnToPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _startPosition.position, 1f);

            transform.LookAt(_startPosition, Vector3.up);
        }
    }

    //____________CargoDrop_____________\\

    //Call the Helicopter in order to drop the cargo around the map
    public void CallCargoDrop()
    {
        _cargoRequested = true;

        _currentWaypointPosition = _nextWaypointPosition;
        CheckCurrentAndNextWaypointPosition();

        SpawnCargo();

        distanceToCargoDrop = Vector3.Distance(transform.position, _nextWaypointPosition.position) / 2;
    }

    //Spawn the cargo prefab and parent it to the helicopter ready to be released.
    private void SpawnCargo()
    {
        GameObject cargoToSpawn = Instantiate(_cargoPrefab, _cargoSpawnPosition.transform.position, _cargoSpawnPosition.transform.rotation);

        if (_cargoNumber <= 3)
            _cargoNumber++;

        _cargoToRelease = cargoToSpawn;
        _cargoRigidbody = _cargoToRelease.GetComponent<Rigidbody>();

        CargoDrop cargoDrop = _cargoToRelease.GetComponent<CargoDrop>();
        cargoDrop._nextWeaponToSpawn = _cargoNumber;

        _cargoRigidbody.useGravity  = false;
        _cargoRigidbody.isKinematic = true;
        _cargoToRelease.transform.parent = this.transform;
    }

    //Detach the cargo from the helicopter and enable phisics to its rigidbody in order to fall on the floor in a realistic way.
    private void ReleaseCargo()
    {
        currentPosition = Vector3.Distance(transform.position, _nextWaypointPosition.position);
        
        if (_cargoToRelease != null && currentPosition < distanceToCargoDrop)
        {
            _cargoToRelease.transform.parent = null;
            _cargoRigidbody.useGravity       = true;
            _cargoRigidbody.isKinematic      = false;

            _cargoToRelease = null;
        }
    }

    //____________Triggers_____________\\

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TargetTrigger")
        {
            _cargoRequested = false;
            _returnToPosition = true;
        }
    }
}
