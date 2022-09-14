using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    public Button LoginButton;
    

    private void Awake()
    {
        LoginButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("San Match");
        });
    }   
    public void RestartGame()
    {
        SceneManager.LoadScene("San Match-login");
    }
}
