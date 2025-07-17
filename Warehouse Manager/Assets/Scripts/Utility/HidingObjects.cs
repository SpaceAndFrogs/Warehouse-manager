using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingObjects : MonoBehaviour
{
    [SerializeField]
    MeshRenderer[] meshRenderer = new MeshRenderer[0];
    [SerializeField]
    bool canMove;
    [SerializeField]
    float posTolerance = 0.1f;
    Vector3 lastCheckPos;

    void OnEnable() {
        CameraScript.OnCameraMove += CheckHide;
    }

    void OnDisable() {
        CameraScript.OnCameraMove -= CheckHide;
    }

    void Start()
    {
        lastCheckPos = transform.position;
        CheckHide();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            if (Vector3.Distance(lastCheckPos, transform.position) > posTolerance)
            {
                lastCheckPos = transform.position;
                CheckHide();
            }
        }
    }
    
    void CheckHide()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraScript.instance.cameraToCheck);

        foreach (MeshRenderer mesh in meshRenderer)
        {
            Bounds bounds = mesh.bounds;

            if (GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                mesh.enabled = true;
            }
            else
            {
                mesh.enabled = false;
            }
        }
        
    }
}
