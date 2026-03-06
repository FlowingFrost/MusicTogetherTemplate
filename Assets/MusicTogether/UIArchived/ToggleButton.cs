using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButton : BassicButton
{
    public bool Selected;

    public void Awake()
    {
        button.onClick.AddListener(ToggleSelection);
    }
    public void ToggleSelection()
    {
        Selected = !Selected;
        UpdateStyle();
    }
    public void SetState(bool TargetState)
    {
        Selected = TargetState;
        UpdateStyle();
    }
    public override void UpdateStyle()
    {
        base.UpdateStyle();
        if (Selected)
        {
            if(style.ChangeHighlighedColorTint)
                button.colors = style.HighlighedButton;
            Background.color = style.ElementBgHighlightedColor;
        }
        else
        {
            if(style.ChangeNormalColorTint) 
                button.colors = style.NormalButton;
            Background.color = style.ElementBgColor;
        }
    }
}
