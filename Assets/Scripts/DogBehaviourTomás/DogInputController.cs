using UnityEngine;

public class DogInputController : MonoBehaviour
{
    [Header("References")]
    public DogStateMachineNew stateMachine;

    private void Start()
    {
        if (stateMachine == null)
        {
            stateMachine = FindFirstObjectByType<DogStateMachineNew>();
        }
    }

    private void Update()
    {
        // =================================================
        // CALL DOG
        // A BUTTON
        // =================================================
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            stateMachine.CallDog();
        }

        // =================================================
        // FETCH BALL
        // B BUTTON
        // =================================================
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            stateMachine.RequestBall();
        }
    }
}
