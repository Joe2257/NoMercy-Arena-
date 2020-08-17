using System.Collections;
using UnityEngine;


//The Weapons_SpecialAttacks Class stores all the secondary fire modes of the guns and communicate with the WeaponSystem Class.
public class Weapons_SpecialAttacks : MonoBehaviour
{
    public WeaponType _weapontype;


    [Header("ThunderBolt")]
    [SerializeField] private GameObject _laser;
    private float                       _ammoToSubtrac = 150f;

    private bool _isFiring = false;

    [Header("GRN_57")]
    [SerializeField] private float          _granadeSpeed;
    [SerializeField] private float          _granadeBlastCoolDown;
    [SerializeField] private Transform      _granadeFirePoint;
    [SerializeField] private GameObject     _granadeObject;

    private bool _granadeReady = true;


    [Header("SHW_22")]
    public LayerMask _layerMask;
    [SerializeField] private float          _range;
    [SerializeField] private float          _piercingShotDamage;
    [SerializeField] private ParticleSystem _piercingShotFX;
    [SerializeField] private AudioClip      _piercingShotAudio;

    private WeaponSystem _weaponSystem;

    void Start()
    {
        _weaponSystem = GetComponent<WeaponSystem>();
    }

    void Update()
    {
        _weapontype = _weaponSystem._weaponType;
    }

    //____________Pistol "ThunderBolt"_____________\\

    public void ThunderBoltChargedShot()
    {
        if (_weaponSystem.magazine >= 2)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                _laser.SetActive(true);

                _isFiring = true;
            }
            else
            {
                _laser.SetActive(false);
                _ammoToSubtrac = _weaponSystem.magazine;
                _isFiring = false;
            }

            if (_isFiring)
            {
                _ammoToSubtrac -= 2 * Time.deltaTime * 3;
                _weaponSystem.magazine = (int)_ammoToSubtrac;
            }
        }
        else
            _laser.SetActive(false);
    }

    //____________AssaultRifle "GRN_57"_____________\\

    public void GRN_57GranadeBlastInc()
    {
        if (_granadeReady && _weaponSystem.magazine >= 15)
        {
            StartCoroutine(GRN_57GranadeBlast());

            _weaponSystem.magazine -= 15;
        }
    }

    private IEnumerator GRN_57GranadeBlast()
    {   
      _granadeReady = false;

      GameObject Nade = Instantiate(_granadeObject, _granadeFirePoint.position, _granadeFirePoint.rotation);
      Rigidbody _nadeBody = Nade.GetComponent<Rigidbody>();

      _nadeBody.AddForce(transform.forward * _granadeSpeed, ForceMode.VelocityChange);

        yield return new WaitForSeconds(_granadeBlastCoolDown);

        _granadeReady = true;
    }

    //____________MachineGun "TSU_57"_____________\\

    public void TSU_BulletStorm()
    {
        if (_weaponSystem.magazine > 0)
        {
            if (_weaponSystem.specialAttack) { _weaponSystem.firerate = _weaponSystem.firerate * 2; }
            else if (Input.GetKeyUp(KeyCode.Mouse1)) { _weaponSystem.firerate = _weaponSystem.firerate / 2; }
        }
    }

    //____________Sniper "SHW_22"_____________\\

    public void SHW_22PiercingBlast()
    {
        if (_weaponSystem.magazine >= 10)
        {
            _weaponSystem.magazine -= 10;
            _weaponSystem.audioSource.PlayOneShot(_piercingShotAudio);
            _piercingShotFX.Play();

            Vector3 _bulletDirection = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            RaycastHit[] hit;
            Ray ray = _weaponSystem.cameraMain.ScreenPointToRay(_bulletDirection);

            hit = Physics.RaycastAll(ray, _range, _layerMask);


            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider.tag == "BodyPart")
                {
                    if (hit[i].collider.GetComponentInParent<AI_Main>())
                    { hit[i].collider.GetComponentInParent<AI_Main>().TakeDamage(_piercingShotDamage); }
                }
            }
        }
    }   
}
