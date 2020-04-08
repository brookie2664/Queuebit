using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public static Countdown countdown {get; private set;}

    public GameObject messageObject;
    public GameObject timerObject;

    private float timer;
    private string message;

    public void StartCountdown(string message, float timer) {
        this.message = message;
        this.timer = timer;
        gameObject.SetActive(true);
        messageObject.GetComponent<Text>().text = message;
        timerObject.GetComponent<Text>().text = System.Math.Ceiling(timer).ToString();
    }

    void Awake() {
        if (countdown == null) {
            countdown = this;
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer != 0) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                timer = 0;
                gameObject.SetActive(false);
            } else {
                timerObject.GetComponent<Text>().text = System.Math.Ceiling(timer).ToString();
            }
        }
    }
}
