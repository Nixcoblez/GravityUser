using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XYZNavController : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 targetNormalRotation;
    private Quaternion targetRotation ;

    public float positionSpeed = 200f;
    public float rotationSpeed = 20f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Slerp(transform.position, targetPosition, positionSpeed*Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed*Time.deltaTime);
    }

    public void setTarget(Vector3 position, Vector3 surfaceNormal){
        targetPosition = position;
        targetNormalRotation = surfaceNormal;
        targetRotation = Quaternion.FromToRotation(Vector3.up, targetNormalRotation);        
    }

    public void applyPosition()
    {
        transform.position = targetPosition;
        transform.rotation = targetRotation;

    }

    public void hide()
    {
        gameObject.SetActive(false);

    }

    public void show()
    {
        gameObject.SetActive(true);
    }

    public void showAt(Vector3 position, Vector3 surfaceNormal)
    {
        setTarget(position, surfaceNormal);
        applyPosition();
        gameObject.SetActive(true);
    }



    public void hideNav(){
        //targetPosition = new Vector3();
    }

}
