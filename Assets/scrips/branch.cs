using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;
public class branch : MonoBehaviour
{
    public float moveSpeed;
    public branch_string bs;
    public GameObject next_wood;
    Vector3 player=player_data.Player_Position;
    public bool catched=false;
    public Vector3 target=new Vector3(0f,0f,0f);
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
        catched=false;
        target=transform.position;
    }
    
    void Update()
    {
        if (catched)
        {
            target=player_data.Player_Position+new Vector3((float)0.4,(float)0.4,(float)0.4);
        }
        move_to(target);
    }
}
public class branch_string
{
    branch_string next;
    branch_string last;
    branch main;

    public void collect()
    {
        main.catched=false;
        if (next!=null)
        {
            next.collect();
        }
        main.target=manager_data.station.transform.position;
    }
    public void add_next(branch t)
    {
        if (!main)
        {
            main =t;
            return;
        }else if (main&&next==null)
        {
            next=new branch_string{};
        }
        if(next != null)
        {
            next.add_next(t);
        }
    }
}