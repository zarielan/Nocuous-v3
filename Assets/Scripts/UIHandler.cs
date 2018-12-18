using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    private List<GameObject> children;

    private void Start()
    {
        children = new List<GameObject>();
    }

    public void OnAddChild(GameObject obj)
    {
        children.Add(obj);
        int index = children.Count;
                
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
        obj.layer = 5;
        obj.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        obj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(50 - 100 * index, -50);

        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
