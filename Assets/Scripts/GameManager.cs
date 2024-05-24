using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI winText; // UI Text to display "You Won"
    public Button menuButton; // Button to return to the main menu
    public SmoothTank[] tanks; // Array to hold references to all tank instances

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        winText.gameObject.SetActive(false); // Hide the win text at the start
        menuButton.gameObject.SetActive(false); // Hide the menu button at the start

        // Assign the OnClick event to the menu button
        menuButton.onClick.AddListener(ReturnToMenu);
    }

    // Call this method to check if all tanks are destroyed
    public void CheckTanks()
    {
        foreach (var tank in tanks)
        {
            if (tank != null)
            {
                return; // If any tank is still alive, do nothing
            }
        }

        // If all tanks are destroyed
        winText.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    // Placeholder method to return to the main menu
    public void ReturnToMenu()
    {
        // Load the main menu scene 
        SceneManager.LoadScene(0);
    }

    // Method to be called when a tank is destroyed
    public void TankDestroyed(SmoothTank tank)
    {
        // Remove the destroyed tank from the array
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i] == tank)
            {
                tanks[i] = null;
                break;
            }
        }

        CheckTanks(); // Check if all tanks are destroyed
    }
}
