using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoDrop : MonoBehaviour
{

    [SerializeField] private GameObject   _cargoLid;
    [SerializeField] private GameObject[] _weaponsToSpawn;
    [SerializeField] private GameObject[] _objectToSpawn;
    [SerializeField] private GameObject   _spawnPosition;

    //Witch weapon to spawn based on the round.
    public int nextWeaponToSpawn = 0;
    //Make the cargo more visible while in air.
    public ParticleSystem flare;

    private GameObject _itemToSpawn;
    private GameObject _weaponToSpawn;
    private Rigidbody  _lidRigidbody;

    private bool _itemHaveBeenSpawned  = false;
    private int  _amountOfItemToSpawn  = 3;



    private void Start()
    {
        _lidRigidbody = _cargoLid.GetComponent<Rigidbody>();
    }

    //Spawns items and weapons inside the cargo.
    private void SpawnItemsAndWeapons()
    {
        if (nextWeaponToSpawn > 2)
            _amountOfItemToSpawn = 6;

        for (int i = 0; i < _amountOfItemToSpawn; i++)
        {
            _itemToSpawn = Instantiate(_objectToSpawn[Random.Range(0, _objectToSpawn.Length)], _spawnPosition.transform.position, _spawnPosition.transform.rotation);
        }

        if (nextWeaponToSpawn <= 2)
        {
            _weaponToSpawn = Instantiate(_weaponsToSpawn[nextWeaponToSpawn], _spawnPosition.transform.position, _weaponsToSpawn[nextWeaponToSpawn].transform.rotation); 
        }
    }


    //____________Collisions_____________\\
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain" || collision.gameObject.tag == "Untagged")
        {
            if (!_itemHaveBeenSpawned)
            { SpawnItemsAndWeapons(); _itemHaveBeenSpawned = true; }

            _lidRigidbody.isKinematic = false;
            _lidRigidbody.useGravity  = true;

            flare.Stop();

            Destroy(this.gameObject, 30f);
        }
            
    }
}
