using UnityEngine;
using UnityEngine.UIElements;

public class manager : MonoBehaviour
{
    public GameObject ground_with_color;
    public GameObject tree;
    public int map_size=9;
    public int tree_quantity=36;
    struct space{
        public bool tree;
        public Vector2 position;
    };
    space [,] map;
    
    void Start(){
        map=new space[map_size,map_size];
        GameObject grounds_folder =new GameObject("ground_colors");
        GameObject trees_folder=new GameObject("trees");
        for(float i = 0; i <map_size; i++){
            for(float j = 0; j <map_size; j++){
                Vector2 position=new Vector2(i-3,j-3);
                Instantiate(ground_with_color, position,Quaternion.identity,grounds_folder.transform);
                map[(int)i,(int)j].position=new Vector2(i-3,j-3);
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
                    Instantiate(tree,map[i,j].position,tree.transform.rotation,trees_folder.transform);
                    map[i,j].tree=true;
                    break;
                }
            }
        }
    }

    void Update(){
        
    }
}
