using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Button hostButton;
    public Button clientButton;
    public TextMeshProUGUI flagsText;

    public static ConnectionManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        hostButton.onClick.AddListener(startHost);
        clientButton.onClick.AddListener(startClient);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void startClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void startHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void UpdateFlags(bool isHost, bool isClient)
    {
        flagsText.text = "IsHost : "+isHost+ "\n" + "IsClient : "+isClient;
    }
}
