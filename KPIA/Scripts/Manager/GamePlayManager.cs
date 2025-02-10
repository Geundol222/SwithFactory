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
    /// 1-1.선상 분출 혈흔
    /// </summary>
    Spurt,                  //1.선상 분출 혈흔
    /// <summary>
    /// 1-2.휘두름 이탈 혈흔
    /// </summary>
    Swing_Cast_Off,         //2.휘두름 이탈 혈흔
    /// <summary>
    /// 1-3.낙하 연결 혈흔
    /// </summary>
    Drip_Trail,             //3.낙하 연결 혈흔

    /// <summary>
    /// 2-1.충격 비산 혈흔
    /// </summary>
    Impact_Spatter,         //1.충격 비산 혈흔
    /// <summary>
    /// 2-2.호기 혈흔
    /// </summary>
    Expectorate,            //2.호기 혈흔
    /// <summary>
    /// 2-3.낙하 혈흔
    /// </summary>
    Drip,                   //3.낙하 혈흔
    /// <summary>
    /// 2-4.정지 이탈 혈흔
    /// </summary>
    Cessation_Cast_Off,     //4.정지 이탈 혈흔

    /// <summary>
    /// 3-1.누적 혈흔
    /// </summary>
    Blood_Into_Blood,       //1.누적 혈흔
    /// <summary>
    /// 3-2.다량 분출/다량 유출 혈흔
    /// </summary>
    Gush_Splash,            //2.다량 분출/다량 유출 혈흔
    /// <summary>
    /// 3-3.문지름 혈흔
    /// </summary>
    Smear,                  //3-1.문지름 혈흔
    /// <summary>
    /// 3-4 닦인 혈흔
    /// </summary>
    Wipe,                   //3-2.닦인 혈흔
    /// <summary>
    /// 3-5 묻힌 혈흔
    /// </summary>
    Swipe,                  //3-3.묻힌 혈흔

    /// <summary>
    /// 4-1.형태 전이 혈흔
    /// </summary>
    Pattern_Transfer,        //1.형태 전이 혈흔
    /// <summary>
    /// 4-2.흡수 혈흔
    /// </summary>
    Saturation,               //2.흡수 혈흔
    /// <summary>
    /// 4-3.흐름 혈흔
    /// </summary>
    Flow,                      //3.흐름 혈흔
    /// <summary>
    /// 4-4.고인 혈흔
    /// </summary>
    Pool,                      //4.고인 혈흔

    /// <summary>
    /// 5-1 윤곽혈흔
    /// </summary>
    Skeletonized_Stain,          //1.윤곽 혈흔
    /// <summary>
    /// 5-2 공간흔
    /// </summary>
    Void,                       //2.공간흔
    /// <summary>
    /// 5-3 희석
    /// </summary>
    Dilution,                   //3.희석

    /// <summary>
    /// 전체 혈흔 분석
    /// </summary>
    WholeBloodstainAnalysis     //전체 혈흔 분석
}

public enum SmearType { Smear, Wipe, Swipe, None }

//한번에 많은 오브젝트를 인스펙터 상에서 연결하고 켜고 끄기 위하여 만들어둔 클래스
[System.Serializable]
public class GameObjectMultiArray
{
    public GameObject[] objects;
    //한번에 키기
    public void Active()
    {
        foreach (var obj in objects)
        {
            obj.SetActive(true);
        }
    }
    //한번에 끄기
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
    public GameObject curvedUILaserPointer;                 //레이저 포인터 오브젝트
    public GameObject bloodStainSimulation;                 //혈흔분석 시뮬레이션용 부모오브젝트
    public GameObject siteAnalysis;                         //현장분석용 부모오브젝트
    public PDAManager _pdaManager;          
    public Player _player;
    Mark[] uiMarks;                                         //UIMark 캐싱용 배열 - Clear()가 호출될때마다 모든 UIMark를 찾는 과정이 들어가므로 성능저하 우려
    bool isLaser;                                           //레이저 on 여부

    [Header("BloodPlay")]
    public BloodEnum _bloodEnum;                            //현재 플레이중인 혈흔분석
    public SmearType _smearType = SmearType.None;           //문지름 혈흔 종류
    public Transform[] bloodPlayerPos;                      //혈흔분석 시작지점 위치/회전값 트랜스폼
    public Transform[] victimPos;                           //피해자 트랜스폼
    public Transform bloodIntoBloodTransform;               //떨어지는 피가 렌더링되는 UIMark위치
    public Transform dripBloodTransform;                    //떨어지는 피가 렌더링되는 UIMark위치
    public GameObjectMultiArray[] bloodblood_stand;         //피격 직후 on 되어야 할 혈흔분석 모델링
    public GameObjectMultiArray[] bloodblood_falldown;      //피격후 쓰러지고 나서 on 되어야 할 혈흔분석 모델링
    public GameObject[] smearTypeArray;                     //문지름 혈흔 배열
    public GameObject bloodyChair;                          //흐름혈흔용 의자
    public Victim victim;                                   //피해자
    public Animator victimAnim;                             //피해자 animator
    public BlendIntensityManager _blendIntensityManager;    //피 번짐 용 Blend Manager
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
    public LethalWeapon[] weapons;                          //무기Set
    public WeaponType _weaponType;                          //사용될 무기

