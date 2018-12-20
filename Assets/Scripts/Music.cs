using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    private static Music musicInstance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (musicInstance == null)
            musicInstance = this;
        else
            Destroy(this.gameObject);
    }
}
