using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiteAnalysisManager : MonoBehaviour
{
    [Header("Play")]
    GamePlayManager _manager;
    Player _player;
    public Transform playerPos;

    private void Awake()
    {
        _manager = FindObjectOfType<GamePlayManager>();
        _player = FindObjectOfType<Player>();
    }

    private void OnEnable()
    {
        _player.transform.position = playerPos.position;
        _player.transform.rotation = playerPos.rotation;

        _manager._pdaManager.gameObject.SetActive(false);
        _player.isMove = true;
    }
}
