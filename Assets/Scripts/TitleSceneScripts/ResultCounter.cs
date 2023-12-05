using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultCounter : MonoBehaviour
{
    public static int numberofwin;
    public static int numberoflose;
    public static int numberofdraw;

    private void Awake()
    {
        int numResultCounter = FindObjectsOfType<ResultCounter>().Length;
        if(numResultCounter > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
