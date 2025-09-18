using UnityEngine;
using System.Collections;

public class SquareShootng : MonoBehaviour
{
    public float speed=2;
    public bool isRight = true;        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isRight)
        this.transform.position += transform.right * Time.deltaTime * speed;

        else
        this.transform.position -= transform.right * Time.deltaTime * speed;
    }
}
