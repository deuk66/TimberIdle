using UnityEngine;

public class PlayerSkillTree : MonoBehaviour
{
    [SerializeField] private Color backgroundColor = new Color(0.82f, 0.84f, 0.85f);
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

        SkillTreeReturnButton.Create(this, "PlayerSkillTree", returnButtonLayout, new Color(0.2f, 0.22f, 0.25f, 0.94f));
    }
}
