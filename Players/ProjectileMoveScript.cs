using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour {

    public int mirror = -1;
    public bool destroyMe;
    public float speed;
    
    private Vector3 directionVector = new Vector3(1, 0, 0);
    private Vector3 movement;

    public void OnStart(float duration)
    {
        if (mirror == 1) directionVector = new Vector3(-1, 0, 0);
        movement = directionVector * speed;
        Invoke("DestroyMe", duration);
    }

    public void FixedUpdateGOF()
    {
        if (CSceneManager.freezePhysics) return;
        transform.position += (movement * Time.fixedDeltaTime);
    }

    public bool IsDestroyed()
    {
        if (this == null) return true;
        if (destroyMe) DestroyMe();
        return destroyMe;
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}