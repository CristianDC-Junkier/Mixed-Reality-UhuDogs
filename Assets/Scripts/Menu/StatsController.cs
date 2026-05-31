using TMPro;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    public TMP_Text nFeeds;
    public TMP_Text nBalls;
    public TMP_Text nWalks;
    public TMP_Text nPoops;
    public TMP_Text distance;
    
    public AudioSource clip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int feeds = PlayerPrefs.GetInt("nFeeds", 0);
        int balls = PlayerPrefs.GetInt("nBalls", 0);
        int walks = PlayerPrefs.GetInt("nWalks", 0);
        int poops = PlayerPrefs.GetInt("nPoops", 0);
        float dist = PlayerPrefs.GetFloat("distance", 0f);

        nFeeds.text = feeds.ToString();
        nBalls.text = balls.ToString();
        nWalks.text = walks.ToString();
        nPoops.text = poops.ToString();
        distance.text = dist.ToString("F2");
    }

    public void PlayClick()
    {
        clip.Play();
    }
}
