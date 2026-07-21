using UnityEngine;

public sealed class RtsCameraController : MonoBehaviour
{
    private Camera controlledCamera;
    private float mapHalfSize;
    private float moveSpeed;
    private float zoomSpeed;
    private float minSize;
    private float maxSize;

    public void Configure(
        Camera targetCamera,
        float mapWorldSize,
        float cameraMoveSpeed,
        float cameraZoomSpeed,
        float minimumSize,
        float maximumSize
    )
    {
        controlledCamera = targetCamera;
        mapHalfSize = mapWorldSize / 2f;
        moveSpeed = cameraMoveSpeed;
        zoomSpeed = cameraZoomSpeed;
        minSize = minimumSize;
        maxSize = maximumSize;

        if (controlledCamera == null)
        {
            Debug.LogError("No Main Camera found.");
            return;
        }

        controlledCamera.orthographic = true;
        controlledCamera.transform.position = new Vector3(0f, 0f, -10f);
        controlledCamera.orthographicSize = maxSize;
        controlledCamera.backgroundColor = new Color(0.08f, 0.08f, 0.09f);
    }

    public void Tick(float deltaTime)
    {
        if (controlledCamera == null)
        {
            return;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 position = controlledCamera.transform.position;
        position += new Vector3(input.x, input.y, 0f).normalized * moveSpeed * deltaTime;

        controlledCamera.orthographicSize = Mathf.Clamp(
            controlledCamera.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed,
            minSize,
            maxSize
        );

        float verticalExtent = controlledCamera.orthographicSize;
        float horizontalExtent = verticalExtent * controlledCamera.aspect;
        float maxX = Mathf.Max(0f, mapHalfSize - horizontalExtent);
        float maxY = Mathf.Max(0f, mapHalfSize - verticalExtent);
        position.x = Mathf.Clamp(position.x, -maxX, maxX);
        position.y = Mathf.Clamp(position.y, -maxY, maxY);
        position.z = -10f;
        controlledCamera.transform.position = position;
    }
}
