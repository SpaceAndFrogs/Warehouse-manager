using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHiding : MonoBehaviour
{
    Vector3 startScale;
    [SerializeField] float scaleWhenHidden = 0.1f;
    void Awake()
    {
        startScale = transform.localScale;
    }
    void FixedUpdate() 
    {
        if(Input.GetKey(KeyCode.Tab))
        {
            transform.localScale = new Vector3(startScale.x, scaleWhenHidden, startScale.z);
        }else
        {
            transform.localScale = startScale;
        }
    }
}
