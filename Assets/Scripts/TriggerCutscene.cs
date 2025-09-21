using UnityEngine;

public class TriggerCutscene : MonoBehaviour
{
    public Animator anim;
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
        other.gameObject.transform.GetChild(3).SetParent(anim.transform);
        //Destroy(other.gameObject);
        anim.SetTrigger("HoleCutscene");
    }
}
