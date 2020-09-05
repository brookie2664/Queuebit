using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour
{
    public static Results results {get; private set;}

    public struct TeamScore {
        public int score;
        public Color color;

        public TeamScore(int score, Color color) {
            this.score = score;
            this.color = color;
        }
    }

    [SerializeField]
    private GameObject[] resultBars;
    [SerializeField]
    private GameObject winnerText;
    
    private TeamScore[] teams;
    private int totalScores = 0;

    [SerializeField]
    private float fillDelay = 1.5f;
    [SerializeField]
    private float colorDelay = 1f;
    private float timeStarted;
    private bool fillStarted = false;
    [SerializeField]
    private float fillTime = 1f;
    private float fillTimeSpent = 0;
    private bool colorStarted = false;
    [SerializeField]
    private float colorTime = .5f;
    private float colorTimeSpent = 0;

    private Color winnerColor = Color.white;
    private string winnerMessage = "";
    
    void Awake() {
        if (results != null) Destroy(results);
        results = this;
        gameObject.SetActive(false);
    }

    public float Bezier(float value, int a) {
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(1 - value, a));
    }

    public void TriggerResults(TeamScore[] scores) {
        gameObject.SetActive(true);
        teams = new TeamScore[scores.Length];
        for (int i = 0; i < scores.Length; i++) {
            teams[i] = scores[i];
            totalScores += scores[i].score;
        }
        for (int i = 1; i < teams.Length; i++) {
            int j = i - 1;
            while (j > -1 && teams[j].score < teams[j + 1].score) {
                TeamScore temp = teams[j];
                teams[j] = teams[j + 1];
                teams[j + 1] = temp;
                j--;
            }
        }
        if (teams.Length > 1 && teams[0].score == teams[1].score) {
            winnerMessage = "It's a Tie!";
        } else {
            winnerColor = teams[0].color;
            if (winnerColor == Color.red) {
                winnerMessage = "Red Wins!";
            } else if (winnerColor == Color.green) {
                winnerMessage = "Green Wins!";
            } else if (winnerColor == Color.blue) {
                winnerMessage = "Blue Wins!";
            } else {
                winnerMessage = "???";
                winnerColor = Color.white;
            }
        }

        timeStarted = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!fillStarted && Time.time - timeStarted > fillDelay) {
            fillStarted = true;
        }
        if (!colorStarted && Time.time - timeStarted > fillDelay + fillTime + colorDelay) {
            colorStarted = true;
            Text t = winnerText.GetComponent<Text>();
            t.text = winnerMessage;
            t.color = winnerColor;
        }
        if (fillStarted) {
            fillTimeSpent += Time.deltaTime;
            for (int i = 0; i < teams.Length && i < resultBars.Length; i++) {
                float perc = Bezier(Mathf.Min(fillTimeSpent / fillTime, 1), 4);
                resultBars[i].GetComponent<Slider>().value = Mathf.Lerp(0, (float) teams[i].score / totalScores, perc);
            }
        }
        if (colorStarted) {
            colorTimeSpent += Time.deltaTime;
            for (int i = 0; i < teams.Length && i < resultBars.Length; i++) {
                float perc = Bezier(Mathf.Min(colorTimeSpent / colorTime, 1), 4);
                resultBars[i].GetComponent<Slider>().fillRect.GetComponent<Image>().color = Color.Lerp(Color.white, teams[i].color, perc);
            }
        }
    }
}
