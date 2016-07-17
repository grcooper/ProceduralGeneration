using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    Rigidbody myRigidbody;
    Vector3 velocity;
    Quaternion rotation;
    Vector3 target;
    public int speed = 10000;
    bool hasTarget = false;

	public float startingHealth = 100;
	public float healthDecreasePerSecond = 1;

	float currentHealth;

	public IslandGenerator generator;


    Vector3 zero = new Vector3(0, 0, 0);

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        rotation = myRigidbody.rotation;
        OnEnabled();
		currentHealth = startingHealth;
		GameObject gen = GameObject.Find("IslandGenerator");
		if (gen)
		{
			generator = gen.GetComponent<IslandGenerator>();
		}
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
            GameObject land = GameObject.Find("LandMesh");
            /*Debug.Log(land.transform.position);
            Debug.Log(land.GetComponent<MeshRenderer>());
            Debug.Log(land.GetComponent<MeshFilter>().transform);*/
        }
		UpdateSurvivalTraits (Time.deltaTime);


    }

	// Update the traits of our player 
	void UpdateSurvivalTraits(float seconds)
	{
		currentHealth -= seconds * healthDecreasePerSecond;
		if (currentHealth <= 0)
		{
			PlayerDied ();
		}
	}


	void PlayerDied()
	{
		// Just restart them for now
		generator.GenerateIsland();
		currentHealth = startingHealth;
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
            target.y = myRigidbody.position.y;
            hasTarget = true;
            Debug.Log(target);
        }
        else
        {
            hasTarget = false;
        }
        
    }
}