using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    private Vector3 dragOffset;
    private float zPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        zPosition = transform.position.z;

        dragOffset = transform.position - GetMouseWorldPosition();
        Debug.Log(gameObject.name + "をクリック ");
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = GetMouseWorldPosition() + dragOffset;
        newPosition.z = zPosition;
        transform.position = newPosition;
    }

    private void OnMouseUp()
    {
        Debug.Log(gameObject.name + "のドラッグ終了");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.nearClipPlane + 10f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
