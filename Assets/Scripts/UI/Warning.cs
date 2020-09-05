using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Warning : MonoBehaviour
{
    public static Warning warning {get; private set;}

    public GameObject messageObject;
    public float displayTime = .5f;
    public float fadeTime = .5f;
    
    private string message;
    private float boxHeight;
    private float timeSinceLastMessage;

    public void ShowMessage(string message) {
        this.message = message;
        messageObject.GetComponent<Text>().text = message;
        timeSinceLastMessage = 0;
        gameObject.SetActive(true);
    }

    void Awake() {
        if (warning != null) Destroy(warning);
        warning = this;
        gameObject.SetActive(false);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        boxHeight =  GetComponent<RectTransform>().anchoredPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastMessage += Time.deltaTime;
        Vector2 pos = GetComponent<RectTransform>().anchoredPosition;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x,
                timeSinceLastMessage < displayTime ?
                boxHeight :
                (timeSinceLastMessage - displayTime) * System.Math.Abs(boxHeight * 2 / fadeTime) + boxHeight);
    }
}
