using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactable
{
    [SerializeField]
    private Door myDoor;
    public override void Interact()
    {
        myDoor.open = true;
        gameObject.SetActive(false);
    }
}
