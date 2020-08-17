using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The GameManager keep track of: the player position around the map/ rounds count/
// mob spawn and mob count per round/ when to call the cargo drop.
public class GameManager : MonoBehaviour
{
    public GameObject _player;
    public Transform  _playerPosition;


    [SerializeField] private float _timeBetweenRounds = 60f;

    public int  mobsCount        = 0;
    public int  currentRound     = 0;
    public bool roundHasStarted  = false;

    public  AI_Helicopter helicopter;

    private Mob_Spawner _mobSpawner;
    private int _previousRound;

    

    void Start()
    {
        _mobSpawner = FindObjectOfType<Mob_Spawner>();

        InitializeRound();
    }

    private void InitializeRound()
    {
        StartCoroutine(TimeBetweenRounds());
    }

    
    void Update()
    {
        _playerPosition = _player.transform;

        MobsCountPerRound();
        RoundUpdates();
    }

    //____________RoundFeatures_____________\\

    //Determine how many and what type of AI need to be spawn based on the round.
    private void MobsCountPerRound()
    {
        if (currentRound == 0)
           _timeBetweenRounds = 30f;
        else
           _timeBetweenRounds = 60f;

        if (currentRound >= 0 && currentRound < 4)
        {
            _mobSpawner._lowTierMobsToSpawn = 20;
        }
        else if (currentRound > 4 && currentRound < 8)
        {
            _mobSpawner._lowTierMobsToSpawn = 25;
            _mobSpawner._midTierMobsToSpawn = 15;
        }
        else if (currentRound > 8 && currentRound < 16)
        {
            _mobSpawner._lowTierMobsToSpawn  = 25;
            _mobSpawner._midTierMobsToSpawn  = 15;
            _mobSpawner._highTierMobsToSpawn = 10;
        }
    }

    //Start the TimeBetweenRounds CoRoutine that give the player 60 seconds to look for resources before the next round start,
    // the helicopter will drop supplies in a random point of the map after the CoRoutine has been called.
    private void RoundUpdates()
    {
        if (mobsCount <= 1 && roundHasStarted)
        {
            StartCoroutine(TimeBetweenRounds());

            currentRound++;

            helicopter.CallCargoDrop();
        }
    }

    private IEnumerator TimeBetweenRounds()
    {
        roundHasStarted = false;

        yield return new WaitForSeconds(_timeBetweenRounds);

        StartCoroutine(_mobSpawner.SpawnMobs(_mobSpawner._lowTierMobsToSpawn, _mobSpawner._midTierMobsToSpawn, _mobSpawner._highTierMobsToSpawn)); 
        roundHasStarted = true;
    }
}
