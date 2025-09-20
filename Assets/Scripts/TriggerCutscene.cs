using UnityEngine;

public class TriggerCutscene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.GetComponent<Animator>().SetTrigger("HoleCutscene");
    }
}
