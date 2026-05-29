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

            int poops = PlayerPrefs.GetInt("nPoops", 0) + 1;
            PlayerPrefs.SetInt("nPoops", poops);
            PlayerPrefs.Save();

            if (dog != null)
            {
                dog.RemovePoopFromCount();
            }

            Destroy(gameObject);
        }
    }
}