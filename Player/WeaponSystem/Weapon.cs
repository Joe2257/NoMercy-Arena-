using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The weapon class is attached to every weapon object and stores all the values that need to be transfered to the weapon system like:
// damage, fire rate, range etc...
public class Weapon : MonoBehaviour
{
    public WeaponType _type;

    [SerializeField] private float  _weaponRange;
    [SerializeField] private float  _weaponFireRate;
    [SerializeField] private float  _weaponNextTimeToFire;
    [SerializeField] private float  _weaponSpreadFactor;
    [SerializeField] private float  _weaponDamage;

    [SerializeField] private AudioClip _weaponSound;

    private WeaponSystem _weaponSystem;

    public ParticleSystem weaponMuzzleFlash;
    public GameObject     weaponImpactFX;

    private void OnEnable()
    {
        _weaponSystem = GetComponentInParent<WeaponSystem>();

        if (_weaponSystem)
            _weaponSystem.UpdateWeaponValue(_weaponRange, _weaponFireRate, _weaponNextTimeToFire, _weaponSpreadFactor, 
                                            _weaponDamage, weaponMuzzleFlash, weaponImpactFX, _type, _weaponSound);
    }
}
