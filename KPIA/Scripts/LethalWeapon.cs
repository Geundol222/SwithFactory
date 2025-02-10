using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Knife, Paring_Knife, Bat, Hammer, Fist, Towel, Hand, Chair }

public class LethalWeapon : MonoBehaviour
{
    GamePlayManager _manager;
    Animator _victimAnim;
    BlendIntensityManager _blendIntensityManager;
    OVRGrabbable chairGrabbable;
    SwipeSphere _swipeSphere;
    public GameObject wipeChair;
    public GameObject normalWeapon;
    public GameObject bloodyWeapon;
    public GameObject paringKnifeCellingBlood;
    public Transform chairTransform;

    //bool isStabed = false;

    bool isRubbing = false;
    int rubbingCount = 0;
    int fruitKnifeStabCount = 0;

    private void Awake()
    {        
        _blendIntensityManager = FindAnyObjectByType<BlendIntensityManager>();
        _manager = FindObjectOfType<GamePlayManager>();
        _victimAnim = FindObjectOfType<Victim>(true).GetComponent<Animator>();
        _swipeSphere = FindObjectOfType<SwipeSphere>(true);
    }

    private void Update()
    {
        if (_manager._weaponType == WeaponType.Chair)
        {
            wipeChair.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnEnable()
    {
        if (_manager._weaponType == WeaponType.Paring_Knife)
            paringKnifeCellingBlood.SetActive(false);

        if (_manager._weaponType == WeaponType.Chair)
        {
            wipeChair.transform.position = chairTransform.position;
            wipeChair.transform.rotation = chairTransform.rotation;

            wipeChair.SetActive(true);

            chairGrabbable = wipeChair.GetComponent<OVRGrabbable>();

            Rigidbody chairRigid = chairGrabbable.GetComponent<Rigidbody>();

            if (chairRigid != null)
            {
                chairRigid.velocity = Vector3.zero;
                chairRigid.angularVelocity = Vector3.zero;
                chairRigid.constraints = RigidbodyConstraints.FreezeRotation;
                chairRigid.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            }
        }

        SetWeaponBloody(false);
        isRubbing = false;
        rubbingCount = 0;
        fruitKnifeStabCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckTrigger(other);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Mark"))
    //    {
    //        if (_manager._weaponType == WeaponType.Hand)
    //        {
    //            if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) != Vector3.zero)
    //            {
    //                Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
    //                Quaternion controllerRot = Quaternion.Euler(_manager.bloodyHand.transform.rotation.x, OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).y, _manager.bloodyHand.transform.rotation.z);
    //
    //                _manager.bloodyHand.transform.localPosition = new Vector3(controllerPos.x, _manager.bloodyHand.transform.position.x, controllerPos.z);
    //                _manager.bloodyHand.transform.localRotation = controllerRot;
    //
    //                float speed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y + OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).z;
    //                _blendIntensityManager.UpdateMaterialProperty(Time.deltaTime * speed, false);
    //            }
    //        }
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Mark") && isStabed)
        //{
        //    //히트
        //    _manager.Hit();
        //
        //    isStabed = false;
        //}

        if (other.CompareTag("Mark") )
        {
            if (_manager._weaponType == WeaponType.Towel && isRubbing)
            {
                rubbingCount++;

                if (isRubbing)
                {
                    if (rubbingCount == 1)
                    {
                        SetWeaponBloody(true);
                        _blendIntensityManager.UpdateMaterialProperty(0.7f, false);
                    }
                    else if (rubbingCount == 2)
                    {
                        _blendIntensityManager.UpdateMaterialProperty(0.86f, false);
                    }
                    else if (rubbingCount == 3)
                    {
                        _blendIntensityManager.UpdateMaterialProperty(1f, false);
                        _manager.HitEnd();
                    }
                }

                isRubbing = false;
            }
            //else if (_manager._weaponType == WeaponType.Hand)
            //{
            //    _manager.bloodyHand.SetActive(false);
            //    normalWeapon.gameObject.SetActive(true);
            //    isSwapping = false;
            //}
        }
    }

    void CheckTrigger(Collider other)
    {
        //print(other.name);

        //트리거 지점이 mark이면
        if (other.CompareTag("Mark"))
        {
            //print($"v : {OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude} , ang_v : {OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude} , end_v : {OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude + OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude / 2}");

            // 문지르기
            if (_manager._weaponType == WeaponType.Towel)
            {
                isRubbing = true;
            }
            else if (_manager._weaponType == WeaponType.Hand)
            {
                print(other.gameObject.name);

                if (other.gameObject == _swipeSphere.swipeSphere[0].gameObject)
                {
                    _swipeSphere.SwipeStart();
                    _swipeSphere.swipeStart = true;
                }
                else if (other.gameObject == _swipeSphere.swipeSphere[1].gameObject)
                {
                    _swipeSphere.SwipeEnd();
                }
            }
            //찌르기
            else if (_manager._weaponType == WeaponType.Knife)
            {
                if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y + OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).z > 0.25f && OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude / 2 < 2f)
                {
                    SetWeaponBloody(true);
                    _manager.Hit();
                }
            }
            // 과도 찌르기
            else if (_manager._weaponType == WeaponType.Paring_Knife)
            {
                if (!_manager.isSwingHit)
                {
                    if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y + OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).z < 0.2f)
                    {
                        // AudioSource Clip 할당
                        _manager.weapon_AS.clip = _manager.weapon_Clips[(int)WeaponType.Paring_Knife];

                        //흉기 소리 추가
                        _manager.weapon_AS.Play();
                        //으악 소리 추가
                        _manager.scream_AS.Play();
                        //피해자 신체 피칠갑
                        _manager.victim.SetBloody();
                        _manager.victim.BloodHitVFXPlay();
                        SetWeaponBloody(true);

                        if (fruitKnifeStabCount == 0)
                        {
                            fruitKnifeStabCount++;
                            _victimAnim.SetTrigger("Hit1");
                            

                            OVRInput.SetControllerVibration(10, 10, OVRInput.Controller.RTouch);
                            _manager.SwingCastOffHit();
                        }
                        else
                        {
                            fruitKnifeStabCount = 0;

                            _manager.Hit();
                        }
                    }
                }
                else
                {
                    if (fruitKnifeStabCount == 1)
                    {
                        paringKnifeCellingBlood.SetActive(true);
                    }

                    _manager.SwingCastOffStart();
                }
                
            }
            //주먹
            else if (_manager._weaponType == WeaponType.Fist)
            {
                if (Mathf.Abs(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).z) > 0.4f && OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude / 2 < 2f)
                {
                    _manager.Hit();
                }
            }
            // 휘두르기
            else
            {
                if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude + OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude / 2 > 5f)
                {
                    //히트
                    SetWeaponBloody(true);
                    _manager.Hit();
                }
            }
        }
    }

    public void WipeChair()
    {
        _blendIntensityManager.UpdateMaterialProperty(1f, false);

        if (chairGrabbable.grabbedBy != null)
        {
            chairGrabbable.grabbedBy.ForceRelease(chairGrabbable);
        }

        wipeChair.SetActive(false);
        _manager.HitEnd();
    }

    public void SetWeaponBloody(bool isBloody)
    {
        if (normalWeapon != null)
            normalWeapon.SetActive(!isBloody);

        if (bloodyWeapon != null)
            bloodyWeapon.SetActive(isBloody);
    }
}

