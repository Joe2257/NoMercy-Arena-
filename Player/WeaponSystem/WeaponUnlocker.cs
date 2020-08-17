using UnityEngine;

//Simple Class use to unlock weapons in the inventory when the trigger attached to the player collide with it,
// every unlockable weapon has an ID assigned to it that will unlock the weapon in question from the inventory.
public class WeaponUnlocker : MonoBehaviour
{
    [SerializeField] private int _weaponId;

    public GameObject _weapon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            WeaponSystem _weaponSys = other.gameObject.GetComponentInChildren<WeaponSystem>();

            _weaponSys._weaponsUnlocked[_weaponId] = true;

            _weapon.SetActive(false);
        }
    }
}
