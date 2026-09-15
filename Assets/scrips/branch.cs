using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;
public class branch : MonoBehaviour
{
    public float moveSpeed;
    Vector3 player=player_data.Player_Position;
    public bool catched;
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
    }
    void Update()
    {
        if (!catched&&(transform.position - player).sqrMagnitude <= 1.0)
        {
            Debug.Log("catched");
            catched=true;
        }
        if(catched&&(transform.position - player).sqrMagnitude > 1.0)
        {
            move_to(player_data.newposition);
        }
    }
}