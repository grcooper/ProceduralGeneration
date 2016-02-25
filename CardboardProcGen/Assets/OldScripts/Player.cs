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
        OnEnabled();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {

        if ( hasTarget )
        {
            GameObject myHead = GameObject.Find("Head");
            Vector3 forwardMovement = new Vector3(myHead.transform.forward.x, 0, myHead.transform.forward.z);
            transform.localPosition += forwardMovement * speed * Time.deltaTime;
        }

    }


    void OnEnabled()
    {
        Cardboard.SDK.OnTrigger += TriggerPulled;
    }

    void TriggerPulled()
    {
        Debug.Log("TriggerPulled");
        GameObject cursor = GameObject.Find("Pointer");
        if( !hasTarget )
        {
            target = cursor.transform.position.normalized;
            target.y = rigidbody.position.y;
            hasTarget = true;
            Debug.Log(target);
        }
        else
        {
            hasTarget = false;
        }
        
    }
}