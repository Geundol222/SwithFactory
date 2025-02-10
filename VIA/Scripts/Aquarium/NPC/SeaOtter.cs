using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaOtter : NPC
{
    public SeaOtterPanel seaOtterPanel;

    public override void Interact(GameObject Obj)
    {
        seaOtterPanel.Init(Obj, gameObject);

        base.Interact(Obj);
    }
}
