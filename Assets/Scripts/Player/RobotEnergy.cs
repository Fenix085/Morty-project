using UnityEngine;
using UnityEngine.UI;

public class RobotEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float consumptionRate = 15f;

    [Header("UI Elements")]
    public Slider energySlider;

    public bool HasEnergy => currentEnergy > 0;

    void Start()
    {
        currentEnergy = maxEnergy;
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = maxEnergy;
        }
    }

    public void UseEnergy()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= consumptionRate * Time.deltaTime;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }
    }
}