using UnityEngine;

public class CameraTargetPosition : MonoBehaviour
{
    public void ToggleWallPosition(bool wall)
    {
        transform.localPosition = new Vector3(0, (wall) ? 0.22f : 1.66f, (wall) ? 0.5f : 0);
    }
}
