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
    float rotationSpeed;

    [SerializeField]
    float minDistanceToAnchor;

    [SerializeField]
    Transform cameraTransform;


    Vector2 lastMousePosition;
    void Update()
    {
        CheckForInputs();
    }

    void CheckForInputs()
    {
        CheckAxisMovement();

        CheckScroll();

        CheckRotation();
    }

    Vector3 CalculateNormalizedDirection(Vector3 startPoint, Vector3 endPoint)
    {
        return endPoint - startPoint;
    }

    void CheckAxisMovement()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward*horizontalVerticalSpeed*Time.unscaledDeltaTime;
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.back*horizontalVerticalSpeed*Time.unscaledDeltaTime;
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left*horizontalVerticalSpeed*Time.unscaledDeltaTime;
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right*horizontalVerticalSpeed*Time.unscaledDeltaTime;
        }
    }

    void CheckScroll()
    {
        if(Input.mouseScrollDelta.y>0)
        {
            Vector3 direction = CalculateNormalizedDirection(cameraTransform.position,transform.position);

            cameraTransform.position += direction * scrollSpeed * Time.unscaledDeltaTime;

            if (Vector3.Distance(cameraTransform.position, transform.position) < minDistanceToAnchor)
            {
                cameraTransform.position -= direction * scrollSpeed * Time.unscaledDeltaTime;
            }
        }
        else if(Input.mouseScrollDelta.y<0)
        {
            Vector3 direction = CalculateNormalizedDirection(transform.position, cameraTransform.position);

            cameraTransform.position += direction * scrollSpeed * Time.unscaledDeltaTime;
        }

        
    }

    void CheckRotation()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector2 currentMousePosition = Input.mousePosition;

            if (currentMousePosition.x != lastMousePosition.x)
            {
                transform.eulerAngles += new Vector3(0, currentMousePosition.x - lastMousePosition.x, 0) * rotationSpeed * Time.unscaledDeltaTime;
            }

            lastMousePosition = currentMousePosition;
        }
    }
}
