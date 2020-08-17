using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The WeaponSystem Class manage all the function of the weapons like: fire, reload, mag and reserve count, and secondary fire.
//(This time instead of making a unique script for all weapons that will have similar or same functions 
// I decided to make a unique WeaponSystem Class to manage all the main function of the weapons, a Weapon Class that stores all the weapon values attached to the weapon object and 
// a SpecialAttacks Class that stores all the secondary fire modes. Every time the player will switch to another weapon the WeaponSystem Class will reset its values
// and comunicate with the new weapon to get all the values like fire rate and damage, while will comunicate with the SpecialAttacks Class to know which special attack to use.)
public enum WeaponType {Null, ThunderBolt, GRN57 , TSU_Nam1, SHW_22, W4, W5, W6, W7 }
public class WeaponSystem : MonoBehaviour
{
    public WeaponType _weaponType;
    public LayerMask _layerMask;

    //Default Variables
    [SerializeField] private float _range;
    [SerializeField] private float _fireRate;
    [SerializeField] private float _nextTimeToFire;
    [SerializeField] private float _spreadFactor;
    [SerializeField] private float _damage;
    [SerializeField] private int   _magazine;

                     private AudioSource _audioSource;
    [SerializeField] private AudioClip   _firingSound;

    public AudioSource audioSource 
    { get { return _audioSource; } set { _audioSource = value; } }

    //Magazines Type
    private int pistolMag = 100;
    private int grnMag    = 150;
    private int tsuMag    = 250;
    private int sniperMag = 45;

    public int _pistolMag
    { get { return pistolMag; } set { pistolMag = value; } }
    public int _grnMag
    { get { return grnMag; } set { grnMag = value; } }
    public int _tsuMag
    { get { return tsuMag; } set { tsuMag = value; } }
    public int _sniperMag
    { get { return sniperMag; } set { sniperMag = value; } }

    //Particles
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private GameObject     _impactFX;

    //Weapon Unlocking
    public bool[] _weaponsUnlocked;

    //Switch Weapon:
    [SerializeField] private int _selectedWeapon = -1;
    [SerializeField] private int _maxWeapons = 7;

    //Components:
    private PlayerController       _controller;
    private PlayerSystem           _playerSystem;
    private Weapons_SpecialAttacks _specialAttacks;

    private GameObject _firePoint;
    private RaycastHit hit;

    private Camera _camera;

    //Getters & Inputs;
    public int magazine
    { get { return _magazine; } set { _magazine = value; } }

    public float firerate
    { get { return _fireRate; } set { _fireRate = value; } }

    public bool specialAttack
    { get { return Input.GetKeyDown(KeyCode.Mouse1); } }

    public Camera cameraMain
    { get { return _camera;} }



    void Start()
    {
        _specialAttacks = GetComponent<Weapons_SpecialAttacks>();
        _controller     = GetComponentInParent<PlayerController>();
        _playerSystem   = GetComponentInParent<PlayerSystem>();
        _audioSource    = GetComponent<AudioSource>();

        InitializeWeaponSystem();
    }

    private void InitializeWeaponSystem()
    {
        SelectWeapon();

        _camera = GetComponentInParent<Camera>();

        for (int i = 0; i < _weaponsUnlocked.Length; i++)
        {
            _weaponsUnlocked[i] = false;
        }
        _weaponsUnlocked[0] = true;
    }


    void Update()
    {
        Fire();
        ScrollWeapon();
        SpecialAttack();
    }


    //Reset all the values of the weapon system making it ready for the next weapon values.
    private void ResetValues()
    {
        _weaponType     = WeaponType.Null;
        _range          = 0;
        _fireRate       = 0;
        _nextTimeToFire = 0;
        _damage         = 0;
        _magazine       = 0;

        _muzzleFlash     = null;
        _impactFX        = null;
    }

    public void UpdateWeaponValue(float range, float fireRate, float nextTTF,float spreadFactor, float damage, ParticleSystem muzzleFlash, GameObject ImpactFx, WeaponType type, AudioClip weaponSound)
    {
        ResetValues();

        _weaponType     = type;
        _range          = range;
        _fireRate       = fireRate;
        _nextTimeToFire = nextTTF;
        _spreadFactor   = spreadFactor;
        _damage         = damage;

        _muzzleFlash = muzzleFlash;
        _impactFX    = ImpactFx;

        _firingSound          = weaponSound;

        if (_weaponType == WeaponType.ThunderBolt)
        { _magazine = pistolMag; }
        else if (_weaponType == WeaponType.GRN57)
        { _magazine = grnMag; }
        else if (_weaponType == WeaponType.TSU_Nam1)
        { _magazine = tsuMag; }
        else if (_weaponType == WeaponType.SHW_22)
        { _magazine = sniperMag; }

    }

    //____________WeaponsSelection_____________\\

