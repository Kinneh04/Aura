using UnityEngine;
using UnityEngine.EventSystems;

public class SpinScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float spinSpeed = 50.0f; // Speed of rotation

    void Update()
    {
        // Rotate the object around its up axis (y-axis)
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private bool isDragging = false;
    private Vector3 offset;
    private float distanceToCamera;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main; // Assuming you have a main camera in your scene
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera);
        Vector3 objPos = mainCamera.ScreenToWorldPoint(mousePos);
        offset = transform.position - objPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera);
        Vector3 objPos = mainCamera.ScreenToWorldPoint(mousePos);
        transform.position = objPos + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
