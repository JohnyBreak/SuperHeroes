using UnityEngine;

public class ZeroSetter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) 
        {
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
    }
}