    private void SelectWeapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == _selectedWeapon && _weaponsUnlocked[i])
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }

    private void ScrollWeapon()
    {
        int _previousSelectedWeapon = _selectedWeapon;
        int _nextSelectedWeapon     = _selectedWeapon + 1;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (_selectedWeapon >= transform.childCount - 1)
            { _selectedWeapon = 0; }
            else if (_weaponsUnlocked[_nextSelectedWeapon])
            { _selectedWeapon++; }
            else if (!_weaponsUnlocked[_nextSelectedWeapon])
            {
                SelectHigherWeapon();
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {

            if (_selectedWeapon <= 0)
            { SelectHigherWeaponAvailable(); }
            
            if (_selectedWeapon > 0 && _weaponsUnlocked[_selectedWeapon - 1])
            { _selectedWeapon--;}
            else if (_selectedWeapon >= 1 && !_weaponsUnlocked[_selectedWeapon - 1])
            { SelectLowerWeapon();}
            
        }

        if (_previousSelectedWeapon != _selectedWeapon)
        {
            SelectWeapon();
        }

        if (_selectedWeapon == -1)
            ResetValues();
    }

    //Select a stronger weapon
    private void SelectHigherWeapon()
    {
        for (int i = 0; i < _maxWeapons; i++)
        {
            if (i != _selectedWeapon && _weaponsUnlocked[i])
            {
                _selectedWeapon = i;
                break;
            }     
        }
    }

    //Select a weaker weapon
    private void SelectLowerWeapon()
    {
        for (int i = _selectedWeapon; i < _maxWeapons; i--)
        {
            if (i != _selectedWeapon && _weaponsUnlocked[i])
            {
                _selectedWeapon = i;
                break;
            }
        }
    }

    //Select the strongest weapon in the inventory
    private void SelectHigherWeaponAvailable()
    {
        for (int i = _maxWeapons; i > 0; i--)
        {
            if (i != _selectedWeapon && _weaponsUnlocked[i])
            {
                _selectedWeapon = i+1;
                break;
            }
        }
    }

    //____________WeaponFireFunctions_____________\\

    //Read the input and subtract ammo to the reserve.
    private void Fire()
    {
        if (Input.GetKey(KeyCode.Mouse0) && _selectedWeapon > -1)
        {
            if (Time.time > _nextTimeToFire && _magazine > 0)
            {
                if(_weaponType != WeaponType.ThunderBolt)
                _magazine--;

                _muzzleFlash.Play();

                _nextTimeToFire = Time.time + 1 / _fireRate;

                Shoot();
                UpdateMagValue();

                _audioSource.PlayOneShot(_firingSound);
            }
        }
    }

    //Fire the weapon and apply damage.
    private void Shoot()
    {
         Vector3 _bulletDirection = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        _bulletDirection.x += Random.Range(-_spreadFactor, _spreadFactor);
        _bulletDirection.y += Random.Range(-_spreadFactor, _spreadFactor);
        _bulletDirection.z += Random.Range(-_spreadFactor, _spreadFactor);

        Ray ray = _camera.ScreenPointToRay(_bulletDirection);

        if (Physics.Raycast(ray, out hit, _range, _layerMask))
        {

            if (hit.collider.tag == "BodyPart")
            {
                if (hit.collider.GetComponentInParent<AI_Main>())
                { hit.collider.GetComponentInParent<AI_Main>().TakeDamage(_damage); }
            }
            else
           if (hit.collider.tag == "Head")
            {
                if (hit.collider.GetComponentInParent<AI_Main>())
                { hit.collider.GetComponentInParent<AI_Main>().TakeDamage(_damage * 2); }
            }

            GameObject tmp = Instantiate(_impactFX, hit.point, transform.rotation);

            Destroy(tmp, .2f);

        }
    }

    //____________SpecialAttacks_____________\\

    private void SpecialAttack()
    {
       if (_weaponType == WeaponType.ThunderBolt)
       {
           _specialAttacks.ThunderBoltChargedShot();
       }
       else if (specialAttack && _weaponType == WeaponType.GRN57)
       {
           _specialAttacks.GRN_57GranadeBlastInc();
       }
       else if (_weaponType == WeaponType.TSU_Nam1)
       {
           _specialAttacks.TSU_NamiBulletStorm();
       }
       else if (specialAttack && _weaponType == WeaponType.SHW_22)
       {
           _specialAttacks.SHW_22PiercingBlast();
       }
    }

    //____________AllWeaponsReserves_____________\\

    //Update reserve value to make sure that every time a weapon is switched to it will have the previous amount of ammo.
    private void UpdateMagValue()
    {
        if (_weaponType == WeaponType.ThunderBolt)
        {
            _pistolMag = _magazine;
        }
        else if (_weaponType == WeaponType.GRN57)
        {
            grnMag = _magazine;
        }
        else if (_weaponType == WeaponType.TSU_Nam1)
        {
            tsuMag = _magazine;
        }
        else if (_weaponType == WeaponType.SHW_22)
        {
            sniperMag = _magazine;
        }
    }
}
