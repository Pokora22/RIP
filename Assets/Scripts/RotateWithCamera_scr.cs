using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithCamera_scr : MonoBehaviour
{
    private Camera cam;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveAndEnabled)
        {
            Vector3 direction = new Vector3(cam.transform.position.x, transform.position.y, cam.transform.position.z);
            transform.LookAt(direction);
        }
            
    }
}
