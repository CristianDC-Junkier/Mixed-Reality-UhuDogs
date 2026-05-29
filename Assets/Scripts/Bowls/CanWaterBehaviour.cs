using UnityEngine;

public class CanWaterBehaviour : MonoBehaviour
{
    [Header("Configuración de Comida")]
    public GameObject waterObjectToShow;

    [Header("Re-instanciación (Prefab)")]
    // Arrastra aquí el PREFAB de esta misma lata desde tu carpeta de Assets
    public GameObject bottlePrefab;

    // Posición y rotación donde quieres que vuelva a aparecer
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool used;

    private void Start()
    {
        // Guardamos la posición inicial de esta instancia

        if(PlayerPrefs.GetInt("water", 1) < 1){
            waterObjectToShow.SetActive(false);
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used)
            return;

        if (other.CompareTag("waterbowl"))
        {
            used = true;

            if (waterObjectToShow != null)
            {
                waterObjectToShow.SetActive(true);
            }

            PlayerPrefs.SetInt("water", 1);
            PlayerPrefs.Save();

            // 1. Creamos una copia nueva en el lugar original
            if (bottlePrefab != null)
            {
                Instantiate(bottlePrefab, initialPosition, initialRotation);
            }

            // 2. Destruimos la lata actual (así la mano de Oculus la suelta sí o sí)
            Destroy(gameObject);
        }
    }
}