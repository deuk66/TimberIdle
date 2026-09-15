using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;
public class branch : MonoBehaviour
{
    public float moveSpeed;
    public void move_to(Vector3 targetPosition)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    void Start()
    {
        
    }
}