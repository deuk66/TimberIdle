using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Application;
using static UnityEngine.JsonUtility;
public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position=new Vector2(0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 target_position = Input.mousePosition;
            target_position.z=-10f;
            target_position=Camera.main.ScreenToWorldPoint(target_position);
            Vector2 tp=target_position;
            move_to(tp);
        }
    }
    void move_to(Vector2 t_p)
    {
        
    }
    void step(float rolation)
    {
        Vector2 tp=transform.position;
        Vector2 angle=new Vector2();
        switch (rolation)
        {
            case 0f:
            {
                tp=tp + Vector2.up;
                angle=Vector2.up;
                break;
            }
            case 90f:
            {
                tp=tp + Vector2.left;
                angle=Vector2.left;
                break;    
            }
            case 180f:
            {   
                tp=tp +Vector2.down;
                angle=Vector2.down;
                break;    
            }
            case 270f:
            {
                tp=tp + Vector2.right;
                angle=Vector2.right;
                break;    
            }
        }
        
        if (!Physics2D.OverlapPoint(tp))
        {
            Vector2 np= transform.position;
            while (np != tp)
            {
                transform.position=transform.position+angle*0.1f;
            }
        }
    }
}
public  static class player_data
{
    
}