using UnityEngine;

public class ThrowBall : MonoBehaviour
{
    public void CountThrow()
    {
        int balls = PlayerPrefs.GetInt("nBalls", 0) + 1;
        PlayerPrefs.SetInt("nBalls", balls);
        PlayerPrefs.Save();
    }
}
