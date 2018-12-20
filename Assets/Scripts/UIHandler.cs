using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  The class that handles how the inventory will be displayed on the upper-right corner.
 */ 
public class UIHandler : MonoBehaviour
{
    public GameObject PF_ItemSelect;
    private GameObject itemselect;

    private List<GameObject> children;
    private List<GameObject> childrenGhost;

    private Image fader;
    private bool acceptingInputs;
    private int selectedItem = 0;
    private Vector3 ghostItemPosition;

    private void Start()
    {
        children = new List<GameObject>();
        childrenGhost = new List<GameObject>();
        acceptingInputs = true;

        fader = transform.Find("Fader").gameObject.GetComponent<Image>();
        ghostItemPosition = Vector3.zero;

        itemselect = Instantiate(PF_ItemSelect, transform);
        itemselect.GetComponent<Image>().canvasRenderer.SetAlpha(0f);
    }

    // When an item has been added.
    public void OnAddChild(GameObject obj)
    {
        // Create a holder GameObject, which will contain both the item and the select icon
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

        obj.transform.SetParent(holder.transform);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        var obj_rect = obj.GetComponent<RectTransform>();
        obj_rect.anchoredPosition = new Vector2(0, 0);
        obj_rect.sizeDelta = new Vector2(64, 64);
        obj_rect.transform.rotation = Quaternion.Euler(0, 0, 0);

        var ghost = Instantiate(obj);
        Destroy(ghost.GetComponent<SpriteRenderer>());
        Destroy(ghost.GetComponent<CircleCollider2D>());
        ghost.GetComponent<Item>().SetRotating(false);
        ghost.GetComponent<Image>().canvasRenderer.SetColor(new Color(1, 1, 1, 0f));
        ghost.transform.SetParent(transform);
        ghost.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
        ghost.GetComponent<RectTransform>().position = new Vector2(0, 0);
        ghost.name = "Ghost" + index;
        childrenGhost.Add(ghost);

        if (index == 1)
        {            
            MoveSelectedItem(0, false);
        }

        if (children.Count > 0)
            itemselect.GetComponent<Image>().canvasRenderer.SetAlpha(1f);
    }

    private void Update()
    {
        if (acceptingInputs)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && children.Count > 0)
                MoveSelectedItem(0);

            if (Input.GetKeyDown(KeyCode.Alpha2) && 1 < children.Count)
                MoveSelectedItem(1);
        }

        if (childrenGhost.Count > 0)
        {
            GameObject ghost = childrenGhost[selectedItem];
            ghost.GetComponent<RectTransform>().position = ghostItemPosition;
        }
    }

    private void MoveSelectedItem(int index, bool remove = true)
    {
        if (remove)
        {
            var ghostbefore = childrenGhost[selectedItem];
            ghostbefore.GetComponent<Image>().canvasRenderer.SetAlpha(0f);
        }

        selectedItem = index;

        GameObject ghost = childrenGhost[selectedItem];
        ghost.GetComponent<Image>().canvasRenderer.SetAlpha(0.5f);

        itemselect.transform.position = children[index].transform.position;
    }

    public void FadeToBlack(float time)
    {
        fader.transform.SetAsLastSibling();
        fader.canvasRenderer.SetAlpha(0f);
        fader.CrossFadeAlpha(1f, time, false);
    }

    public void FadeToGame(float time)
    {
        fader.transform.SetAsLastSibling();
        fader.canvasRenderer.SetAlpha(1f);
        fader.CrossFadeAlpha(0f, time, false);
    }

    public GameObject UseItem()
    {
        GameObject current = children[selectedItem];
        return current;        
    }

    public void OnRemoveChild()
    {
        int y = selectedItem;
        var currentItem = children[y];
        var currentGhost = childrenGhost[y];

        children.RemoveAt(selectedItem);
        childrenGhost.RemoveAt(selectedItem);
        
        var x = selectedItem;
        while (x >= children.Count)
        {
            x--;            
        }

        if (x < 0)
        {
            itemselect.GetComponent<Image>().canvasRenderer.SetAlpha(0f);
            x = 0;
        }

        for (int i = 0; i < children.Count; i++)
            children[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-50 + 100 * i, -50);

        Destroy(currentItem);
        Destroy(currentGhost);

        MoveSelectedItem(x, false);
 
    }

    public void SetAcceptingInputs(bool arewe)
    {
        acceptingInputs = arewe;
    }

    public void SetGhostItemPosition(Vector3 v)
    {
        ghostItemPosition = v;
    }
}
