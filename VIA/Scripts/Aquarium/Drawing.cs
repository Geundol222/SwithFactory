using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drawing : MonoBehaviourPun, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Transform spawnPoint;
    public GameSceneManager gameSceneManager;

    [SerializeField]
    [Tooltip("The Canvas which is a parent to this Mouse Drawing Component")]
    private Canvas HostCanvas;

    [Range(2, 20)]
    [Tooltip("The Pens Radius")]
    public int penRadius = 10;
    public int curPenRadius;

    [Tooltip("The Pens Colour.")]
    public Color32 penColour = new Color32(0, 0, 0, 255);

    [Tooltip("The Drawing Background Colour.")]
    public Color32 backroundColour = new Color32(0, 0, 0, 0);

    [SerializeField]
    [Tooltip("Pen Pointer Graphic GameObject")]
    private Image penPointer;

    [Tooltip("Toggles between Pen and Eraser.")]
    public bool IsEraser = false;

    private bool _isInFocus = false;
    /// <summary>
    /// Is this Component in focus.
    /// </summary>
    public bool IsInFocus
    {
        get => _isInFocus;
        private set
        {
            if (value != _isInFocus)
            {
                _isInFocus = value;
                TogglePenPointerVisibility(value);
            }
        }
    }

    public bool startDrawing = false;

    public Color32[] colors;
    public RectTransform rectTransform;
    public Queue<Texture> fishQueue;
    [HideInInspector] public int fishCount = 0;

    private DrawObj fish;
    private float m_scaleFactor = 10;
    private RawImage m_image;

    private Texture2D undoTexture;
    private Stack<Color[]> undoStack;
    private Stack<Color[]> redoStack;

    private Vector2? m_lastPos;

    private void Awake()
    {
        fishQueue = new Queue<Texture>();

        undoStack = new Stack<Color[]>();
        redoStack = new Stack<Color[]>();

        m_image = transform.GetComponent<RawImage>();
        TogglePenPointerVisibility(false);
    }

    #region Drawing
    /// <summary>
    /// Initialisation logic.
    /// </summary>
    public void Init()
    {
        SetPenColour(Color.black);
        curPenRadius = penRadius;

        //Set scale Factor...        
        m_scaleFactor = HostCanvas.scaleFactor * 2;
        
        Texture2D tex = new Texture2D(Convert.ToInt32(rectTransform.rect.width / m_scaleFactor), Convert.ToInt32(rectTransform.rect.height / m_scaleFactor), TextureFormat.RGBA32, false);

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                tex.SetPixel(i, j, backroundColour);
            }
        }

        tex.Apply();
        m_image.texture = tex;
    }

    /// <summary>
    /// Writes the pixels to the Texture at the given ScreenSpace position.
    /// </summary>
    /// <param name="pos"></param>
    private void WritePixels(Vector2 pos)
    {
        pos /= m_scaleFactor;
        Texture mainTex = m_image.texture;
        Texture2D tex2d = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGBA32, false);

        RenderTexture curTex = RenderTexture.active;
        RenderTexture renTex = new RenderTexture(mainTex.width, mainTex.height, 32);

        Graphics.Blit(mainTex, renTex);
        RenderTexture.active = renTex;

        tex2d.ReadPixels(new Rect(0, 0, mainTex.width, mainTex.height), 0, 0);

        Color32 col = IsEraser ? backroundColour : penColour;
        List<Vector2> positions = m_lastPos.HasValue ? GetLinearPositions(m_lastPos.Value, pos) : new List<Vector2>() { pos };

        foreach (Vector2 position in positions)
        {
            List<Vector2> pixels = GetNeighbouringPixels(new Vector2(mainTex.width, mainTex.height), position, penRadius);

            if (pixels.Count > 0)
                foreach (Vector2 p in pixels)
                    tex2d.SetPixel((int)p.x, (int)p.y, col);
        }

        tex2d.Apply();

        RenderTexture.active = curTex;
        renTex.Release();
        Destroy(renTex);
        Destroy(mainTex);
        curTex = null;
        renTex = null;
        mainTex = null;

        m_image.texture = tex2d;
        undoTexture = tex2d;
        m_lastPos = pos;
    }

    /// <summary>
    /// Gets the neighbouring pixels at a given screenspace position.
    /// </summary>
    /// <param name="textureSize">The texture size or pixel domain.</param>
    /// <param name="position">The ScreenSpace position.</param>
    /// <param name="brushRadius">The Brush radius.</param>
    /// <returns>List of pixel positions.</returns>
    private List<Vector2> GetNeighbouringPixels(Vector2 textureSize, Vector2 position, int brushRadius)
    {
        var pixels = new List<Vector2>();

        for (int i = -brushRadius; i < brushRadius; i++)
        {
            for (int j = -brushRadius; j < brushRadius; j++)
            {
                var pxl = new Vector2(position.x + i, position.y + j);
                if (pxl.x > 0 && pxl.x < textureSize.x && pxl.y > 0 && pxl.y < textureSize.y)
                    pixels.Add(pxl);
            }
        }

        return pixels;
    }

    /// <summary>
    /// Interpolates between two positions with a spacing (default = 2)
    /// </summary>
    /// <param name="firstPos"></param>
    /// <param name="secondPos"></param>
    /// <param name="spacing"></param>
    /// <returns>List of interpolated positions</returns>
    private List<Vector2> GetLinearPositions(Vector2 firstPos, Vector2 secondPos, int spacing = 2)
    {
        var positions = new List<Vector2>();

        var dir = secondPos - firstPos;

        if (dir.magnitude <= spacing)
        {
            positions.Add(secondPos);
            return positions;
        }

        for (int i = 0; i < dir.magnitude; i += spacing)
        {
            var v = Vector2.ClampMagnitude(dir, i);
            positions.Add(firstPos + v);
        }

        positions.Add(secondPos);
        return positions;
    }
    #endregion

    #region SettingPen
    /// <summary>
    /// Sets the Pens Colour.
    /// </summary>
    /// <param name="color"></param>
    public void SetPenColour(Color32 color) => penColour = color;

    /// <summary>
    /// Sets the Radius of the Pen.
    /// </summary>
    /// <param name="radius"></param>
    public void SetPenRadius(int radius) => penRadius = radius;

    /// <summary>
    /// Sets the Size of the Pen Pointer.
    /// </summary>
    private void SetPenPointerSize()
    {
        var rt = penPointer.rectTransform;
        rt.sizeDelta = new Vector2(penRadius * 5, penRadius * 5);
    }

    /// <summary>
    /// Sets the position of the Pen Pointer Graphic.
    /// </summary>
    /// <param name="pos"></param>
    private void SetPenPointerPosition(Vector2 pos)
    {
        penPointer.transform.position = pos;
    }

    /// <summary>
    /// Toggles the visibility of the Pen Pointer Graphic.
    /// </summary>
    /// <param name="isVisible"></param>
    private void TogglePenPointerVisibility(bool isVisible)
    {
        if (isVisible)
            SetPenPointerSize();

        penPointer.gameObject.SetActive(isVisible);
        Cursor.visible = !isVisible;
    }

    public void ChangeColor(int index)
    {
        penRadius = curPenRadius;
        SetPenColour(colors[index]);
    }
    #endregion

    #region EventSystem
    /// <summary>
    /// On Mouse Pointer entering this Components Image Space.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        SetPenPointerPosition(eventData.position);
        IsInFocus = true;
    }

    /// <summary>
    /// On Mouse Pointer exiting this Components Image Space.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData) => IsInFocus = false;

    public void Erase(Toggle toggle)
    {
        IsEraser = toggle.isOn;

        curPenRadius = penRadius;
        penRadius = 5;
        SetPenPointerSize();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDrawing = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float inputX = eventData.position.x + -((Screen.width + (-rectTransform.rect.width)) * 0.5f + (-100f));
        float inputY = eventData.position.y + -((Screen.height + (-rectTransform.rect.height)) * 0.5f + 25f);

        Vector2 pos = new Vector2(inputX, inputY);

        if (IsInFocus)
        {
            SetPenPointerPosition(eventData.position);

            if (eventData.IsPointerMoving())
                WritePixels(pos);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        startDrawing = false;

        m_lastPos = null;

        Color[] tempPixel = undoTexture.GetPixels();

        undoStack.Push(tempPixel);

        undoTexture = null;
    }
    #endregion

    #region Buttons
    public void ClickCompleteButton(SketchPanel panel)
    {
        gameSceneManager.ShowPopUpUI("물고기 그림을 완성하셨습니까?",
                delegate
                {
                    Texture2D sendTexture = m_image.texture as Texture2D;

                    byte[] bytes = sendTexture.EncodeToPNG();
                                        
                    photonView.RPC("InstantiateFish", RpcTarget.MasterClient, bytes, sendTexture.width, sendTexture.height);
                    panel.CompleteAnimation(false);
                });
    }

    public void ClickBackButton(SketchPanel panel)
    {
        gameSceneManager.ShowPopUpUI("정말 그만하시겠습니까?\n지금까지 그린 내용은 보존되지 않습니다.",
                delegate
                {
                    panel.CompleteAnimation(true);
                });
    }

    public void ClickUndoButton()
    {
        if (undoStack.Count > 0)
        {
            Color[] pixels = undoStack.Pop();
            redoStack.Push(pixels);

            Texture mainTex = m_image.texture;
            Texture2D image = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGBA32, false);

            image.SetPixels(pixels);

            image.Apply();

            m_image.texture = image;
        }
    }

    public void ClickRedoButton()
    {
        if (redoStack.Count > 0)
        {
            Color[] pixels = redoStack.Pop();
            undoStack.Push(pixels);

            Texture mainTex = m_image.texture;
            Texture2D image = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGBA32, false);

            image.SetPixels(pixels);

            image.Apply();

            m_image.texture = image;
        }
    }
    #endregion

    [PunRPC]
    public void InstantiateFish(byte[] bytes, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        tex.LoadImage(bytes);

        if (fishCount < 3)
        {
            fish = PhotonNetwork.InstantiateRoomObject("Prefabs/DrawPlane", spawnPoint.position, Quaternion.identity).GetComponent<DrawObj>();
            fishCount++;
            fish?.OnComplete(tex);
        }
        else
        {
            fishQueue.Enqueue(tex);
        }
    }
}
