using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowProcessStatesUI : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI pageShowText;


    private const int itemCount = 10;
    public GameObject IntemPrefab;
    public RectTransform parent;
    private List<ItemUI> items = new List<ItemUI>();

    private int nowPage = 0;

    public void Start()
    {
        Init(itemCount);
        RefreshToUI();
    }

    private Color GetColorFromState(PictureOrganizeTool.ProcessItemStateEnum state)
    {
        if (state == PictureOrganizeTool.ProcessItemStateEnum.Copied)
        {
            return Color.green;
        }
        if (state == PictureOrganizeTool.ProcessItemStateEnum.Skipped)
        {
            return Color.blue;
        }
        return Color.grey;
    }

    public void RefreshUI_FollowIndex(int indexNow, List<PictureOrganizeTool.ProcessItemState> states)
    {
        int page = Mathf.FloorToInt((float)indexNow / (float)items.Count);
        RefreshToUI(page, states);
    }

    public void RefreshToUI(int _page = 0, List<PictureOrganizeTool.ProcessItemState> states = null)
    {
        if (states == null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ClearData();
            }
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            int index = _page * items.Count + i;
            if (index < states.Count)
            {
                var data = states[index];
                items[i].SetData(data.index.ToString(), data.path, data.state.ToString(), GetColorFromState(data.state));
            }
            else
            {
                items[i].ClearData();
            }
        }
    }

    private void Init(int itemCount)
    {
        DestroyItems();

        for (int i = 0; i < itemCount; i++)
        {
            GameObject item1 = Instantiate<GameObject>(IntemPrefab);
            item1.transform.SetParent(parent);
            ItemUI itemUI = item1.GetComponent<ItemUI>();
            items.Add(itemUI);
        }
        nowPage = 0;
    }

    private void DestroyItems()
    {
        foreach (var item in items)
        {
            item.transform.SetParent(null);
            Destroy(item.gameObject);
        }
        items.Clear();
        nowPage = 0;
    }


    public void ShowOrHideThis()
    {
        root.SetActive(!root.activeSelf);
    }
    public void ShowThis()
    {
        root.SetActive(true);
    }
    public void Button_PrePage()
    {

    }
    public void Button_NextPage()
    {

    }
}
