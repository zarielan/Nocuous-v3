using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryScene : MonoBehaviour
{
    public GameObject page1;
    public GameObject page2;

    private int page = 0;

    private void Start()
    {
        page1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        page2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        if (page == 0)
        {
            page1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            page2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0);
        }
        else if (page == 1)
        {
            page1.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
            page2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

            if (Input.GetKeyDown(KeyCode.Return))
                SceneManager.LoadScene("InstructionsScene");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            page++;
        }
    }
}
