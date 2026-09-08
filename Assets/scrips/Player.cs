using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Application;
using static UnityEngine.JsonUtility;
public class Player : MonoBehaviour
{
    Vector3 tp=new Vector3();
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
            tp.z=-1f;
        }
        move_to(tp);
        player_data.Player_Position=transform.position;
    }

    [Header("移動設定")]
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
}