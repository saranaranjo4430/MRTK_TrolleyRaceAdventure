using UnityEngine;
using UnityEngine.UI;

public class SliderSelector : MonoBehaviour
{
    public Slider carSlider;               // Reference to the slider
    public GameObject[] carPreviews;      // Array of car preview objects
    private int currentIndex = 0;

    void Start()
    {
        // Ensure all previews are hidden except the first
        UpdatePreview((int)carSlider.value);
        carSlider.onValueChanged.AddListener(delegate { UpdatePreview((int)carSlider.value); });
    }

    public void UpdatePreview(int index)
    {
        // Hide all previews
        for (int i = 0; i < carPreviews.Length; i++)
        {
            carPreviews[i].SetActive(i == index);
        }
        currentIndex = index;
        Debug.Log("Selected Car: " + index);
    }

    public int GetCurrentSelection()
    {
        return currentIndex;
    }
}
