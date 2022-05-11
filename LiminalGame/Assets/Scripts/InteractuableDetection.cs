using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractuableDetection : MonoBehaviour
{
    public GameObject interact;
    public float range;
    private Transform cameraPos;
    private bool _uiActive;
    private void Start()
    {
        cameraPos = GameObject.FindGameObjectWithTag("CinemachineTarget").transform;
    }
    private void Update()
    {
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out RaycastHit hit, range))
        {
            if (hit.collider.GetComponent<Interactable>())
            {
                ChangeUI(true);
                _uiActive = true;
            }
            else
                ChangeUI(false);
        }
        else if (_uiActive == true)
        {
            ChangeUI(false);
        }
    }
    void ChangeUI(bool on)
    {
        if (on)
        {
            interact.SetActive(true);
        }
        else
        {
            interact.SetActive(false);
            _uiActive = false;
        }
    }
}
