using UnityEngine;

public class Billboard2D : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}