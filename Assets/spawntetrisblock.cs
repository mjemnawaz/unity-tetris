using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawntetrisblock : MonoBehaviour
{

    public GameObject[] blocks;

    // Start is called before the first frame update
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    public void Spawn()
    {
        Instantiate(blocks[Random.Range(0,blocks.Length)],transform.position,Quaternion.identity);
    }
}
