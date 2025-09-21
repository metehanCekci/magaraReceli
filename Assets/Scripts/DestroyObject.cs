using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float timing = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(this.gameObject,timing);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
