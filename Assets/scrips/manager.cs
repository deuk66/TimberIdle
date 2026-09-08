using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class manager : MonoBehaviour
{
    public GameObject tree;
    public int map_size=9;
    public int tree_quantity=36;
    struct space{
        public GameObject tree;
        public Vector3 position;
    };
    space [,] map;
    
    void Start(){
        map=new space[map_size,map_size];
        GameObject tree_top=new GameObject{};

        for(int i = 0; i < map_size; i++)
        {
            for(int j = 0; j < map_size; j++)
            {
                map[i,j].position=new Vector3(i-4,j-4,-0.01f);
            }
        }

        for(int _ = 0; _ < tree_quantity; _++)
        {
            while (true)
            {
                int i=Random.Range(0,map_size);
                int j=Random.Range(0,map_size);
                if (!map[i, j].tree)
                {
                    manager_data.trees.Add(Instantiate(tree,map[i,j].tree.transform.position,tree.transform.rotation,tree_top.transform));
                    map[i,j].tree=manager_data.trees[_];
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
    public static List<GameObject> trees=new List<GameObject>{};
}
