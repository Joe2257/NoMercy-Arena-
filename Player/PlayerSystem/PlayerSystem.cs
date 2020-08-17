using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

//The player systems manage the damage dealth to the player and the pickups collected.
public class PlayerSystem : MonoBehaviour
{
    private PlayerController _controller;
    private PlayerMovement   _playerMovement;
    private WeaponSystem     _weaponSystem;

    public float healthPoints = 100f;
    public float armorPoints = 100f;

    [HideInInspector] public int _magazine;

    public float[] medkits;
    public int     medKitsUsage = 0;

    //Audio.
    private AudioSource _audioSource;
    public AudioClip[]   damageSound;



    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _weaponSystem   = GetComponentInChildren<WeaponSystem>();
        _audioSource    = GetComponent<AudioSource>();
    }

    void Update()
    {
        _magazine = _weaponSystem.magazine;
    }

    //Take damage and divide it between armor points and health points.
    public void TakeDamage(float value)
    {
        if (armorPoints > 0)
        {
            float damage = value / 4;
            float armorDamage = value - damage;

            armorPoints -= armorDamage;
            healthPoints -= damage;

            if (armorPoints < 0)
                armorPoints = 0;

            _audioSource.PlayOneShot(damageSound[Random.Range(0, damageSound.Length)]);
        }
        else
        { healthPoints -= value;}
    }

    //____________Triggers_____________\\

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Interactive")
        {
            if (other.gameObject.name == "Shield")
            {
                if (armorPoints < 100)
                {
                    armorPoints += Random.Range(10f, 20f);

                    other.gameObject.SetActive(false);
                }

                if (armorPoints > 100)
                    armorPoints -= 1 * Time.deltaTime;
            }
            else if (other.gameObject.name == "Medkit")
            {
                float pointsToHeal = Random.Range(15f, 30f);

                if (healthPoints < 100f)
                {
                    healthPoints += pointsToHeal;
                    other.gameObject.SetActive(false);
                }
                else if (healthPoints >= 100f)
                {
                    for (int i = 0; i < medkits.Length; i++)
                    {
                        if (medkits[i] == 0)
                        { medkits[i] += pointsToHeal; other.gameObject.SetActive(false); medKitsUsage++; break; }
                    }
                }
            }
            else if (other.gameObject.name == "Pistol_Magazine" && _weaponSystem._pistolMag < 100)
            {
                _weaponSystem._pistolMag += Random.Range(10, 20);

                if (_weaponSystem._pistolMag > 100)
                    _weaponSystem._pistolMag = 100;

                if (_weaponSystem._weaponType == WeaponType.ThunderBolt)
                    _weaponSystem.magazine = _weaponSystem._pistolMag;

                other.gameObject.SetActive(false);
            }
            else if (other.gameObject.name == "Grn_Magazine" && _weaponSystem._grnMag < 150)
            {
                _weaponSystem._grnMag += Random.Range(30, 60);

                if (_weaponSystem._grnMag > 150)
                    _weaponSystem._grnMag = 150;

                if (_weaponSystem._weaponType == WeaponType.GRN57)
                    _weaponSystem.magazine = _weaponSystem._grnMag;

                other.gameObject.SetActive(false);
            }
            else if (other.gameObject.name == "7su_Magazine" && _weaponSystem._tsuMag < 250)
            {
                _weaponSystem._tsuMag += Random.Range(40, 70);

                if (_weaponSystem._tsuMag > 250)
                    _weaponSystem._tsuMag = 250;

                if (_weaponSystem._weaponType == WeaponType.TSU_Nam1)
                    _weaponSystem.magazine = _weaponSystem._tsuMag;

                other.gameObject.SetActive(false);
            }
            else if (other.gameObject.name == "Sniper_Magazine" && _weaponSystem._sniperMag < 45)
            {
                _weaponSystem._sniperMag += Random.Range(5, 10);

                if (_weaponSystem._sniperMag > 45)
                    _weaponSystem._sniperMag = 45;

                if (_weaponSystem._weaponType == WeaponType.SHW_22)
                    _weaponSystem.magazine = _weaponSystem._sniperMag;

                other.gameObject.SetActive(false);
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Poison")
        {
            healthPoints -= 5f * Time.deltaTime;

            _playerMovement._walkSpeed = 6f;
        }
        else
        {
            _playerMovement._walkSpeed = 12f;
        }

        if (other.gameObject.tag == "Laser")
        {
            healthPoints -= 30f * Time.deltaTime;
        }
    }
}
