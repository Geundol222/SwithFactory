using CurvedUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
//using UnityEngine;
//using Unity.Profiling;

public enum BloodEnum
{
    /// <summary>
    /// 1-1.���� ���� ����
    /// </summary>
    Spurt,                  //1.���� ���� ����
    /// <summary>
    /// 1-2.�ֵθ� ��Ż ����
    /// </summary>
    Swing_Cast_Off,         //2.�ֵθ� ��Ż ����
    /// <summary>
    /// 1-3.���� ���� ����
    /// </summary>
    Drip_Trail,             //3.���� ���� ����

    /// <summary>
    /// 2-1.��� ��� ����
    /// </summary>
    Impact_Spatter,         //1.��� ��� ����
    /// <summary>
    /// 2-2.ȣ�� ����
    /// </summary>
    Expectorate,            //2.ȣ�� ����
    /// <summary>
    /// 2-3.���� ����
    /// </summary>
    Drip,                   //3.���� ����
    /// <summary>
    /// 2-4.���� ��Ż ����
    /// </summary>
    Cessation_Cast_Off,     //4.���� ��Ż ����

    /// <summary>
    /// 3-1.���� ����
    /// </summary>
    Blood_Into_Blood,       //1.���� ����
    /// <summary>
    /// 3-2.�ٷ� ����/�ٷ� ���� ����
    /// </summary>
    Gush_Splash,            //2.�ٷ� ����/�ٷ� ���� ����
    /// <summary>
    /// 3-3.������ ����
    /// </summary>
    Smear,                  //3-1.������ ����
    /// <summary>
    /// 3-4 ���� ����
    /// </summary>
    Wipe,                   //3-2.���� ����
    /// <summary>
    /// 3-5 ���� ����
    /// </summary>
    Swipe,                  //3-3.���� ����

    /// <summary>
    /// 4-1.���� ���� ����
    /// </summary>
    Pattern_Transfer,        //1.���� ���� ����
    /// <summary>
    /// 4-2.��� ����
    /// </summary>
    Saturation,               //2.��� ����
    /// <summary>
    /// 4-3.�帧 ����
    /// </summary>
    Flow,                      //3.�帧 ����
    /// <summary>
    /// 4-4.���� ����
    /// </summary>
    Pool,                      //4.���� ����

    /// <summary>
    /// 5-1 ��������
    /// </summary>
    Skeletonized_Stain,          //1.���� ����
    /// <summary>
    /// 5-2 ������
    /// </summary>
    Void,                       //2.������
    /// <summary>
    /// 5-3 ��
    /// </summary>
    Dilution,                   //3.��

    /// <summary>
    /// ��ü ���� �м�
    /// </summary>
    WholeBloodstainAnalysis     //��ü ���� �м�
}

public enum SmearType { Smear, Wipe, Swipe, None }

//�ѹ��� ���� ������Ʈ�� �ν����� �󿡼� �����ϰ� �Ѱ� ���� ���Ͽ� ������ Ŭ����
[System.Serializable]
public class GameObjectMultiArray
{
    public GameObject[] objects;
    //�ѹ��� Ű��
    public void Active()
    {
        foreach (var obj in objects)
        {
            obj.SetActive(true);
        }
    }
    //�ѹ��� ����
    public void Deactive()
    {
        foreach (var obj in objects)
        {
            obj.SetActive(false);
        }
    }
}

public class GamePlayManager : MonoBehaviour
{
    [Header("General")]
    public GameObject curvedUILaserPointer;                 //������ ������ ������Ʈ
    public GameObject bloodStainSimulation;                 //����м� �ùķ��̼ǿ� �θ������Ʈ
    public GameObject siteAnalysis;                         //����м��� �θ������Ʈ
    public PDAManager _pdaManager;          
    public Player _player;
    Mark[] uiMarks;                                         //UIMark ĳ�̿� �迭 - Clear()�� ȣ��ɶ����� ��� UIMark�� ã�� ������ ���Ƿ� �������� ���
    bool isLaser;                                           //������ on ����

