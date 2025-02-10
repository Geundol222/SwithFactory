using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TriLibCore;
using TriLibCore.General;

namespace Crosstales.FB.Demo
{
    public class Examples : MonoBehaviour
    {
        public void ETCButtonClick()
        {
            uploadEtcTr.GetComponentInParent<VerticalLayoutGroup>().padding.top = (uploadEtcTr.GetComponentInParent<VerticalLayoutGroup>().padding.top == 0) ? -1 : 0;

            if (isFirst == 2)
            {
                if (allModelingNum == 0)
                {
                    isFirst++;
                    ImageButtonClick();
                    loadPanel.SetActive(false);
                }
                else
                {
                    StartCoroutine(UploadModelingThumb($"{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/Modeling/Modeling", url));
                }
            }
            else
            {
                //for (int i = 0; i < uploadEtcTr.childCount; i++)
                //{
                //    uploadEtcTr.GetChild(i).GetChild(1).gameObject.SetActive(false);
                //}
            }
        }
        #endregion

        #region Public methods
        public IEnumerator UploadAWS(string fileName, string filePath, string form)
        {            
            else if (form == "Modeling")
            {
                fileName = $"{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/{form}/{form}{allModelingNum}";
                currentModelExtension = Path.GetExtension(filePath).ToLower();
                currentModelName = $"{form}{allModelingNum}{currentModelExtension}";
                URL3 = $"{url}/{fileName}/{currentModelName}";
                nowFileName = $"{fileName}/{currentModelName}";
            }

            byte[] fileRawBytes = File.ReadAllBytes(filePath);
            UnityWebRequest www = UnityWebRequest.Put(URL3, fileRawBytes);
            yield return www.SendWebRequest();

            if (form == "Modeling")
            {
                StartCoroutine(UploadModeling(fileName, url, Vector3.zero, Quaternion.identity, Vector3.one));
            }
        }

