using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWiggle : MonoBehaviour
{
    private float deltaTime = 0f;

    private void Start()
    {
        deltaTime = Random.Range(0, 30);
    }

    private void Update()
    {
        deltaTime += Time.deltaTime * 2;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(deltaTime) * 15);
    }
}
