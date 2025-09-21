using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour
{
    public float speed=2;       
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        this.transform.position -= transform.up * Time.deltaTime * speed;


    }
}
