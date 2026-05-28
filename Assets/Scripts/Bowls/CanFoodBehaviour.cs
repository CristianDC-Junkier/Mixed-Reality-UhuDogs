using UnityEngine;

public class CanFoodBehaviour : MonoBehaviour
{
    [Header("Configuración de Comida")]
    public GameObject foodObjectToShow;

    [Header("Re-instanciación (Prefab)")]
    // Arrastra aquí el PREFAB de esta misma lata desde tu carpeta de Assets
    public GameObject canPrefab;

    // Posición y rotación donde quieres que vuelva a aparecer
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool used;

    private void Start()
    {
        // Guardamos la posición inicial de esta instancia
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

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

            // 1. Creamos una copia nueva en el lugar original
            if (canPrefab != null)
            {
                Instantiate(canPrefab, initialPosition, initialRotation);
            }

            // 2. Destruimos la lata actual (así la mano de Oculus la suelta sí o sí)
            Destroy(gameObject);
        }
    }
}