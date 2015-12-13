using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.Find("Player");
        transform.position = player.transform.position;
        Debug.Log(transform.position);
    }

    void Update()
    {
        GameObject player = GameObject.Find("Player");
        transform.position = player.transform.position;
        Debug.Log(transform.position);
    }
}