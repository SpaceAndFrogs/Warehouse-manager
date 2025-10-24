using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    Coroutine movementLeft = null;
    Coroutine movementRight = null;
    Coroutine movementForward = null;
    Coroutine movementBackward = null;
    enum MovementDirection
    {
        Left,
        Right,
        Forward,
        Backward
    }
    Vector2 lastMousePosition;
    void Start()
    {
        HotkeysManager.OnKeyPressed += HandleKeyPress;
        HotkeysManager.OnKeyReleased += HandleKeyRelease;
    }   
    void Update()
    {
        CheckForInputs();
    }

    void CheckForInputs()
    {
        CheckScroll();

        CheckRotation();
    }

    Vector3 CalculateNormalizedDirection(Vector3 startPoint, Vector3 endPoint)
    {
        return endPoint - startPoint;
    }
    void HandleKeyRelease(KeyCode kcode)
    {
        if(kcode == KeyCode.A)
        {
            if(movementLeft != null)
            {
                StopCoroutine(movementLeft);
                movementLeft = null;
            }
        }
        else if(kcode == KeyCode.D)
        {
            if(movementRight != null)
            {
                StopCoroutine(movementRight);
                movementRight = null;
            }
        }
        else if(kcode == KeyCode.W)
        {
            if(movementForward != null)
            {
                StopCoroutine(movementForward);
                movementForward = null;
            }
        }
        else if(kcode == KeyCode.S)
        {
            if(movementBackward != null)
            {
                StopCoroutine(movementBackward);
                movementBackward = null;
            }
        }
    }
    void HandleKeyPress(KeyCode kcode)
    {
        if(kcode == KeyCode.A)
        {
            if(movementLeft == null)
            {
                movementLeft = StartCoroutine(MoveInDirection(MovementDirection.Left));
            }
        }
        else if(kcode == KeyCode.D)
        {
            if(movementRight == null)
            {
                movementRight = StartCoroutine(MoveInDirection(MovementDirection.Right));
            }
        }
        else if(kcode == KeyCode.W)
        {
            if(movementForward == null)
            {
                movementForward = StartCoroutine(MoveInDirection(MovementDirection.Forward));
            }
        }
        else if(kcode == KeyCode.S)
        {
            if(movementBackward == null)
            {
                movementBackward = StartCoroutine(MoveInDirection(MovementDirection.Backward));
            }
        }
    }
    
    IEnumerator MoveInDirection(MovementDirection direction)
    {
        while (true)
        {
            // movement relative to camera's local axes, flattened to XZ so vertical camera tilt doesn't affect movement
            Vector3 camRight = transform.right;
            Vector3 camForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            Vector3 move = Vector3.zero;
            switch (direction)
            {
                case MovementDirection.Left:
                    move = -camRight;
                    break;
                case MovementDirection.Right:
                    move = camRight;
                    break;
                case MovementDirection.Forward:
                    move = camForward;
                    break;
                case MovementDirection.Backward:
                    move = -camForward;
                    break;
            }

            transform.position += move * horizontalVerticalSpeed * Time.unscaledDeltaTime;

            yield return new WaitForEndOfFrame();
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
        // set the last mouse position once when the middle button is pressed
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        // while middle mouse button is held, rotate based on horizontal mouse delta
        if (Input.GetMouseButton(2))
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastMousePosition;

            if (Mathf.Abs(delta.x) > 0f)
            {
                transform.eulerAngles += new Vector3(0f, delta.x, 0f) * rotationSpeed * Time.unscaledDeltaTime;
            }

            lastMousePosition = currentMousePosition;
        }
    }
}
