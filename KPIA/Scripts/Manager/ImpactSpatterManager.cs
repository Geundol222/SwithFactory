using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactSpatterManager : MonoBehaviour
{
    public enum ImpactToolType { Arrow, RightAngle, Paper, Protractor, Sticker, Vernier, Foldable}

    public GameObject[] impactTools;
    int toolIndex = 0;

    private void OnEnable()
    {
        toolIndex = 0;
    }
}
