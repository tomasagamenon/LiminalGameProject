using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDetect : MonoBehaviour
{
    public float range;
    public float angle;
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsInSight(Transform target)
    {
        if (FindObjectOfType<StarterAssets.FirstPersonController>().hide)
            return false;
        Vector3 diff = (target.position - transform.position);
        //A--->B
        //B-A
        float distance = diff.magnitude;
        if (distance > range) return false;
        if (Vector3.Angle(transform.forward, diff) > angle / 2) return false;
        if (Physics.Raycast(transform.position, diff.normalized, distance, mask)) return false;
        return true;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * range);
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, angle / 2, 0) * transform.forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -angle / 2, 0) * transform.forward * range);
    }
}
