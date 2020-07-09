using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dataVisualizer : MonoBehaviour
{
    public CarAgent agent;
    public GameObject cumulativeRewardText;
    public GameObject episodesCompletedText;

    Text cumulativeReward;
    Text episodesCompleted;

    private void Start()
    {
        cumulativeReward = cumulativeRewardText.GetComponent<Text>();
        episodesCompleted = episodesCompletedText.GetComponent<Text>();
    }
    void Update()
    {
        cumulativeReward.text = "Cumulative Reward: " + agent.GetCumulativeReward().ToString();
        episodesCompleted.text = "Episodes Completed: " + agent.CompletedEpisodes.ToString();
    }
}
