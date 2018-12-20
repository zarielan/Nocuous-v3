using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private float deltaTime = 0f;
    private bool rotating = true;

    void Start()
    {
        deltaTime = Random.Range(0, 30);
    }

    void Update()
    {
        if (rotating)
        {
            deltaTime += Time.deltaTime * 2;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(deltaTime) * 15);
        }
    }

    public bool UseItem(LevelScript lvl, GameObject inthisroom)
    {
        bool allUsedUp = false;

        if (name == lvl.GetPrefabName(lvl.PF_FireExtinguisher))
        {
            Transform fire = inthisroom.transform.Find(lvl.GetPrefabName(lvl.PF_Fire));

            if (fire != null)
                Destroy(fire.gameObject);

            // Decrease extinguisher use here TODO
        }
        else if (name == lvl.GetPrefabName(lvl.PF_Plank))
        {
            Transform hole = inthisroom.transform.Find(lvl.GetPrefabName(lvl.PF_Hole));

            if (hole != null)
            {
                lvl.CreateElementInRoom(lvl.PF_PlankBridge, inthisroom);

                allUsedUp = true;
            }
        }

        return allUsedUp;
    }

    public void SetRotating(bool rot)
    {
        rotating = rot;
    }
}
