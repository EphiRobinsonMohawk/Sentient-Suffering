using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatHuman : MonoBehaviour
{
    private ParticleSystem blood;

    public HumanController humanInRange;


    private void Start()
    {
        blood = GetComponent<ParticleSystem>();
    }

    public void eatHuman()
    {
        blood.Play();
        Destroy(humanInRange.gameObject);
    }

    private void OnTriggerStay(Collider collision)
    {
        humanInRange = collision.gameObject.GetComponent<HumanController>();
    }

    private void OnTriggerExit(Collider collision)
    {
        humanInRange = collision.gameObject.GetComponent<HumanController>();
        if (humanInRange != null)
        {
            humanInRange = null;
        }
    }
}
