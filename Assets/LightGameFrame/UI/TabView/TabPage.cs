using UnityEngine.UIElements;

namespace LightGameFrame.UI
{
    public class TabPage : VisualElement
    {
        public const string USSClassName = "lgf-tabview__page";

        public string Title { get; private set; }
        public bool Closable { get; private set; }
        public bool Selected { get; private set; }

        public new class UxmlFactory : UxmlFactory<TabPage, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _title = new() { name = "title", defaultValue = "" };
            private readonly UxmlBoolAttributeDescription _closable = new() { name = "closable", defaultValue = false };
            private readonly UxmlBoolAttributeDescription _selected = new() { name = "selected", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var page = (TabPage)ve;
                page.Title = _title.GetValueFromBag(bag, cc);
                page.Closable = _closable.GetValueFromBag(bag, cc);
                page.Selected = _selected.GetValueFromBag(bag, cc);
                page.AddToClassList(USSClassName);
            }
        }

        public TabPage()
        {
            AddToClassList(USSClassName);
        }
    }
}
