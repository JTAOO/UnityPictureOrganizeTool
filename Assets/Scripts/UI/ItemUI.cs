using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{

    public Image background;
    public TextMeshProUGUI index, path, state;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetData(string _index, string _path, string _state, Color _color)
    {
        index.text = _index;
        path.text = _path;
        state.text = _state;
        background.color = _color;
    }

    public void ClearData()
    {
        SetData("", "", "", Color.grey);
    }
}
