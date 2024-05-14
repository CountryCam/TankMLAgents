using UnityEngine;
using UnityEngine.UI;
public class PlayerUI : MonoBehaviour
{
    public Slider healthSlider;
    public void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = 100;  // Set this to the same max health as your Health script.
            healthSlider.value = healthSlider.maxValue;
        }
    }
    public void UpdateHealth(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}