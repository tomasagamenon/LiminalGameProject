using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private List<Transform> pos1;
    [SerializeField]
    private List<Transform> pos2;

    private Transform myPos;

    private bool move;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(move)
            transform.Translate((pos2[pos1.IndexOf(myPos)].position - transform.position).normalized * Time.deltaTime * speed);
    }
    public void Event()
    {
        move = true;
        StartCoroutine(StartEvent());
    }

    IEnumerator StartEvent()
    {
        yield return new WaitUntil(() => (Vector3.Distance(pos2[pos1.IndexOf(myPos)].position, transform.position) < 1
            && Vector3.Distance(pos2[pos1.IndexOf(myPos)].position, transform.position) > -1));
        move = false;
        transform.position = pos1[Random.Range(0, pos1.Count - 1)].position;
    }
}
