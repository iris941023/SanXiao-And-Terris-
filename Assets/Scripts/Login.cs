using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Login : MonoBehaviour
{
    public Button LoginButton;
    private void Start()
    {      
        // LoginButton.onClick.AddListener(StartClick);
    }
    public void StartClick()
    {
        SceneManager.LoadScene(1); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
