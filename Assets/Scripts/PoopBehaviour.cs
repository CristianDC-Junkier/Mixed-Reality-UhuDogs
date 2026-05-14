using UnityEngine;

public class PoopBehaviour : MonoBehaviour
{
    public DogParkAI dog;

    private bool destroyed;

    private void OnTriggerEnter(Collider other)
    {
        if (destroyed)
            return;

        if (other.CompareTag("trashbin"))
        {
            destroyed = true;

            if (dog != null)
            {
                dog.RemovePoopFromCount();
            }

            Destroy(gameObject);
        }
    }
}