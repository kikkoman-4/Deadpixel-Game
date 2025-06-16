using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] public Slider slider;

    public void UpdateHealthBar(float currenValue, float maxValue)
    {
        slider.value = currenValue / maxValue;
    }
}
