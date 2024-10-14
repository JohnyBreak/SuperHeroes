using UnityEngine;

public class ZeroSetter : MonoBehaviour
{
    [SerializeField] private bool _hideCursor = true;

    private void Start()
    {
        if (_hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) 
        {
            transform.position = new Vector3(transform.position.x, 1.2f, transform.position.z);
        }
    }
}
