using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEntityPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveByDirection(Vector3 direction, float speed = 0)
    {

        transform.Translate(direction * speed);
    }
}
