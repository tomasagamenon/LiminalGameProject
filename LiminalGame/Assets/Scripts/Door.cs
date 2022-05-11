using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    public bool open;
    public override void Interact()
    {
        if (open)
            gameObject.SetActive(false);
    }
}
