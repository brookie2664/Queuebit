using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ToggleMenu();
    }

    private bool show = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ToggleMenu();
        }
    }

    public void ToggleMenu() {
        show = !show;
        foreach(Transform child in transform) {
            child.gameObject.SetActive(show);
        }
        GetComponent<Image>().enabled = show;
        if (!show) {
            PlayerPrefs.Save();
        }
    }
}
