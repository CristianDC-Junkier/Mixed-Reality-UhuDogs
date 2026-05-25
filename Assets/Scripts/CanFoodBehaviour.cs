using UnityEngine;

public class CanFoodBehaviour : MonoBehaviour
{
    public GameObject foodObjectToShow;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used)
            return;

        if (other.CompareTag("foodbowl"))
        {
            used = true;

            if (foodObjectToShow != null)
            {
                foodObjectToShow.SetActive(true);
            }

            Destroy(gameObject);
        }
    }
}