    [Header("BloodPlay")]
    public BloodEnum _bloodEnum;                            //���� �÷������� ����м�
    public SmearType _smearType = SmearType.None;           //������ ���� ����
    public Transform[] bloodPlayerPos;                      //����м� �������� ��ġ/ȸ���� Ʈ������
    public Transform[] victimPos;                           //������ Ʈ������
    public Transform bloodIntoBloodTransform;               //�������� �ǰ� �������Ǵ� UIMark��ġ
    public Transform dripBloodTransform;                    //�������� �ǰ� �������Ǵ� UIMark��ġ
    public GameObjectMultiArray[] bloodblood_stand;         //�ǰ� ���� on �Ǿ�� �� ����м� �𵨸�
    public GameObjectMultiArray[] bloodblood_falldown;      //�ǰ��� �������� ���� on �Ǿ�� �� ����м� �𵨸�
    public GameObject[] smearTypeArray;                     //������ ���� �迭
    public GameObject bloodyChair;                          //�帧����� ����
    public Victim victim;                                   //������
    public Animator victimAnim;                             //������ animator
    public BlendIntensityManager _blendIntensityManager;    //�� ���� �� Blend Manager
    public GameObject swipeSphere;
    public GameObject cessationCastOffChara;
    public GameObject swingCastOffCol;
    public GameObject swingCastOffMark;
    public bool isSwingHit = false;
    public bool isSimulating = false;
    public ParticleScript _particle;
    public BloodBlinkManager _blinkManager;
    public int arrowCount = 0;

    [Header("Weapon")]
    public LethalWeapon[] weapons;                          //����Set
    public WeaponType _weaponType;                          //���� ����

    //[Header("FieldPlay")]
    //public GameObject field_victim;
    //public Transform fieldPlayerPos;
    //public GameObject field_Marks;

    [Header("Sound")]
    public AudioSource scream_AS;                           //���� AudioSource
    public AudioSource weapon_AS;                           //����� AudioSource
    public AudioClip[] weapon_Clips;                        //���⺰ Clip (������ �ϴ� �߱�����̸� ����)


    [Header("Tool")]
    public GameObject[] tools;
    public ToolType _toolType;
    public Camcoder camcoder;

    [Header("ImpactSpatter")]
    public Text[] spatterTexts;    
    public GameObject phone;
    public GameObject[] arrows;
    public GameObject[] ropes;
    public GameObject[] spatterBloods;

    private void Awake()
    {
        UnityEngine.Rendering.TextureXR.maxViews = 2;
        SettingAnalysis(true);
        swingCastOffCol.SetActive(false);
        _player = FindObjectOfType<Player>();
        //_pdaManager = FindObjectOfType<PDAManager>();
        uiMarks = FindObjectsOfType<Mark>(true);
    }

    private void Start()
    {
        //SetLaser(true);
        //Application.targetFrameRate = 30;
    }

    //private void Update()
    //{
    //    VibrateTest();
    //    TESTTEST();
    //}
    public void Clear()
    {
        //�ʱ� �÷��� �ʱ�ȭ ���� �ڵ�
        //�ʿ信 ���� ���� �ٶ�

        SettingAnalysis(true);
        DisableTool();

        _player.isMove = true;
        _smearType = SmearType.None;

        foreach (var obj in bloodblood_stand)
        {
            if (obj == null) continue;
            obj.Deactive();
        }
        foreach (var obj in bloodblood_falldown)
        {
            if (obj == null) continue;
            obj.Deactive();
        }

        AllUIMarkDeactive();

        foreach (GameObject smearObj in smearTypeArray)
        {
            smearObj.SetActive(false);
        }

        foreach (LethalWeapon weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }

        foreach (GameObject blood in _particle.bloodList)
        {
            Destroy(blood);
        }
        _particle.bloodList.Clear();
        victim.Clear();
        _player.DisableBloodIntoBloodUI();
        _blendIntensityManager.ClearMaterialProperty();
        cessationCastOffChara.SetActive(false);
        bloodyChair.SetActive(false);
        swipeSphere.SetActive(false);
    }

