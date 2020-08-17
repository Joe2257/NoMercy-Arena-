using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//The Mob_Spawner communicate with the GameManager to spawn the mobs on the map, not all mobs will be spawned in the same time window 
// to avoid fps drops cause by instantiating too many AI's.
//The mobs will be spawned in a order based on their tier, low => mid => high,
//so that by the time the higher tier spawn some low tier will already be dead, helping to keep the game more performant.
public class Mob_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _lowTierMobs;
    [SerializeField] private GameObject[] _mediumTierMobs;
    [SerializeField] private GameObject[] _highTierMobs;

    public GameObject[] spawnPositions;

    public int lowTierMobsToSpawn;
    public int midTierMobsToSpawn;
    public int highTierMobsToSpawn;

    public bool spawnNewMobs = false;

    private GameObject _mob;

    private GameManager _gameManager;



    void Start()
    {
       _gameManager = FindObjectOfType<GameManager>();
    }

    //Spawn mobs in random positions around the map.
    public IEnumerator SpawnMobs(float lowTier, float mediumTier, float highTier)
    {
        if (lowTier > 0)
        {
            for (int i = 0; i < lowTier; i++)
            {
                _mob = Instantiate(_lowTierMobs[Random.Range(0, _lowTierMobs.Length)], 
                    spawnPositions[Random.Range(0, spawnPositions.Length)].transform.position, spawnPositions[Random.Range(0, spawnPositions.Length)].transform.rotation);

                _gameManager._mobsCount++;
            }
        }
        yield return new WaitForSeconds(30f);

        if (mediumTier > 0)
        {
            for (int i = 0; i < mediumTier; i++)
            {
                _mob = Instantiate(_mediumTierMobs[Random.Range(0, _mediumTierMobs.Length)], 
                    spawnPositions[Random.Range(0, spawnPositions.Length)].transform.position, spawnPositions[Random.Range(0, spawnPositions.Length)].transform.rotation);

                _gameManager._mobsCount++;
            }
        }
        yield return new WaitForSeconds(30f);

        if (highTier > 0)
        {
            for (int i = 0; i < mediumTier; i++)
            {
                _mob = Instantiate(_highTierMobs[Random.Range(0, _highTierMobs.Length)],
                    spawnPositions[Random.Range(0, spawnPositions.Length)].transform.position, spawnPositions[Random.Range(0, spawnPositions.Length)].transform.rotation);

                _gameManager._mobsCount++;
            }
        }
    }
}
