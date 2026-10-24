using UnityEngine;
using UnityEngine.Tilemaps;

public class InfiniteTilemap2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Tilemap Loop Settings")]
    [Tooltip("The width of your tilemap layout in world units (e.g. 20 tiles = 20 units if cell size is 1).")]
    [SerializeField] private float tilemapWidth = 20f;

    private float startPosX;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        startPosX = transform.position.x;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Calculate how far the camera has moved relative to the tilemap start
        float offset = cameraTransform.position.x - startPosX;

        // When camera moves past the threshold width, snap the tilemap forward
        if (offset >= tilemapWidth)
        {
            startPosX += tilemapWidth;
            transform.position = new Vector3(startPosX, transform.position.y, transform.position.z);
        }
    }
}