        IEnumerator UploadModelingThumb(string fileName, string url)
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < allModelingNum; i++)
            {
                bool isCheck = false;

                if (uploadEtcTr.childCount > 0)
                {
                    for (int j = 0; j < uploadEtcTr.childCount; ++j)
                    {
                        ContentsButton target = uploadEtcTr.GetChild(j).GetComponent<ContentsButton>();

                        if (i == int.Parse(target.nameText.text.Replace("Modeling", "")))
                        {
                            print("Model Check");
                            isCheck = true;
                            break;
                        }
                    }
                }

                if (isCheck)
                {
                    if (i + 1 == allModelingNum)
                    {
                        isFirst++;
                        loadPanel.SetActive(false);
                        ImageButtonClick();
                    }
                    continue;
                }

                string thumbfile = $"{url}/{fileName}{i}/Thumb.png";
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(thumbfile);
                yield return request.SendWebRequest();

                nowUpload++;
                string[] splitFileName = fileName.Split('/');

                print("Instantiate Model Button");
                GameObject insETC = Instantiate(contentsButton, uploadEtcTr);
                uploadEtcTr.GetComponent<GridLayoutGroup>().spacing = uploadEtcTr.GetComponent<GridLayoutGroup>().spacing == new Vector2(13, 13) ? new Vector2(12, 12) : new Vector2(13, 13);
                uploadEtcTr.parent.GetComponent<VerticalLayoutGroup>().spacing = uploadEtcTr.parent.GetComponent<VerticalLayoutGroup>().spacing == 12 ? 13 : 12;
                yield return new WaitForFixedUpdate();

                insETC.name = $"Modeling{i}{extensions[i]}";

                ContentsButton contents = insETC.GetComponent<ContentsButton>();

                contents.myButton.onClick.AddListener(() => { OnClickETCButton(insETC); });
                contents.nameText.text = $"Modeling{i}";
                contents.SetType(ContentsType.Modeling);


                RawImage img = contents.thumbNail;

                img.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                img.SetNativeSize();
                float width = img.gameObject.GetComponent<RectTransform>().sizeDelta.x;
                float height = img.gameObject.GetComponent<RectTransform>().sizeDelta.y;
                if (width > height)
                {
                    float dev = (float)Math.Round((height / width), 2);
                    img.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(160, dev * 160);
                }

                else
                {
                    float dev = (float)Math.Round((width / height), 2);
                    img.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(dev * 160, 160);
                }

                insETC.GetComponent<ContentsButton>().fileName = $"Modeling{i}{extensions[i]}";

                if (i + 1 == allModelingNum)
                {
                    isFirst++;
                    loadPanel.SetActive(false);
                    ImageButtonClick();
                }
                yield return null;
            }
        }

        public void OnClickApplyETCButton()
        {
            int index = currentModelName.IndexOf('.');

            string fileName = $"{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/Modeling/{currentModelName.Substring(0, index)}";

            StartCoroutine(UploadModeling(fileName, url, Vector3.zero, Quaternion.identity, Vector3.one, true));
        }

        public void OnClickETCButton(GameObject button)
        {
            currentModelName = button.name;
        }

        JObject nowJSON;

        public IEnumerator UploadModeling(string fileName, string url, Vector3 position, Quaternion rotation, Vector3 scale, bool texture = false, JObject json = null)
        {

            if (!isButtonClick)
                extensions.Add(currentModelExtension);

            string targetURL = $"{url}/{fileName}/{currentModelName}";
            yield return new WaitForEndOfFrame();
            UnityWebRequest www = UnityWebRequest.Get(targetURL);
            yield return www.SendWebRequest();
            modelPostion = position;
            modelRotation = rotation;
            modelScale = scale;
            hasTexture = texture;
            if (www.isDone)
            {
                Stream modelStream = new MemoryStream(www.downloadHandler.data);

                AssetLoaderOptions options = AssetLoader.CreateDefaultLoaderOptions();
                options.UseUnityNativeNormalCalculator = true;
                options.AddAssetUnloader = false;
                options.PivotPosition = PivotPosition.Bottom;
                options.AnimationType = AnimationType.None;
                AssetLoader.LoadModelFromStream(modelStream, Path.GetFileName(targetURL), Path.GetExtension(targetURL), OnLoad, OnMaterialsLoad, null, null, null, options);
                nowJSON = json;
            }

            www.Dispose();
        }

        public void OnLoad(AssetLoaderContext context)
        {
            _galleryManager.myPrefab = context.RootGameObject;            
        }

        public void OnMaterialsLoad(AssetLoaderContext context)
        {
            AutoResize(desiredSize, nowJSON);
            nowJSON = null;
        }

        public void AutoResize(Vector3 targetSize, JObject json = null)
        {
            _galleryManager.myPrefab.name = currentModelName;
            Rigidbody rigid = null;

            if (_galleryManager.myPrefab.GetComponent<MeshRenderer>())
            {
                _galleryManager.myPrefab.AddComponent<BoxCollider>();
                rigid = _galleryManager.myPrefab.AddComponent<Rigidbody>();
                _galleryManager.myPrefab.tag = "Modeling";
            }
            else
            {
                foreach (Transform child in _galleryManager.myPrefab.transform)
                {
                    if (child.GetComponent<MeshRenderer>())
                    {
                        GameObject parent = _galleryManager.myPrefab;

                        _galleryManager.myPrefab = child.gameObject;
                        _galleryManager.myPrefab.transform.SetParent(_galleryManager.all.transform);
                        child.gameObject.AddComponent<BoxCollider>();
                        rigid = child.gameObject.AddComponent<Rigidbody>();
                        child.gameObject.name = currentModelName;
                        child.gameObject.tag = "Modeling";

                        Destroy(parent);
                        break;
                    }
                }
            }
            _galleryManager.myPrefab.AddComponent<GalleryModelingInfo>().Init_JsonToInfo(json);
            currentModelInfo = _galleryManager.myPrefab.GetComponent<GalleryModelingInfo>();
            currentModelInfo.SetMyMat(Instantiate(basemt));

            currentModelInfo._objInfoManager = _galleryManager._infoManager[0];

            if (hasTexture && !isButtonClick)
            {
                _galleryManager.myPrefab.transform.localScale = modelScale;
            }
            else
            {
                Bounds bounds = new Bounds(_galleryManager.myPrefab.transform.position, Vector3.zero);
                //Renderer renderer = _galleryManager.myPrefab.GetComponent<MeshRenderer>();

                //bounds.Encapsulate(renderer.bounds);
                foreach (Renderer renderer in _galleryManager.myPrefab.GetComponentsInChildren<Renderer>())
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                Vector3 currentSize = bounds.size;

                if (currentSize.x == 0 || currentSize.y == 0 || currentSize.z == 0)
                {
                    currentSize = Vector3.one; // 기본값 설정
                }

                Vector3 scaleRatio = new Vector3(
                    targetSize.x / currentSize.x,
                    targetSize.y / currentSize.y,
                    targetSize.z / currentSize.z
                    );

                float minScale = Mathf.Min(scaleRatio.x, scaleRatio.y, scaleRatio.z);
                _galleryManager.myPrefab.transform.localScale = new Vector3(minScale, minScale, minScale);

                _galleryManager.myPrefab.transform.position -= bounds.center;
            }


            if (rigid != null)
            {
                rigid.useGravity = true;
                rigid.constraints = RigidbodyConstraints.FreezeAll;

                if (_galleryManager.myPrefab.transform.position.y > 0)
                {
                    Vector3 origin = _galleryManager.myPrefab.transform.position;

                    _galleryManager.myPrefab.transform.position = new Vector3(origin.x, 0f, origin.z);
                }
            }

            if (hasTexture)
            {
                StartCoroutine(HasTextureSet());
            }
            else
            {
                //ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg") };
                //string path = FileBrowser.Instance.OpenSingleFile("Image File", "", "", extensions);
                //
                //if (string.IsNullOrEmpty(path))
                //    return;
                //StartCoroutine(UploadTextureInAWS(path));
                uploadTexturePanel.SetActive(true);
            }
        }

        IEnumerator HasTextureSet()
        {
            ShareTexture(_galleryManager.myPrefab);
            int index = currentModelName.IndexOf('.');
            if (isButtonClick)
            {
                _galleryManager._state = GalleryManager.GalleryState.ObjSet;
            }

            currentModelInfo.SetMyMat(Instantiate(basemt));

            print($"current Material : {currentModelInfo.myMat.name}");

            for (int i = 0; i < textureUploadGrid.childCount; ++i)
            {
                string type = textureUploadGrid.GetChild(i).GetChild(0).gameObject.name;
                bool complete = false;

                complete = (i == textureUploadGrid.childCount - 1);

                if (_backEndManager.galleryName != string.Empty)
                    yield return StartCoroutine(LoadTexture(type, $"{url}/{_backEndManager.galleryName.Split('_')[0]}/{_backEndManager.currentGalleryCode}/Modeling/{currentModelName.Substring(0, index)}/{type}.png", complete));
                else
                    yield return StartCoroutine(LoadTexture(type, $"{url}/{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/Modeling/{currentModelName.Substring(0, index)}/{type}.png", complete));
            }
        }

        public void SelectTextureType(Button button)
        {
            ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg") };
            string path = FileBrowser.Instance.OpenSingleFile("Image File", "", "", extensions);

            if (string.IsNullOrEmpty(path)) 
                return;

            byte[] fileData = File.ReadAllBytes(path);

            Texture2D texture = new Texture2D(1, 1);
            if (texture.LoadImage(fileData))
            {
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                button.image.sprite = newSprite;
                texturePathDic.Add(button.gameObject.name, path);
                validKey.Add(button.gameObject.name);
            }
        }

        public void OnClickUploadTexture()
        {
            StartCoroutine(ValidTexture());
        }

        IEnumerator ValidTexture()
        {
            string lastValidKey = validKey.Count > 0 ? validKey[validKey.Count - 1] : string.Empty;

            currentModelInfo.SetMyMat(Instantiate(basemt));

            for (int i = 0; i < textureUploadGrid.childCount; ++i)
            {
                string key = textureUploadGrid.GetChild(i).GetChild(0).gameObject.name;

                if (!texturePathDic.ContainsKey(key))
                    continue;

                string path = texturePathDic[key];

                bool isLast = key == lastValidKey;

                yield return StartCoroutine(UploadTextureInAWS(key, path, isLast));
            }

            for (int i = 0; i < textureUploadGrid.childCount; ++i)
            {
                textureUploadGrid.GetChild(i).GetChild(0).GetComponent<Button>().image.sprite = defaultTexture;
            }

            texturePathDic.Clear();
            validKey.Clear();
            uploadTexturePanel.SetActive(false);
        }

        IEnumerator UploadTextureInAWS(string type, string path, bool complete)
        {
            byte[] fileData = File.ReadAllBytes(path);
            string fileExtension = Path.GetExtension(path).ToLower();

            int index = currentModelName.IndexOf('.');
            string fileName = $"{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/Modeling/{currentModelName.Substring(0, index)}";
            //string URL3 = $"{url}/{fileName}/{currentModelName.Substring(0, index)}{fileExtension}";
            string URL3 = $"{url}/{fileName}/{type}{fileExtension}";

            UnityWebRequest request = UnityWebRequest.Put(URL3, fileData);
            yield return request.SendWebRequest();

            if (request.isDone)
            {
                StartCoroutine(LoadTexture(type, URL3, complete));
                request.Dispose();
            }
        }

        public void ShareTexture(GameObject target)
        {
            foreach (Transform child in imgAll)
            {
                if (!child.gameObject.name.Contains("Modeling"))
                    continue;

                if (child.gameObject.name.Equals(target.name) && child.GetComponent<GalleryModelingInfo>().modelingTexture != null)
                {
                    target.GetComponent<GalleryModelingInfo>().modelingTexture = child.GetComponent<GalleryModelingInfo>().modelingTexture;
                    break;
                }
            }
        }

        IEnumerator LoadTexture(string type, string url, bool complete)
        {
            //isButtonClick = false;

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;

                print($"current Material : {currentModelInfo.myMat.name}");

                switch (type)
                {
                    case "Base":
                        currentModelInfo.SetMatTexture("_BaseMap", tex);
                        break;
                    case "Metallic":
                        currentModelInfo.SetMatTexture("_MetallicGlossMap", tex);
                        break;
                    case "Normal":
                        currentModelInfo.SetMatTexture("_BumpMap", tex);
                        break;
                    case "Height":
                        currentModelInfo.SetMatTexture("_ParallaxMap", tex);
                        break;
                    case "Occlusion":
                        currentModelInfo.SetMatTexture("_OcclusionMap", tex);
                        break;
                }

            }

            //yield return StartCoroutine(TakeCaptureAndButton());
            if (complete)
            {
                if (hasTexture && isButtonClick)
                {
                    _galleryManager.myPrefab.transform.SetParent(_galleryManager.all.transform);
                    _galleryManager.myPrefab.transform.localPosition = modelPostion;
                    _galleryManager.myPrefab.transform.localRotation = modelRotation;

                    if (captureCam.gameObject.activeSelf)
                        captureCam.gameObject.SetActive(false);

                    ShareTexture(_galleryManager.myPrefab);

                    _galleryManager.isProp = true;
                    isButtonClick = false;
                }
                else
                {
                    yield return StartCoroutine(TakeCaptureAndButton());
                }
            }
        }

        IEnumerator TakeCaptureAndButton()
        {
            if (uploadEtcTr.childCount > 0)
            {
                for (int i = 0; i < uploadEtcTr.childCount; i++)
                {
                    if (uploadEtcTr.GetChild(i).gameObject.name.Contains(_galleryManager.myPrefab.name))
                    {
                        _galleryManager.myPrefab.transform.SetParent(_galleryManager.all.transform);
                        _galleryManager.myPrefab.transform.localPosition = modelPostion;
                        _galleryManager.myPrefab.transform.localRotation = modelRotation;

                        if (captureCam.gameObject.activeSelf)
                            captureCam.gameObject.SetActive(false);

                        _backEndManager.isModelSet = true;
                        yield break;
                    }
                }
            }

            _galleryManager.myPrefab.layer = 12;
            _galleryManager.myPrefab.transform.SetParent(modelTr);
            _galleryManager.myPrefab.transform.localPosition = Vector3.zero;
            _galleryManager.myPrefab.transform.localRotation = Quaternion.identity;

            captureCam.gameObject.SetActive(true);

            RenderTexture renderTexture = new RenderTexture(256, 256, 16);
            captureCam.targetTexture = renderTexture;
            captureCam.Render();

            yield return new WaitForEndOfFrame();

            RenderTexture.active = renderTexture;
            Texture2D tex = new Texture2D(256, 256, UnityEngine.TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tex.Apply();

            RenderTexture current = RenderTexture.active;

            float texWidth = tex.width;
            float texHeight = tex.height;

            RenderTexture copied = new RenderTexture((int)texWidth, (int)texHeight, 0);
            Graphics.Blit(tex, copied);
            RenderTexture.active = copied;
            Texture2D texCopied = new Texture2D((int)texWidth, (int)texHeight);
            texCopied.ReadPixels(new Rect(0, 0, (int)texWidth, (int)texHeight), 0, 0);
            texCopied.Apply();
            RenderTexture.active = current;
            byte[] fileData = texCopied.EncodeToPNG();

            int index = currentModelName.IndexOf('.');
            string fileName = $"{_backEndManager.nickName}/{_backEndManager.currentGalleryCode}/Modeling/{currentModelName.Substring(0, index)}";
            string URL3 = $"{url}/{fileName}/Thumb.png";

            UnityWebRequest request = UnityWebRequest.Put(URL3, fileData);
            yield return request.SendWebRequest();

            yield return new WaitForEndOfFrame();

            GameObject etcObj = Instantiate(contentsButton, uploadEtcTr, false);

            etcObj.name = currentModelName;

            ContentsButton contents = etcObj.GetComponent<ContentsButton>();

            contents.myButton.onClick.AddListener(() => { OnClickETCButton(etcObj); });
            contents.SetType(ContentsType.Modeling);

            contents.nameText.text = $"{currentModelName.Substring(0, index)}";

            currentModelInfo.modelingTexture = tex;

            RawImage ri = contents.thumbNail;
            ri.texture = tex;
            ri.SetNativeSize();

            float width = ri.gameObject.GetComponent<RectTransform>().sizeDelta.x;
            float height = ri.gameObject.GetComponent<RectTransform>().sizeDelta.y;
            if (width > height)
            {
                float dev = (float)Math.Round((height / width), 2);
                ri.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(160, dev * 160);
            }

            else
            {
                float dev = (float)Math.Round((width / height), 2);
                ri.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(dev * 160, 160);
            }

            etcObj.GetComponent<ContentsButton>().fileName = currentModelName;

            yield return new WaitForEndOfFrame();

            _galleryManager.myPrefab.transform.SetParent(_galleryManager.all.transform);
            _galleryManager.myPrefab.transform.localPosition = modelPostion;
            _galleryManager.myPrefab.transform.localRotation = modelRotation;

            RenderTexture.active = null;
            captureCam.targetTexture = null;
            Destroy(renderTexture);

            captureCam.gameObject.SetActive(false);

            if (!isButtonClick)
                allModelingNum++;

            if (_backEndManager.uploadGalleryData == null)
                ApplyButtonClick();

            if (hasTexture)
                _backEndManager.isModelSet = true;
            else
            {
                _galleryManager._state = GalleryManager.GalleryState.ObjSet;
                _galleryManager.isProp = true;
            }
        }

        public IEnumerator SetModeling(string fileName, string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url + "/" + fileName);
            yield return request.SendWebRequest();

            string myData = Application.persistentDataPath + Path.DirectorySeparatorChar + fileName;
            if (File.Exists(myData))
            {
                File.Delete(myData);
            }
            File.WriteAllBytes(myData + "/" + fileName, request.downloadHandler.data);

            yield return null;

        }
        #endregion

        #region Single
        public void OpenSingleModelingFile(string path)
        {
            //ExtensionFilter[] extensions = { new ExtensionFilter("3D Object Files" , "fbx" , "obj") };
            //string path = FileBrowser.Instance.OpenSingleFile("Open file" , "" , "" , extensions);
            string fileName = Path.GetFileName(path);
            string destPath;
            if (!Directory.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + _backEndManager.galleryName))
            {
                Directory.CreateDirectory(Application.persistentDataPath + Path.DirectorySeparatorChar + _backEndManager.galleryName);
                destPath = Application.persistentDataPath + Path.DirectorySeparatorChar + _backEndManager.galleryName;
            }

            else
            {
                destPath = Application.persistentDataPath + Path.DirectorySeparatorChar + _backEndManager.galleryName;
            }

            if (File.Exists(Path.Combine(destPath, fileName)))
            {
                File.Delete(Path.Combine(destPath, fileName));
            }
            File.Copy(path, Path.Combine(destPath, fileName));
            form = "Modeling";
            StartCoroutine(UploadAWS(fileName, path, form));
        }
        #endregion
    }
}
// © 2017-2021 crosstales LLC (https://www.crosstales.com)