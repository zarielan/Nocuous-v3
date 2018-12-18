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
        print("added children!");
    }
}
