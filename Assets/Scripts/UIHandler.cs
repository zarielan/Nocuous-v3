﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  The class that handles how the inventory will be displayed on the upper-right corner.
 */ 
public class UIHandler : MonoBehaviour
{
    public GameObject PF_ItemSelect;
    private GameObject itemselect;

    private List<GameObject> children;
    private int selectedItem;

    private void Start()
    {
        children = new List<GameObject>();
        selectedItem = 0;
    }

    // When an item has been added.
    public void OnAddChild(GameObject obj)
    {
        GameObject holder = new GameObject();
        holder.transform.SetParent(transform);
        var rect = holder.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64, 64);
        holder.layer = 5;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        
        children.Add(holder);
        int index = children.Count;

        rect.anchoredPosition = new Vector2(-50 + 100 * index, -50);
        holder.name = "Item" + index;

        // Set its position to the upper-right corner
        /*
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
        obj.layer = 5;
        obj.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        obj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(50 - 100 * index, -50);
        */

        obj.transform.SetParent(holder.transform);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        var obj_rect = obj.GetComponent<RectTransform>();
        obj_rect.anchoredPosition = new Vector2(0, 0);
        obj_rect.sizeDelta = new Vector2(64, 64);
        obj_rect.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (index == 1)
        {
            itemselect = Instantiate(PF_ItemSelect, holder.transform);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && children.Count > 0)
            MoveSelectedItem(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) && children.Count > 1)
            MoveSelectedItem(1);
    }

    private void MoveSelectedItem(int index)
    {
        print("MOVE!");
        itemselect.transform.SetParent(children[index].transform);
        itemselect.transform.localPosition = new Vector3(0, 0, 0);
    }
}
