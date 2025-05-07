using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraScript : MonoBehaviour
{
    public static event Action OnCameraMove;
    public static CameraScript instance;
    public Camera cameraToCheck;
    [SerializeField]
    float posTolerance = 0.1f;
    [SerializeField]
    float rotationTolerance = 0.1f;
    [SerializeField]
    float scrollTolerance = 0.1f;

    Vector3 lastCheckCameraPos;
    Vector3 lastCheckCameraRotation;
    Vector3 lastCheckCameraScroll;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        lastCheckCameraPos = transform.position;
        lastCheckCameraRotation = transform.rotation.eulerAngles;
        lastCheckCameraScroll = cameraToCheck.transform.position;
    }
    
    void FixedUpdate() 
    {
        MoveCheck();
        RotationCheck();
        ScrollCheck();
    }

    void MoveCheck()
    {
        if (Vector3.Distance(lastCheckCameraPos,transform.position) > posTolerance)
        {
            lastCheckCameraPos = transform.position;
            OnCameraMove?.Invoke();
        }

    }

    void RotationCheck()
    {
        if (Mathf.Abs(lastCheckCameraPos.y - transform.rotation.eulerAngles.y) > rotationTolerance)
        {
            lastCheckCameraRotation = transform.rotation.eulerAngles;
            OnCameraMove?.Invoke();
        }
    }

    void ScrollCheck()
    {
        if (Vector3.Distance(cameraToCheck.transform.position, lastCheckCameraScroll) > scrollTolerance)
        {
            lastCheckCameraScroll = cameraToCheck.transform.position;
            OnCameraMove?.Invoke();
        }
    }
}
