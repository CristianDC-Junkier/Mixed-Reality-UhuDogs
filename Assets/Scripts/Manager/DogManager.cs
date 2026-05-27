using UnityEngine;

public class DogStateMachine : MonoBehaviour
{
    [Header("Necesidades")]
    public float hunger = 1f;
    public float thirst = 1f;
    public float bladder = 1f;
    public float energy = 1f;

    [Header("Velocidades de desgaste")]
    public float hungerDecay = 0.01f;
    public float thirstDecay = 0.017f;
    public float bladderDecay = 0.015f;
    public float energyDecay = 0.02f;

    private void Update()
    {
        // Desgaste progresivo por tiempo
        hunger -= hungerDecay * Time.deltaTime;
        thirst -= thirstDecay * Time.deltaTime;
        bladder -= bladderDecay * Time.deltaTime;
        energy -= energyDecay * Time.deltaTime;

        // Limitar valores
        hunger = Mathf.Clamp01(hunger);
        thirst = Mathf.Clamp01(thirst);
        bladder = Mathf.Clamp01(thirst);
        energy = Mathf.Clamp01(energy);
    }

    public void RestoreHunger()
    {
        hunger = 1f;
    }

    public void RestoreThirst()
    {
        thirst = 1f;
    }

    public void RestoreBladder()
    {
        bladder = 1f;
    }

    public void RestoreEnergy()
    {
        energy = 1f;
    }
}