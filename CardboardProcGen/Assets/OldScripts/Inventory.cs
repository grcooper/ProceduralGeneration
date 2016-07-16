using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour {

	public class InventoryItem
	{
		// Constructor
		public InventoryItem() { }

		// The object "held" by the inventory
		// Dropping the item spawns it back into the world
		GameObject item;

		// Spawns the item back into the world at pos
		void DropItem(Vector3 pos)
		{

		}

		// Pick up the game object and put it into characters inventory
		void PickupItem(GameObject go)
		{

		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
