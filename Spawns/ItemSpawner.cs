using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The item spawner spaw a random item like: ammo, meds etc.. 
// every 60 seconds it has a small "chance" to spawn an item on his position, if the item is not spawned
// the timer will restart untill an item is spawned if the items is spawned the timer will stop untill the item is picked up.
// (I made this feature to avoid too many supplies being spawned in the map so the player will need to move constantly around the map).
public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] _itemsToSpawn;
    [SerializeField] private GameObject   _objectSpawned = null;
    [SerializeField] private float        _requirementToSpawnItem = 70f;

    private GameObject _spawnedItem;

    [Header("Timer")]
    public float  timeBeforeSpawn = 60f;
    public float  timer           = 0f;
    public float  timerTick       = 1;



    void Update()
    {
        SpawnTimer();
    }

    private void SpawnTimer ()
    {
        if(_objectSpawned == null)
        timer += timerTick * Time.deltaTime;

        if (timer >= timeBeforeSpawn && _objectSpawned == null)
        {
            float chanceToSpawn;

            timer = 0;
            chanceToSpawn = Random.Range(0f, 100f);

            if (chanceToSpawn > _requirementToSpawnItem)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        _spawnedItem = Instantiate(_itemsToSpawn[Random.Range(0, _itemsToSpawn.Length)], transform.position, transform.rotation);
        _objectSpawned = _spawnedItem;
    }
}
