using UnityEngine;

public class disableCollider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void disabler()
    {
        this.GetComponent<CircleCollider2D>().enabled = false;
        
    }

    public void destroyer()
    {
        Destroy(this.gameObject);
    }
}
