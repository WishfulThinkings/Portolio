using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalScore = 0;
    public TextMeshProUGUI scoreNumber;
    public BubbleMoveDown bubbleMoveDown;

    public GameObject pauseUI;
    public bool isPaused = false;
    public Animator transition;

    public void AddScore(int score)
    {
        int result = totalScore + score;
        scoreNumber.text = result.ToString();
        totalScore = result;
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseButton()
    {
        isPaused = !isPaused;
        if(isPaused == true)
        {
            Time.timeScale = 0;
        }
        else { Time.timeScale = 1; }
        pauseUI.SetActive(isPaused);
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void ExitToMainMenu()
    {
        StartCoroutine(Transition("Transition_In", 0));
    }

    private IEnumerator Transition(string name, int scene)
    {
        Time.timeScale = 1;
        transition.Play(name);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(scene);
    }

    public void StartGame()
    {
        StartCoroutine(Transition("Transition_In", 1));
    }
}
