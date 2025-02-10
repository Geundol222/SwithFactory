using Crosstales.FB.Demo;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackEndManager : MonoBehaviour
{        
    public void LikeToggle(bool tf)
    {
        WWWForm form = new WWWForm();
        form.AddField(..., nickName);
        form.AddField(..., galleryCode);
        if (tf)
        {
            BaseSQL(likeURL, form);
        }

        else
        {
            BaseSQL(unlikeURL, form);
        }
    }

    public IEnumerator BaseSQL(string uri, WWWForm form, Action act = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(uri, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                if (act != null)
                {
                    act();
                }
            }

            else
            {
                print("error");
            }
            www.Dispose();
        }
    }

    public void OnClickPublicContents()
    {
        all = GameObject.Find("All").transform;
        WWWForm form = new WWWForm();
        form.AddField(..., BaseData.getv2);
        form.AddField(..., galleryName);
        form.AddField(..., nickName);
        GetGallery(galleryURL , form);
    }

    async UniTaskVoid GetGallery(string uri, WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(uri, form))
        {
            Examples _aws = FindObjectOfType<Examples>();
            GalleryManager _galleryManager = FindObjectOfType<GalleryManager>();
            switch (mySize)
            {
                case "소형":
                    _aws.maxUpload = 10;
                    _aws.maxMB = 10;
                    break;

                case "중형":
                    _aws.maxUpload = 30;
                    _aws.maxMB = 20;
                    break;

                case "대형":
                    _aws.maxUpload = 50;
                    _aws.maxMB = 40;
                    break;

                default:
                    _aws.maxUpload = 10;
                    _aws.maxMB = 10;
                    break;
            }
            try
            {
                await www.SendWebRequest();
            }

            catch
            {
                SwaggerData sd = JsonUtility.FromJson<SwaggerData>(www.downloadHandler.text);

                if (!string.IsNullOrEmpty(sd.success))
                {
                    await UniTask.WaitUntil(() => _galleryManager.isLoaded);
                    _galleryManager.inpanel.SetActive(false);
                    _galleryManager.loadPanel.SetActive(false);
                    return;
                }
            }
            if (www.result == UnityWebRequest.Result.Success)
            {                  
                JObject jsonData = JObject.Parse(www.downloadHandler.text);
                StartCoroutine(InstantiateBundle(jsonData));
            }
            www.Dispose();
        }
    }

    public void OnClickGalleryContents()
    {
        all = GameObject.Find("All").transform;
        WWWForm form = new WWWForm();
        form.AddField(..., BaseData.getv2);
        form.AddField(..., galleryName);
        form.AddField(..., nickName);
        StartCoroutine(SetLikeToggle(galleryURL, form));        
    }

    IEnumerator SetLikeToggle(string url, WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                JObject jData = JObject.Parse(www.downloadHandler.text);
                GalleryManager _galleryManager = FindObjectOfType<GalleryManager>();
                _galleryManager.likeToggle.SetIsOnWithoutNotify(bool.Parse(jData[...].ToString()));
            }
            www.Dispose();
        }
    }

    
    public IEnumerator InstantiateBundle(JObject data)
    {
        
        else if (myName.Contains("Modeling"))
        {
            JArray jar = JArray.Parse(data[...].ToString());
            JObject json;
            try
            {
                json = JObject.Parse(jar[infoNum].ToString());
            }
            catch
            {
                json = null;
            }
            if (json != null)
            {
                if (... + i == json[...].ToString())
                {
                    infoNum++;
                    callBackQueue.Enqueue(() => { StartCoroutine(InsertModel(myName , myPos , Quaternion.Euler(myRot) , myScale, json)); });
                }

                else
                {
                    callBackQueue.Enqueue(() => { StartCoroutine(InsertModel(myName , myPos , Quaternion.Euler(myRot) , myScale, null)); });
                }
            }

            else
            {
                callBackQueue.Enqueue(() => { StartCoroutine(InsertModel(myName , myPos , Quaternion.Euler(myRot) , myScale , null)); });
            }                
        }

        _galleryManager.loadingImg.fillAmount = (float)Math.Round((float)(i - callBackQueue.Count )  / myNames.Count  , 2);
        _galleryManager.loadingPerText.text = ((float)Math.Round((float)(i - callBackQueue.Count) / myNames.Count, 2) * 100f).ToString("00") + "%";

        if (callBackQueue.Count < 1)
        {
            if ((int)( i / myNames.Count ) == 1)
            {
                _galleryManager.loadingImg.fillAmount = 1;
            }
        }
        yield return new WaitForEndOfFrame();
        

        while (callBackQueue.Count > 0)
        {
            isModelSet = false;

            Action action = callBackQueue.Dequeue();

            action();

            _galleryManager.loadingImg.fillAmount = (float)Math.Round((float)( myNames.Count - callBackQueue.Count ) / myNames.Count, 2);
            _galleryManager.loadingPerText.text = ((float)Math.Round((float)( myNames.Count - callBackQueue.Count ) / myNames.Count , 2) * 100f ).ToString("00") + "%";
            yield return new WaitUntil(() => { return isModelSet; });
        }

        yield return null;
    }
        
    IEnumerator InsertModel(string myName , Vector3 myPos , Quaternion myRot , Vector3 myScale, JObject json)
    {
        _aws.form = "Modeling";
        _aws.currentModelName = myName;
        int index = myName.IndexOf('.');
        _aws.currentModelExtension = Path.GetExtension(myName);
        string fileName = string.Empty;

        if (isMake)
            fileName = $"{galleryName.Split('_')[0]}/{currentGalleryCode}/Modeling/{myName.Substring(0 , index)}";
        else
            fileName = $"{allName.Split('_')[0]}/{currentGalleryCode}/Modeling/{myName.Substring(0, index)}";

        yield return StartCoroutine(_aws.UploadModeling(fileName , url , myPos , myRot , myScale , true, json));
    }

    public void SetLike(bool isLike)
    {
        WWWForm form = new WWWForm();
        form.AddField(..., nickName);
        form.AddField(..., currentGalleryCode);

        if (isLike)
        {
            StartCoroutine(SettingLike(likeGalleryURL, form));
        }
        else
        {
            StartCoroutine(SettingLike(unLikeGalleryURL, form));
        }
    }

    IEnumerator SettingLike(string url, WWWForm form)
    {
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            yield break;
        }
        else
        {
            GalleryManager galleryManager = FindObjectOfType<GalleryManager>();

            galleryManager.SetNotice("좋아요설정에 문제가 발생했습니다.");            
        }

        request.Dispose();
    }
}
