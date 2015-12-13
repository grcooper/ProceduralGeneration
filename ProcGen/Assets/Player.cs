using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    Rigidbody rigidbody;
    Vector3 velocity;
    Quaternion rotation;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * 10;
        GameObject mainCamera = GameObject.Find("Head");
        if (mainCamera)
        {
            rotation = mainCamera.transform.rotation;
        }
        else
        {
            rotation = transform.rotation;
        }
    }

    void FixedUpdate()
    {

        rigidbody.rotation = rotation;
        rigidbody.MovePosition(rigidbody.position + (transform.forward * velocity.z * Time.fixedDeltaTime));
        rigidbody.MovePosition(rigidbody.position + (transform.right * velocity.x * Time.fixedDeltaTime));
    }
}