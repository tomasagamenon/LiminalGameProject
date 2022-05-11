using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIFollow : MonoBehaviour
{
    [SerializeField] private float timeSeaching;
    private NavMeshAgent agent;
    private GameObject player;
    private Vector3 posToGo;
    public Commands command;
    public float searchArea;
    public List<Transform> nodes;
    public Animator animator;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<CharacterController>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        switch(command)
        {
            case Commands.Idle:
                animator.SetBool("Walk", false);
                agent.isStopped = true;
                break;
            case Commands.Walk:
                animator.SetBool("Walk", true);
                agent.isStopped = false;
                if (!agent.hasPath)
                    agent.SetDestination(nodes[Random.Range(0, nodes.Count)].position);
                break;
            case Commands.Follow:
                animator.SetBool("Walk", true);
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
                break;
            case Commands.Search:
                animator.SetBool("Walk", true);
                agent.isStopped = false;
                if (!agent.hasPath)
                {
                    NavMeshPath path = new NavMeshPath();
                    var destination = posToGo + new Vector3(Random.Range(-searchArea, searchArea),
                        transform.position.y, Random.Range(-searchArea, searchArea));
                    agent.CalculatePath(destination, path);
                    if (path.status != NavMeshPathStatus.PathPartial)
                    {
                        agent.SetDestination(destination);
                    }
                }
                break;
            case Commands.Nothing:
                agent.isStopped = false;
                break;
        }
        if (GetComponent<AIDetect>().IsInSight(player.transform))
        {
            StopAllCoroutines();
            StartCoroutine(Follow());
        }
    }

    IEnumerator Follow()
    {
        FindObjectOfType<PlayerSanity>().pursuedBool = true;
        command = Commands.Follow;
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, player.transform.position) 
        || FindObjectOfType<StarterAssets.FirstPersonController>().hide);
        command = Commands.Nothing;
        if (!FindObjectOfType<StarterAssets.FirstPersonController>().hide)
            yield return new WaitForSeconds(1);
        posToGo = player.transform.position;
        agent.SetDestination(posToGo);
        yield return new WaitUntil(() => (Vector3.Distance(posToGo, transform.position) < 1
            && Vector3.Distance(posToGo, transform.position) > -1) || FindObjectOfType<StarterAssets.FirstPersonController>().hide);
        command = Commands.Idle;
        yield return new WaitForSeconds(0.5f);
        command = Commands.Search;
        yield return new WaitForSeconds(timeSeaching);
        command = Commands.Idle;
        yield return new WaitForSeconds(2);
        FindObjectOfType<PlayerSanity>().pursuedBool = false;
        command = Commands.Walk;
    }
}

public enum Commands
{
    Idle,
    Walk,
    Follow,
    Search,
    Nothing,
}