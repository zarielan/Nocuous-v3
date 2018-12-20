using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    private void Update()
    {
        StaticClass.LEVEL_NUMBER = 1;
        if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene("GameScene");
    }
}
