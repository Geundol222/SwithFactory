using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Otter : NPC
{
    public OtterPanel otterPanel;

    public override void Interact(GameObject Obj)
    {
        otterPanel.Init(Obj, gameObject);

        base.Interact(Obj);
    }
}
