using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class score : MonoBehaviour
{
    public static int show_score = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateScore()
    {
        show_score += 1000;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), show_score.ToString());
    }

}
