using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Application;
using static UnityEngine.JsonUtility;
public class Player : MonoBehaviour
{
    Vector3 tp=new Vector3();
    public Vector3 newposition=new Vector3();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tp = Input.mousePosition;
            tp=Camera.main.ScreenToWorldPoint(tp);
            tp.z=0f;
        }
        move_to(tp);
        player_data.Player_Position=transform.position;
    }

    public float moveSpeed = 5f; // 移動速度（單位：單位/秒）

    /// <summary>
    /// 讓角色均速平移至指定的目標座標
    /// </summary>
    /// <param name="targetPosition">目標世界座標</param>
    public void move_to(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        player_data.newposition=targetPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("touch "+other.name);
        if (!(player_data.first_branch)&&other.name == "branch")
        {
            player_data.first_branch=other.gameObject.GetComponent<branch>();
            player_data.first_branch.catched=true;
        }else if(player_data.first_branch&&other.name=="branch")
        {
            // if (!(other.gameObject.GetComponent<branch>().catched))
            // {
            //     other.gameObject.GetComponent<branch>().catched=true;
            //     player_data.branch_count++;
            // }
            player_data.first_branch.bs.end=player_data.first_branch.bs.add_next(other.GetComponent<branch>());
        }
        else if (other.name == "station")
        {
            player_data.first_branch.bs.collect();
        }
    }
    //向量版本
    /*
    /// <summary>
    /// 讓角色朝指定方向向量均速平移指定距離
    /// </summary>
    /// <param name="direction">移動方向（建議傳入單位向量）</param>
    /// <param name="distance">移動總距離</param>
    public void MoveInDirection(Vector3 direction, float distance)
    {
        // 算出目標位置後呼叫原有的移動函數
        Vector3 targetPosition = transform.position + direction.normalized * distance;
        MoveToTarget(targetPosition);
    }
    */
}
public static class player_data
{
    public static Vector3 Player_Position;
    public static Vector3 newposition;
    public static int branch_count;
    public static branch first_branch=null;
}
