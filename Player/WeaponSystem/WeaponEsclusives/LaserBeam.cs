using UnityEngine;


//Simple laser behavior for the laser beam mounted on the pistol.
public class LaserBeam : MonoBehaviour
{
    [SerializeField] private float _damage;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "BodyPart")
        {
            if (other.gameObject.GetComponentInParent<AI_Main>())
            { other.gameObject.GetComponentInParent<AI_Main>().TakeDamage(_damage * Time.deltaTime * 2); }
        }
        else
        if (other.gameObject.tag == "Head")
        {
            if (other.gameObject.GetComponentInParent<AI_Main>())
            { other.gameObject.GetComponentInParent<AI_Main>().TakeDamage(_damage * 2 * Time.deltaTime * 2); }
        }
    }

}
