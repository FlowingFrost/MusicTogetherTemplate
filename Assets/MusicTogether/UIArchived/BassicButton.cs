using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BassicButton : MonoBehaviour
{
    public UIStyleData style;
    public ButtonType contentType;
    public Image Background;
    public Button button;
    [ShowIf("@contentType == ButtonType.Image")]public Image image;
    [ShowIf("@contentType == ButtonType.Image"),PreviewField] public Sprite Icon;
    [ShowIf("@contentType == ButtonType.Text")] public TextMeshProUGUI text;
    [ShowIf("@contentType == ButtonType.Text")] public string Content;

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateStyle();
    }
#endif
    // Update is called once per frame
    void Start()
    {
        UpdateStyle();
    }
    public virtual void UpdateStyle()
    {
        if (!style || !Background || !button)
            return;
        Background.color = style.ElementBgColor;
        if (style.ChangeNormalColorTint && button.transition == Selectable.Transition.ColorTint)
        {
            button.colors = style.NormalButton;
        }
        switch (contentType)
        {
            case ButtonType.Image:
                if (!image)
                    return;
                image.sprite = Icon;
                image.color = style.ElementColor;
                break;
            case ButtonType.Text:
                if (!text)
                    return;
                text.text = Content;
                text.fontSize = style.elementFontSize;
                text.font = style.TMP_font;
                text.color = style.ElementColor;
                break;
        }
    }
}
