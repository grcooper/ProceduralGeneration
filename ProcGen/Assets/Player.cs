using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    Rigidbody rigidbody;
    Vector3 velocity;
    Quaternion rotation;
    Vector3 target;
    public int speed = 1;
    bool hasTarget = false;

    Vector3 zero = new Vector3(0, 0, 0);

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rotation = rigidbody.rotation;
        target = rigidbody.position;
        OnEnabled();
    }

    void Update()
    {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * 10;
        if( velocity != zero )
        {
            target = rigidbody.position;
        }
        
        GameObject mainCamera = GameObject.Find("Head");
        if (mainCamera && !hasTarget)
        {
            rotation = mainCamera.transform.rotation;
        }
        else
        {
            rotation = rigidbody.rotation;
        }
    }

    void FixedUpdate()
    {

        rigidbody.rotation = rotation;
        
        if ( hasTarget )
        {
            velocity = Vector3.MoveTowards(rigidbody.position, target, 0.1f).normalized;
            velocity = velocity.z * transform.forward;
            rigidbody.MovePosition(rigidbody.position + (velocity * Time.fixedDeltaTime * 5));

        }
        else
        {
            rigidbody.MovePosition(rigidbody.position + (transform.forward * velocity.z * Time.fixedDeltaTime));
            rigidbody.MovePosition(rigidbody.position + (transform.right * velocity.x * Time.fixedDeltaTime));
        }
        

    }


    void OnEnabled()
    {
        Cardboard.SDK.OnTrigger += TriggerPulled;
    }

    void TriggerPulled()
    {
        GameObject cursor = GameObject.Find("Gaze Pointer Cursor");
        if( !hasTarget )
        {
            target = cursor.transform.position.normalized;
            hasTarget = true;
        }
        else
        {
            hasTarget = false;
        }
        
    }
}