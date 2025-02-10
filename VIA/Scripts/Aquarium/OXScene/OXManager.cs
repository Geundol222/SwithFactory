using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

#region CustomProperty
public class CustomProperty
{
    public bool GetLoad(Player player)
    {
        PhotonHashtable property = player.CustomProperties;

        if (property.ContainsKey("Load"))
            return (bool)property["Load"];
        else
            return false;
    }

    public void SetLoad(Player player, bool load)
    {
        PhotonHashtable property = player.CustomProperties;

        property["Load"] = load;
        player.SetCustomProperties(property);
    }

    public int GetLoadTime(Room room)
    {
        PhotonHashtable property = room.CustomProperties;

        if (property.ContainsKey("LoadTime"))
            return (int)property["LoadTime"];
        else
            return -1;
    }

    public void SetLoadTime(Room room, int loadTime)
    {
        PhotonHashtable property = room.CustomProperties;

        property["LoadTime"] = loadTime;
        room.SetCustomProperties(property);
    }
}
#endregion

public class OXManager : MonoBehaviourPunCallbacks
{
    public float countDownTimer;
    public Transform playerSpawnPoint;
    public GamePlayManager gamePlayerManager;
    public List<GameObject> watchingPlayers = new List<GameObject>();
    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    [HideInInspector] public CustomProperty customProperty;

    private void Awake()
    {
        customProperty = new CustomProperty();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        string name = $"OXRoom";
        RoomOptions options = new RoomOptions { MaxPlayers = 20 };

        PhotonNetwork.JoinOrCreateRoom(roomName: name, roomOptions: options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonView player = PhotonNetwork.Instantiate("Prefabs/Player", playerSpawnPoint.position, Quaternion.identity).GetPhotonView();

        photonView.RPC("AddComponent", RpcTarget.AllBuffered, player.ViewID);

        if (PhotonNetwork.InRoom)
            customProperty.SetLoad(PhotonNetwork.LocalPlayer, true);
    }

    [PunRPC]
    public void AddComponent(int viewID)
    {
        GameObject newPlayer = PhotonView.Find(viewID).gameObject;

        if (!newPlayer.GetPhotonView().IsMine)
            watchingPlayers.Add(newPlayer);

        newPlayer.AddComponent<OXPlayer>();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        if (changedProps.ContainsKey("Load"))
        {
            // 모든 플레이어 로딩 완료
            if (PlayerLoadCount() > 1)
            {
                gamePlayerManager.GameStart();
            }
            // 일부 플레이어 로딩 완료
            else
            {
                print($"Wait Players {PlayerLoadCount()} / {PhotonNetwork.PlayerList.Length}");
            }
        }
    }

    private int PlayerLoadCount()
    {
        int loadCount = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (customProperty.GetLoad(player))
                loadCount++;
        }

        return loadCount;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }
}
