using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float horizontalVerticalSpeed;

    [SerializeField]
    float scrollSpeed;

    [SerializeField]
    float minDistanceToAnchor;

    [SerializeField]
    Transform cameraTransform;



    void Update()
    {
        CheckForInputs();
    }

    void CheckForInputs()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward*horizontalVerticalSpeed*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.back*horizontalVerticalSpeed*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left*horizontalVerticalSpeed*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right*horizontalVerticalSpeed*Time.deltaTime;
        }

        if(Input.mouseScrollDelta.y>0)
        {
            Vector3 direction = CalculateNormalizedDirection(cameraTransform.position,transform.position);

            cameraTransform.position += direction * scrollSpeed * Time.deltaTime;

            if(Vector3.Distance(cameraTransform.position, transform.position) < minDistanceToAnchor)
            {
                cameraTransform.position -= direction * scrollSpeed * Time.deltaTime;
            }
        }
        else if(Input.mouseScrollDelta.y<0)
        {
            Vector3 direction = CalculateNormalizedDirection(transform.position, cameraTransform.position);

            cameraTransform.position += direction * scrollSpeed * Time.deltaTime;
        }

    }

    Vector3 CalculateNormalizedDirection(Vector3 startPoint, Vector3 endPoint)
    {
        return endPoint - startPoint;
    }
}
