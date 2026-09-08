using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct space{
    public GameObject tree;
    public Vector3 position;
};
public class manager : MonoBehaviour
{
    public GameObject tree;
    public int map_size=9;
    public int tree_quantity=36;
    
    public GameObject tree_top;
    void Start(){
        manager_data.map=new space[map_size,map_size];

        for(int i = 0; i < map_size; i++)
        {
            for(int j = 0; j < map_size; j++)
            {
                manager_data.map[i,j].position=new Vector3(i-4,j-4,-0.01f);
            }
        }

        for(int _ = 0; _ < tree_quantity; _++)
        {
            while (true)
            {
                int i=Random.Range(0,map_size);
                int j=Random.Range(0,map_size);
                if (!manager_data.map[i, j].tree)
                {
                    manager_data.trees.Add(Instantiate(tree,manager_data.map[i,j].position,tree.transform.rotation,tree_top.transform));
                    manager_data.map[i,j].tree=manager_data.trees[_];
                    break;
                }
            }
        }
    }
    void Update(){
        
    }

}

public static class manager_data
{
    public static space [,] map;
    public static List<GameObject> trees=new List<GameObject>{};
}