    //[Header("FieldPlay")]
    //public GameObject field_victim;
    //public Transform fieldPlayerPos;
    //public GameObject field_Marks;

    [Header("Sound")]
    public AudioSource scream_AS;                           //비명용 AudioSource
    public AudioSource weapon_AS;                           //무기용 AudioSource
    public AudioClip[] weapon_Clips;                        //무기별 Clip (지금은 일단 야구방망이만 있음)


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
        //초기 플레이 초기화 관련 코드
        //필요에 따라서 갱신 바람

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
        //레이저 포인터 on 시켜주는 함수        
        isLaser = isOn;
        curvedUILaserPointer.SetActive(isOn);
    }

    public void BloodPlay(int index)
    {
        //혈흔분석 플레이하시겠습니까 -> 예 눌렀을 때

        isSimulating = true;

        Clear(); //초기화
        //pda off
        _pdaManager.gameObject.SetActive(false);
        SetLaser(false); //레이저 off
        
        //현재 플레이중인 혈흔분석 종류 저장
        victim._bloodEnum = (BloodEnum)index;  
        _bloodEnum = (BloodEnum)index;

        //위치 초기화
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
                    // 피격지점 표시
                    victim.BloodHitMarkActive();

                    // 피해자 배치
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
    /// 상황 세팅
    /// </summary>
    /// <param name="weapon"> 사용하는 Weapon Type </param>
    /// <param name="needVictim"> 피해자가 필요한지 여부 (Default = true)</param>
    public void SettingBloodType(WeaponType weapon, bool needVictim = true)
    {
        if (_smearType == SmearType.Smear || _smearType == SmearType.Wipe)
            smearTypeArray[(int)_smearType].transform.GetChild(0).gameObject.SetActive(true);

        // 흉기표시
        _weaponType = weapon;
        weapons[(int)weapon].gameObject.SetActive(true);

        if (_weaponType == WeaponType.Chair)
        {
            _player.isMove = true;
        }

        // 만약 피해자가 필요하면 (Default)
        if (needVictim)
        {
            // 피격지점 표시
            victim.BloodHitMarkActive();

            // 피해자 배치
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
        //씬 상에 존재하는 모든 ui-mark 전부 패널 off
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
        //혈흔 파티클 나오기
        victim.BloodHitVFXPlay();

        victim.BloodHitMarkDeactive();                                              //피격지점 끄기

        //피격시
        OVRInput.SetControllerVibration(10, 10, OVRInput.Controller.RTouch);        //진동        
        bloodblood_stand[(int)_bloodEnum].Active();                                 //벽에 튀는 혈흔 on (Object)

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
        // 휘두름 이탈 혈흔은 따로 처리
        if (bloodType != BloodEnum.Swing_Cast_Off)
        {
            // AudioSource Clip 할당
            weapon_AS.clip = weapon_Clips[(int)_weaponType];

            //으악 소리 추가(바꿀필요는?)
            scream_AS.Play();
            //흉기 소리 추가
            weapon_AS.Play();
            //피해자 신체 피칠갑
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
                    // 누워있다가 죽은 애니메이션
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
        //머리가 바닥에 닿은 쓰러지는 점에서 플레이 

        if (_bloodEnum == BloodEnum.Blood_Into_Blood)
        {
            victim.bloodDripParticle.Stop();

            //쓰러지고 난 다음의 혈흔 생성
            bloodblood_falldown[(int)_bloodEnum].Active();
            //레이저 On
            SetLaser(true);
            //분석 ui마크 표시
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

            //흉기 끄기
            weapons[(int)_weaponType].gameObject.SetActive(false);
            //쓰러지고 난 다음의 혈흔 생성
            bloodblood_falldown[(int)_bloodEnum].Active();
            //레이저 On
            SetLaser(true);
            //분석 ui마크 표시
            //victim.BloodUIMarkActive();

            if (_bloodEnum == BloodEnum.Pool || _bloodEnum == BloodEnum.Gush_Splash)
                victim.gameObject.SetActive(false);

            _player.isMove = true;
            isSimulating = false;
        }
    }

    public void SettingImpactSpatter()
    {
        //흉기 끄기
        weapons[(int)_weaponType].gameObject.SetActive(false);
        //쓰러지고 난 다음의 혈흔 생성
        bloodblood_falldown[(int)_bloodEnum].Active();

        _blinkManager.SetBlink(true);

        //자 켜기
        for (int i = 0; i < spatterBloods.Length; i++)
        {
            spatterBloods[i].SetActive(true);
        }
        //휴대폰 켜기

        //레이저 On
        SetLaser(true);
        _player.isMove = true;
    }

    ///// <summary>
    ///// 무기를 휘두른 후 3초 동안 기다리는 코루틴, 만약 코루틴 활성화 중 손을 과도하게 움직일경우 시간 초기화
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

    //    // 손 가만히 있으라는 UI Disable
    //    _player.DisableBloodIntoBloodUI();

    //    //흉기 끄기
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
        //효과음 관련된 AudioSource는 전부 여기에 연결
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

