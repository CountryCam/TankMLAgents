using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public Button menuButton; // Button to return to the main menu
    public Button exitButton; // Button to exit the game

    void Start()
    {
        menuButton.gameObject.SetActive(true); // show menu button at the start
        exitButton.gameObject.SetActive(true); // show exit button at the start 

        // Assign the OnClick event to the menu button
        menuButton.onClick.AddListener(ReturnToGame);
        exitButton.onClick.AddListener(ExitToWindows);
    }
    public void ReturnToGame()
    {
        // Load the main game scene 
        SceneManager.LoadScene(1);
    }
    public void ExitToWindows()
    {
        Application.Quit();
    }
}
