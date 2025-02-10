using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeSphere : MonoBehaviour
{
    BlendIntensityManager _blendIntensityManager;
    GamePlayManager _manager;
    public Animator[] swipeSphere;
    public bool swipeStart = false;

    private void Start()
    {
        _blendIntensityManager = FindObjectOfType<BlendIntensityManager>();
        _manager = FindObjectOfType<GamePlayManager>();
    }

    private void OnEnable()
    {
        swipeStart = false;

        swipeSphere[0].SetBool("Emission", true);
        swipeSphere[1].SetBool("Emission", false);

        swipeSphere[0].transform.GetChild(0).gameObject.SetActive(true);
        swipeSphere[1].transform.GetChild(0).gameObject.SetActive(false);
    }

    public void SwipeStart()
    {
        swipeSphere[0].SetBool("Emission", false);
        swipeSphere[1].SetBool("Emission", true);

        swipeSphere[0].transform.GetChild(0).gameObject.SetActive(false);
        swipeSphere[1].transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SwipeEnd()
    {
        if (swipeStart)
        {
            swipeSphere[0].gameObject.SetActive(false);
            swipeSphere[1].gameObject.SetActive(false);

            _blendIntensityManager.UpdateMaterialProperty(1f, false);
            _manager.smearTypeArray[(int)_manager._smearType].SetActive(true);
            _manager.HitEnd();
        }
    }
}
