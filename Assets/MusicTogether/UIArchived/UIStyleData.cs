using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public enum ButtonType { Text, Image }
[CreateAssetMenu(menuName = "Dancing Line Fanmade/UI Style", fileName = "NewStyle")]
public class UIStyleData : ScriptableObject
{
    public string StyleName;
    public Color ElementBgColor, ElementBgHighlightedColor, ElementColor,ElementHighlightedColor;
    [Title("NormalButton")]
    public bool ChangeNormalColorTint;
    public ColorBlock NormalButton;
    [Title("HighlighedButton")]
    public bool ChangeHighlighedColorTint;
    public ColorBlock HighlighedButton;
    [Title("Font")]
    public int titleFontSize;
    public int subTitleFontSize, labelFontSize, elementFontSize;
    public Font font;
    public TMP_FontAsset TMP_font;
}
