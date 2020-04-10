using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class IPPanel : MonoBehaviour
{
    [SerializeField]
    private Text ipText;
    [SerializeField]
    private GameState gameState;

    private bool show = true;

    // Start is called before the first frame update
    void Start()
    {
        string hostName = System.Net.Dns.GetHostName();
        string localIP = System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString();
        ipText.text = localIP;
    }

    // Update is called once per frame
    void Update()
    {
        if (show && gameState.IsGameLoaded()) {
            show = false;
            gameObject.SetActive(false);
        }
    }
}
