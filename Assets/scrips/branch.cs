using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;
public class branch : MonoBehaviour
{
    public float moveSpeed;
    public GameObject next_wood;
    Vector3 player=player_data.Player_Position;
    public bool catched=false;
    public Transform target;
    public void move_to(Vector3 targetPosition)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    //.normalized表單位向量
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("碰到了物件：" + other.name);
        catched=true;
    }
    void Update()
    {
        if(catched)
        {
            Vector3 a =new Vector3((float)0.4,(float)0.4,(float)0.4);
            move_to(target.position+a);
        }
    }
}