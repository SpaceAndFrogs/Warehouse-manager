using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingObjects : MonoBehaviour
{
    [SerializeField]
    MeshRenderer[] meshRenderer = new MeshRenderer[0];

    void OnEnable() {
        CameraScript.OnCameraMove += CheckHide;
    }

    void OnDisable() {
        CameraScript.OnCameraMove -= CheckHide;
    }
    
    void CheckHide()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraScript.instance.cameraToCheck);

        foreach (MeshRenderer mesh in meshRenderer)
        {
            Bounds bounds = GetComponent<Renderer>().bounds;

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
