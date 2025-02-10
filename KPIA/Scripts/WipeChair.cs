using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WipeChair : MonoBehaviour
{
    GamePlayManager _manager;
    LethalWeapon _weapon;
    OVRGrabbable _grabbable;

    private void Start()
    {
        _manager = FindObjectOfType<GamePlayManager>();
        _grabbable = GetComponent<OVRGrabbable>();
        _weapon = _manager.weapons[(int)_manager._weaponType];
    }

    private void OnTriggerExit(Collider other)
    {
        if (_manager._weaponType == WeaponType.Chair && other.gameObject.tag == "Mark")
        {
            _weapon.WipeChair();
        }
    }
}
