using UnityEngine;

public enum ToolType
{
    Arrow, Phone, Rope, Sticker_Ruler, Paper_Ruler, Loupe, Footprints_Filming, Filming_Ruler, Evidence_Bag, CottonSwab, BurnierCaliper, Angle_Measuring
}

public class Tool : MonoBehaviour
{
	protected GamePlayManager _manager;

	public bool activeSelf
	{
		get
		{
			return gameObject.activeInHierarchy;
		}
	}

	protected virtual void Awake()
	{
		_manager = FindObjectOfType<GamePlayManager>(true);
	}

	public void SetActive()
	{
        //_toolsTR.SetPos();
        foreach (GameObject tool in _manager.tools)
        {
            if (tool == null)
                continue;

            tool.SetActive(false);
        }

        gameObject.SetActive(!gameObject.activeInHierarchy);
	}
	public void SetActive(bool b)
	{
		//_toolsTR.SetPos();
		gameObject.SetActive(b);
	}

	
}
