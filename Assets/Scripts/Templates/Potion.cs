using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifact/Potion")]
public class Potion : Artifact
{
    public override bool activate()
    {
        return GameObject.FindWithTag("GameManager").GetComponent<PlayerAttributes_scr>().heal();
    }

    public override bool deactivate()
    {

        return false;
    }
}
