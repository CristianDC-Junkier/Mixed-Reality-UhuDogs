using UnityEngine;

public class DogNeedsInteraction : MonoBehaviour
{
    [Header("References")]
    public DogStats dogStats;

    [Header("Amounts")]
    public float foodAmount = 30f;
    public float waterAmount = 30f;

    private void Start()
    {
        if (dogStats == null)
        {
            dogStats = FindFirstObjectByType<DogStats>();
        }
    }

    // =====================================================
    // FOOD
    // =====================================================
    public void FeedDog()
    {
        dogStats.FeedDog(foodAmount);
    }

    // =====================================================
    // WATER
    // =====================================================
    public void GiveWater()
    {
        dogStats.Drink(waterAmount);
    }
}