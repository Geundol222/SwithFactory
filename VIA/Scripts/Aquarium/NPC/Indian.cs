using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indian : NPC
{
    public IndianPanel indianPanel;

    public override void Interact(GameObject Obj)
    {
        indianPanel.Init(Obj, gameObject);

        base.Interact(Obj);
    }
}
