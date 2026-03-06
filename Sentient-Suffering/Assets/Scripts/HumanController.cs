using UnityEngine;
using UnityEngine.AI;

public class HumanController : MonoBehaviour
{
    public NavMeshAgent controller;
    public Transform destination;
    public bool isStunned;

    [SerializeField]
    private float hunger = 0;

    [Range(0, 100)]
    [Tooltip("When the human becomes hungry")]
    public float eatingThreshhold = 60;

    [Tooltip("How fast the human becomes hungry")]
    [Range(3, 5)]
    public float gluttony = 4;

    public bool wantsFood = false;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<NavMeshAgent>();
        }
    }

    void Start()
    {
        controller.SetDestination(destination.position);
        hunger = 0;
    }

    private void Update()
    {
        hunger += gluttony * Time.deltaTime * 0.08f; //adds 8% off gluttony eg. 0.32, with a threshhold of 60 it'd take 187.5 seconds

        if (hunger >= eatingThreshhold && wantsFood != true)
        {
            wantsFood = true;
        }
    }
}
