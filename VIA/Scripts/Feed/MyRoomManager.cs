using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MyRoomManager : MonoBehaviour
{
	[Space]
	[Header("LHS's Header")]
	public MyRoomButtonSet[] buttons;
	public Text likeCountText;          // 좋아요 CountText
	public int likeCount;                      // 좋아요 Count
									   //public bool otherGetMyLike;
	public GameObject[] leftPanels;
	public GameObject nickNameImage;
	public Text otherNickName;
	public Toggle likeToggle;
	public GameObject rankLodingPanel;
	public GameObject rankingObj;
	public Transform rankingContentTr;
	public Text myRankText;
	public Text myLikeText;
	public Sprite[] kingsImg;
	public Transform decoToggleGroup;
	public GameObject saveCompletePanel;
	public GameObject screenShotBG;
	bool isSave = false;
	const string waveURL = "https://www.viaback.com/api/viaRoom/wave";
	const string likeURL = "https://www.viaback.com/api/viaRoom/like";
	const string unLikeURL = "https://www.viaback.com/api/viaRoom/Unlike";
	const string rankURL = "https://www.viaback.com/api/viaRoom/Ranking";

	#region LHS

	#region GoToPlay
	public void OnClickGoToPlayButton()
	{
		WWWForm form = new WWWForm();
		form.AddField(..., _backendManager.id);
		GetRandomUser(waveURL, form);
	}

	async UniTaskVoid GetRandomUser(string url, WWWForm form)
	{
		using (UnityWebRequest www = UnityWebRequest.Post(url, form))
		{
			await www.SendWebRequest();

			if (www.isDone)
			{
				JObject jsonData = JObject.Parse(www.downloadHandler.text);

				JToken responseData = jsonData[...];

				_backendManager.otherNick = responseData[...].ToString();
				_backendManager.otherId = responseData[...].ToString();
			}
			else
			{
				print("error");
			}

			www.Dispose();
		}
		FindObjectOfType<SceneFadeManager>().FadeSceneLoad("MyRoom", true);
	}
	#endregion

	#region Like
	public void OnClickLikeToggle()
	{
		WWWForm form = new WWWForm();
		form.AddField(..., _backendManager.id);
		form.AddField(..., _backendManager.otherId);

		if (likeToggle.isOn)
		{
			likeCount++;
			likeCountText.text = likeCount.ToString();
			SetLike(likeURL, form);
		}
		else
		{
			likeCount--;
			likeCountText.text = likeCount.ToString();
			SetLike(unLikeURL, form);
		}
	}

	async UniTaskVoid SetLike(string url, WWWForm form)
	{
		UnityWebRequest www = UnityWebRequest.Post(url, form);
		await www.SendWebRequest();
		www.Dispose();
	}
	#endregion

	#region Ranking
	public void OnClickMyRoomRankingButton()
	{
		rankLodingPanel.SetActive(true);

		WWWForm form = new WWWForm();
		form.AddField(..., _backendManager.id);
		form.AddField(..., 1);
		SetRanking(rankURL, form);
	}

	async UniTaskVoid SetRanking(string url, WWWForm form)
	{
		using (UnityWebRequest www = UnityWebRequest.Post(url, form))
		{
			await www.SendWebRequest();

			if (www.isDone)
			{
				JObject jsonData = JObject.Parse(www.downloadHandler.text);
				JToken responseData = jsonData[...];

				JToken myRankdata = responseData[...];

				if (myRankdata == null || myRankdata.Type == JTokenType.Null)
				{
					myRankText.text = "-위";
					myLikeText.text = "0";
				}
				else
				{
					myRankText.text = $"{myRankdata[...]}위";
					myLikeText.text = $"{myRankdata[...]}";
				}

				JArray rankArray = JArray.Parse(responseData[...].ToString());

				for (int i = 0; i < rankArray.Count; i++)
				{
					RankingObj obj = Instantiate(rankingObj, rankingContentTr).GetComponent<RankingObj>();

					Texture2D profileTexture = null;
					int rank = int.Parse(rankArray[i][...].ToString());
					string nickName = rankArray[i][...].ToString();
					int likeCount = int.Parse(rankArray[i][...].ToString());

					if (!string.IsNullOrEmpty(rankArray[i][...].ToString()))
					{
						UnityWebRequest request = UnityWebRequestTexture.GetTexture(rankArray[i][...].ToString());
						await request.SendWebRequest();

						profileTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
					}

					switch (rank)
					{
						case 1:
							obj.InitRank(rank, nickName, likeCount, kingsImg[0], profileTexture);
							break;
						case 2:
							obj.InitRank(rank, nickName, likeCount, kingsImg[1], profileTexture);
							break;
						case 3:
							obj.InitRank(rank, nickName, likeCount, kingsImg[2], profileTexture);
							break;
						default:
							obj.InitRank(rank, nickName, likeCount, kingsImg[3], profileTexture);
							break;
					}
				}
			}
			else
			{
				print("error");
			}

			www.Dispose();
		}

		rankLodingPanel.SetActive(false);
	}

	public void OnClickCloseRankPanel()
	{
		for (int i = 0; i < rankingContentTr.childCount; i++)
		{
			Destroy(rankingContentTr.GetChild(i).gameObject);
		}
	}
	#endregion

	#region DecoPanel
	public void SetPanelOn(Toggle target)
	{
		if (!target.isOn)
		{
			if (!decoToggleGroup.GetComponent<ToggleGroup>().AnyTogglesOn())
				decoToggleGroup.GetChild(0).GetComponent<Toggle>().isOn = true;
			return;
		}

		for (int i = 0; i < decoToggleGroup.childCount; i++)
		{
			Toggle toggle = decoToggleGroup.GetChild(i).GetComponent<Toggle>();

			if (toggle == target)
			{
				print(target + " / " + i);
				ToggleActivate(false);
				OnRoomMenuButtonClick(i);
				SetPanelActive(i);
				break;
			}
		}
	}

	void SetPanelActive(int index)
	{
		for (int i = 0; i < panels.Length; i++)
		{
			panels[i].SetActive(i == index);
		}

		OnRoomBagButtonClick(1);
		OnRoomFloorButtonClick(0);
		OnRoomWallButtonClick(0);
	}

	public void SaveCompelte(string explain)
	{
		isSave = false;
		saveCompletePanel.GetComponentInChildren<Text>().text = explain;
		saveCompletePanel.SetActive(true);
		//SetButtons(3);
		miniloadImage.SetActive(false);		
	}

	public void ToggleActivate(bool isActive)
	{
		foreach (var toggle in decoToggleGroup.GetComponentsInChildren<Toggle>())
		{
			toggle.interactable = isActive;
		}
	}
	#endregion

	#endregion

}
