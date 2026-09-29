using UnityEngine;

public class TreeSkillTree : MonoBehaviour
{
    [SerializeField] private Color backgroundColor = new Color(0.76f, 0.88f, 0.7f);
    [SerializeField] private UIElementSettings returnButtonLayout = new UIElementSettings { anchor = Vector2.zero, position = new Vector2(28f, 28f), size = new Vector2(280f, 72f), content = "返回遊戲", fontSize = 24 };

    private void Awake()
    {
        Camera skillTreeCamera = GetComponent<Camera>();
        if (skillTreeCamera != null)
        {
            skillTreeCamera.clearFlags = CameraClearFlags.SolidColor;
            skillTreeCamera.backgroundColor = backgroundColor;
            skillTreeCamera.cullingMask = 0;
        }

        SkillTreeReturnButton.Create(this, "TreeSkillTree", returnButtonLayout, new Color(0.12f, 0.2f, 0.14f, 0.94f));
    }
}