    public void SettingAnalysis(bool isBloodStain = true)
    {
        bloodStainSimulation.SetActive(isBloodStain);
        siteAnalysis.SetActive(!isBloodStain);
    }

    #region UI
    public bool GetLaser()
    {
        return isLaser;
    }

    public void SetLaser(bool isOn)
    {
        //������ ������ on �����ִ� �Լ�        
        isLaser = isOn;
        curvedUILaserPointer.SetActive(isOn);
    }

    public void BloodPlay(int index)
    {
        //����м� �÷����Ͻðڽ��ϱ� -> �� ������ ��

        isSimulating = true;

        Clear(); //�ʱ�ȭ
        //pda off
        _pdaManager.gameObject.SetActive(false);
        SetLaser(false); //������ off
        
        //���� �÷������� ����м� ���� ����
        victim._bloodEnum = (BloodEnum)index;  
        _bloodEnum = (BloodEnum)index;

        //��ġ �ʱ�ȭ
        SetPlayerBloodPos();

        switch ((BloodEnum)index)
        {
            case BloodEnum.Spurt:
                {
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Swing_Cast_Off:
                {
                    SettingBloodType(WeaponType.Paring_Knife);
                    break;
                }
            case BloodEnum.Drip_Trail:
                {
                    SettingBloodType(WeaponType.Fist);
                    break;
                }
            case BloodEnum.Impact_Spatter:
                {
                    victim.SetBloody();
                    SettingBloodType(WeaponType.Hammer);
                    break;
                }
            case BloodEnum.Expectorate:
                {
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Drip:
                {
                    SettingBloodType(WeaponType.Fist);
                    break;
                }
            case BloodEnum.Cessation_Cast_Off:
                {
                    SetCessationCastOff();
                    break;
                }
            case BloodEnum.Blood_Into_Blood:
                {
                    // �ǰ����� ǥ��
                    victim.BloodHitMarkActive();

                    // ������ ��ġ
                    victim.transform.position = victimPos[(int)_bloodEnum].position;
                    victim.transform.rotation = victimPos[(int)_bloodEnum].rotation;
                    victim.gameObject.SetActive(true);

                    victim.SetAnim();
                    victim.bloodDripParticle.transform.position = victim.hitMarks[(int)_bloodEnum].transform.position;

                    victim.BloodDripVFXPlay();
                    break;
                }
            case BloodEnum.Gush_Splash:
                {
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Smear:
                {
                    _smearType = SmearType.Smear;
                    smearTypeArray[(int)_smearType].SetActive(true);
                    SettingBloodType(WeaponType.Towel, false);
                    break;
                }
            case BloodEnum.Wipe:
                {
                    _smearType = SmearType.Wipe;
                    smearTypeArray[(int)_smearType].SetActive(true);
                    SettingBloodType(WeaponType.Chair, false);
                    break;
                }
            case BloodEnum.Swipe:
                {
                    _smearType = SmearType.Swipe;
                    swipeSphere.SetActive(true);
                    smearTypeArray[(int)_smearType].SetActive(false);
                    SettingBloodType(WeaponType.Hand, false);
                    break;
                }
            case BloodEnum.Pattern_Transfer:
                {
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Saturation:
                {
                    SettingBloodType(WeaponType.Hammer);
                    break;
                }
            case BloodEnum.Flow:
                {
                    bloodyChair.SetActive(true);
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Pool:
                {
                    SettingBloodType(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Skeletonized_Stain:
                {
                    ETCBloodSetting(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Void:
                {
                    ETCBloodSetting(WeaponType.Knife);
                    break;
                }
            case BloodEnum.Dilution:
                {
                    ETCBloodSetting(WeaponType.Knife);
                    break;
                }
            case BloodEnum.WholeBloodstainAnalysis:
                {
                    foreach(var obj in bloodblood_stand)
                    {
                        if (obj == null) continue;
                        obj.Active();
                    }
                    foreach (var obj in bloodblood_falldown)
                    {
                        if (obj == null) continue;
                        obj.Active();
                    }

                    for (int i = 0; i < smearTypeArray.Length; i++)
                    {
                        smearTypeArray[i].SetActive(true);
                        if (i < smearTypeArray.Length - 1)
                            smearTypeArray[i].transform.GetChild(0).gameObject.SetActive(false);
                    }

                    _blendIntensityManager.UpdateMaterialProperty(1f, true);
                    _blendIntensityManager.UpdateMaterialProperty(1f, false);

                    SetLaser(true);
                    _player.isMove = true;
                    break;
                }
        }
    }

    public void SwingCastOffHit()
    {
        victim.BloodHitMarkDeactive();
        swingCastOffCol.SetActive(true);
        swingCastOffMark.SetActive(true);
        isSwingHit = true;
    }

    public void SwingCastOffStart()
    {
        victim.BloodHitMarkActive();
        swingCastOffCol.SetActive(false);
        swingCastOffMark.SetActive(false);
        isSwingHit = false;
    }

    public void ETCBloodSetting(WeaponType weapon)
    {
        _weaponType = weapon;
        HitEnd();
    }

    public void SetCessationCastOff()
    {
        cessationCastOffChara.SetActive(true);
    }

    /// <summary>
    /// ��Ȳ ����
    /// </summary>
    /// <param name="weapon"> ����ϴ� Weapon Type </param>
    /// <param name="needVictim"> �����ڰ� �ʿ����� ���� (Default = true)</param>
    public void SettingBloodType(WeaponType weapon, bool needVictim = true)
    {
        if (_smearType == SmearType.Smear || _smearType == SmearType.Wipe)
            smearTypeArray[(int)_smearType].transform.GetChild(0).gameObject.SetActive(true);

        // ���ǥ��
        _weaponType = weapon;
        weapons[(int)weapon].gameObject.SetActive(true);

        if (_weaponType == WeaponType.Chair)
        {
            _player.isMove = true;
        }

        // ���� �����ڰ� �ʿ��ϸ� (Default)
        if (needVictim)
        {
            // �ǰ����� ǥ��
            victim.BloodHitMarkActive();

            // ������ ��ġ
            victim.transform.position = victimPos[(int)_bloodEnum].position;
            victim.transform.rotation = victimPos[(int)_bloodEnum].rotation;
            victim.gameObject.SetActive(true);

            victim.SetAnim();

            victim.hitVFX.transform.position = victim.hitMarks[(int)_bloodEnum].transform.position;

            if (_bloodEnum == BloodEnum.Drip || _bloodEnum == BloodEnum.Drip_Trail || _bloodEnum == BloodEnum.Blood_Into_Blood)
                victim.bloodDripParticle.transform.position = victim.hitMarks[(int)_bloodEnum].transform.position;
        }
    }

    public void AllUIMarkDeactive()
    {
        //�� �� �����ϴ� ��� ui-mark ���� �г� off
        foreach (Mark mark in uiMarks)
        {
            mark.UIOff();
        }
    }

    public bool isArrow;
    public bool isRope;

    public void SplatterArrow()
    {
        for (int i = 0; i < 8; i++)
        {
            arrows[i].SetActive(false);
            spatterTexts[i].gameObject.SetActive(false);
            ropes[i].SetActive(false);
        }
        _pdaManager.gameObject.SetActive(false);
        phone.SetActive(false);
        isArrow = true;
        isRope = false;
    }

    public void SplatterPhone()
    {
        for (int i = 0; i < 8; i++)
        {
            arrows[i].SetActive(true);
            spatterTexts[i].gameObject.SetActive(false);
            ropes[i].SetActive(false);
        }
        _pdaManager.gameObject.SetActive(false);
        phone.SetActive(true);
        isArrow = false;
        isRope = false;
    }

    public void SplatterRope()
    {
        for (int i = 0; i < 8; i++)
        {
            arrows[i].SetActive(true);
            //spatterTexts[i].gameObject.SetActive(true);
            ropes[i].SetActive(false);
        }
        _pdaManager.gameObject.SetActive(false);
        phone.SetActive(false);
        isArrow = false;
        isRope = true;
    }

    #endregion

    #region Blood
    public void SetPlayerBloodPos()
    {
        _player.transform.position = bloodPlayerPos[(int)_bloodEnum].position;
        _player.transform.rotation = bloodPlayerPos[(int)_bloodEnum].rotation;
    }

    #region Victim
    public void Hit()
    {
        //���� ��ƼŬ ������
        victim.BloodHitVFXPlay();

        victim.BloodHitMarkDeactive();                                              //�ǰ����� ����

        //�ǰݽ�
        OVRInput.SetControllerVibration(10, 10, OVRInput.Controller.RTouch);        //����        
        bloodblood_stand[(int)_bloodEnum].Active();                                 //���� Ƣ�� ���� on (Object)

        if (_bloodEnum == BloodEnum.Pattern_Transfer)
        {
            weapons[(int)_weaponType].gameObject.SetActive(false);
        }

        if (_bloodEnum == BloodEnum.Drip || _bloodEnum == BloodEnum.Drip_Trail)
            victim.BloodDripVFXPlay();

        HitType(_bloodEnum);
    }

    public void HitType(BloodEnum bloodType)
    {
        // �ֵθ� ��Ż ������ ���� ó��
        if (bloodType != BloodEnum.Swing_Cast_Off)
        {
            // AudioSource Clip �Ҵ�
            weapon_AS.clip = weapon_Clips[(int)_weaponType];

            //���� �Ҹ� �߰�(�ٲ��ʿ��?)
            scream_AS.Play();
            //��� �Ҹ� �߰�
            weapon_AS.Play();
            //������ ��ü ��ĥ��
            victim.SetBloody();
        }

        switch (bloodType)
        {
            case BloodEnum.Swing_Cast_Off:
                {
                    victimAnim.SetTrigger("Hit2");
                    break;
                }
            case BloodEnum.Saturation:
                {
                    // �����ִٰ� ���� �ִϸ��̼�
                    victimAnim.StopPlayback();
                    StartCoroutine(SettingBloodyPillow());
                    break;
                }
            case BloodEnum.Flow:
                {
                    HitEnd();
                    break;
                }
            default:
                victimAnim.SetTrigger("Hit1");
                break;
        }
    }

    public void HitEnd()
    {
        //�Ӹ��� �ٴڿ� ���� �������� ������ �÷��� 

        if (_bloodEnum == BloodEnum.Blood_Into_Blood)
        {
            victim.bloodDripParticle.Stop();

            //�������� �� ������ ���� ����
            bloodblood_falldown[(int)_bloodEnum].Active();
            //������ On
            SetLaser(true);
            //�м� ui��ũ ǥ��
            //victim.BloodUIMarkActive();

            _player.isMove = true;
            isSimulating = false;
        }
        else if (_bloodEnum == BloodEnum.Impact_Spatter)
        {
            SettingImpactSpatter();
        }
        else
        {
            if (_smearType == SmearType.Smear || _smearType == SmearType.Wipe)
            {
                smearTypeArray[(int)_smearType].transform.GetChild(0).gameObject.SetActive(false);
            }

            if (_bloodEnum == BloodEnum.Flow)
            {
                bloodyChair.transform.GetChild(0).gameObject.SetActive(false);
                bloodyChair.transform.GetChild(1).gameObject.SetActive(true);
            }

            //��� ����
            weapons[(int)_weaponType].gameObject.SetActive(false);
            //�������� �� ������ ���� ����
            bloodblood_falldown[(int)_bloodEnum].Active();
            //������ On
            SetLaser(true);
            //�м� ui��ũ ǥ��
            //victim.BloodUIMarkActive();

            if (_bloodEnum == BloodEnum.Pool || _bloodEnum == BloodEnum.Gush_Splash)
                victim.gameObject.SetActive(false);

            _player.isMove = true;
            isSimulating = false;
        }
    }

    public void SettingImpactSpatter()
    {
        //��� ����
        weapons[(int)_weaponType].gameObject.SetActive(false);
        //�������� �� ������ ���� ����
        bloodblood_falldown[(int)_bloodEnum].Active();

        _blinkManager.SetBlink(true);

        //�� �ѱ�
        for (int i = 0; i < spatterBloods.Length; i++)
        {
            spatterBloods[i].SetActive(true);
        }
        //�޴��� �ѱ�

        //������ On
        SetLaser(true);
        _player.isMove = true;
    }

    ///// <summary>
    ///// ���⸦ �ֵθ� �� 3�� ���� ��ٸ��� �ڷ�ƾ, ���� �ڷ�ƾ Ȱ��ȭ �� ���� �����ϰ� �����ϰ�� �ð� �ʱ�ȭ
    ///// </summary>
    ///// <returns></returns>
    //IEnumerator HoldHandCounter()
    //{
    //    float time = 3f;

    //    while (time > 0f)
    //    {
    //        if (OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch).magnitude / 2 < 0.5f)
    //            time -= Time.deltaTime;
    //        else
    //            time = 3f;

    //        yield return null;
    //    }

    //    // �� ������ ������� UI Disable
    //    _player.DisableBloodIntoBloodUI();

    //    //��� ����
    //    weapons[(int)_weaponType].gameObject.SetActive(false);
        
    //    yield break;
    //}

    IEnumerator SettingBloodyPillow()
    {
        float intensity = 0f;

        while (intensity < 1f)
        {
            intensity += Time.deltaTime * 0.4f;

            _blendIntensityManager.UpdateMaterialProperty(intensity, true);
            yield return null;
        }

        HitEnd();
    }
    #endregion

    #endregion

    #region Tool
    public void ActiveTool(int index)
    {
        DisableTool();
        _toolType = (ToolType)index;

        SetLaser(false);
        _pdaManager.gameObject.SetActive(false);

        switch (_toolType)
        {
            case ToolType.Arrow:
                SetLaser(true);
                SplatterArrow();
                break;
            case ToolType.Phone:
                SetLaser(true);
                SplatterPhone();
                break;
            case ToolType.Rope:
                SetLaser(true);
                SplatterRope();
                break;
            default:
                tools[index].GetComponent<Tool>().SetActive(true);
                break;
        }
    }

    public void DisableTool()
    {
        phone.SetActive(false);

        if (!_pdaManager.gameObject.activeSelf)
        {
            _pdaManager.gameObject.SetActive(true);
            SetLaser(true);
        }

        foreach (GameObject tool in tools)
        {
            if (tool == null)
                continue;

            tool.SetActive(false);
        }
    }
    #endregion

    #region Field
    //public void FieldPlay(int index)
    //{

    //    Clear();
    //    field_victim.SetActive(true);
    //    field_Marks.SetActive(true);
    //    _pdaManager.gameObject.SetActive(false);
    //    SetLaser(false);


    //    SetPlayerFieldPos();
    //}
    //public void SetPlayerFieldPos()
    //{
    //    _player.transform.position = fieldPlayerPos.position;
    //    _player.transform.rotation = fieldPlayerPos.rotation;
    //}
    #endregion

    #region Sound
    public void SetSfxVolume(float f)
    {
        //ȿ���� ���õ� AudioSource�� ���� ���⿡ ����
        scream_AS.volume = f;
        weapon_AS.volume = f;
    }
    #endregion

    #region test
    void VibrateTest()
    {

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            VibrateController(OVRInput.Controller.LTouch);

        }
        if ( OVRInput.GetDown(OVRInput.RawButton.B))
        {

            VibrateController(OVRInput.Controller.RTouch);

        }
    }

    public float vibrationDuration = 0.3f;
    public float vibrationIntensity = 0.5f;
    OVRInput.Controller controller;

    void VibrateController(OVRInput.Controller controller)
    {
        // Vibrate the controller
        OVRInput.SetControllerVibration(10, 10, controller);
        print("SetControllerVibration!");
        // Stop the vibration after the specified duration
        //Invoke("StopVibration", vibrationDuration);

    }

    //void StopVibration()
    ////{
    ////    // Stop the vibration
    ////    print(" Stop the vibration");
    ////    OVRInput.SetControllerVibration(0, 0, controller);
    //}

    public void TESTTEST()
    {

#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
        //if (Input.GetKeyDown(KeyCode.Space))
        //{

        //    Hit();
        //}

#endif

    }

    #endregion
}

