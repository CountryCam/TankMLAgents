using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public UnityEvent onDeath;
    public UnityEvent<int> onHealthChange;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        slider.value = CalculateHealthPercentage();
        onHealthChange.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            onDeath.Invoke();
        }
    }
    private float CalculateHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }  
}
