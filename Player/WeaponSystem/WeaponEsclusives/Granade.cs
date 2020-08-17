using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Simple granade behavior for the granade laucher mounted to the assault rifle.
public class Granade : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float _damage;
    [SerializeField] private float _nadeRadius;
    
    //Explosion particle prefab.
    [SerializeField] private GameObject _nadeExplosion;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   explosion;

    private bool _isExploded = false;


    //Instantiate the explosion particle and damage every damageable object in the radius.
    private void OnTriggerEnter(Collider other)
    {
        if (!_isExploded)
        {
            GameObject nadeExplosion =  Instantiate(_nadeExplosion, transform.position, transform.rotation);
            _isExploded = true;

            audioSource.PlayOneShot(explosion);
            Destroy(nadeExplosion, .5f); Destroy(gameObject, 1f);
        }
      
        //Damage every damageable object in the radius.
       Collider[] collidersHitted = Physics.OverlapSphere(transform.position, _nadeRadius);
      
       foreach (Collider objectsHitByExplosion in collidersHitted)
       {
           if (objectsHitByExplosion.tag == "Enemy")
           {
               if (objectsHitByExplosion.GetComponentInParent<AI_Main>())
               { objectsHitByExplosion.GetComponentInParent<AI_Main>().TakeDamage(_damage);}
           }
       }
    }
}
