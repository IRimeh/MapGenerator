using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 10.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_PlayerPosition", transform.position);

        float maxDist = Mathf.Floor(Settings.ViewDistance * 0.5f) * Settings.ChunkSize * Settings.TileWidth;
        Shader.SetGlobalFloat("_FadeEndDistance", maxDist);
        Shader.SetGlobalFloat("_FadeStartDistance", maxDist - 50.0f);


        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward * (Time.deltaTime * _movementSpeed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -Vector3.forward * (Time.deltaTime * _movementSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -Vector3.right * (Time.deltaTime * _movementSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * (Time.deltaTime * _movementSpeed);
        }
    }
}
