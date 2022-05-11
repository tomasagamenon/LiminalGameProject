using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    private Settings settings;
    private void Awake()
    {
        settings = GetComponent<Settings>();
        settings.LoadValues();
    }
    private void Start()
    {
        SceneManager.LoadScene(1);
    }
}
