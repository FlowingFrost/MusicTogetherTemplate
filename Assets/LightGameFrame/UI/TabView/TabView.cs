using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightGameFrame.UI
{
    public class TabView : VisualElement
    {
        public const string USSClassName = "lgf-tabview";
        public const string HeaderClassName = "lgf-tabview__header";
        public const string TabClassName = "lgf-tabview__tab";
        public const string TabSelectedClassName = "lgf-tabview__tab--selected";
        public const string TabLabelClassName = "lgf-tabview__label";
        public const string TabCloseClassName = "lgf-tabview__close";
        public const string ContentClassName = "lgf-tabview__content";
        public const string PanelClassName = "lgf-tabview__panel";
        public const string PanelContentClassName = "lgf-tabview__panel-content";
        public const string PanelSelectedClassName = "lgf-tabview__panel--selected";

    private readonly List<TabData> _tabs = new();
    private VisualElement _header;
    private VisualElement _content;
    private int _selectedIndex;
    private bool _allowNoneSelected;
    private int _tabCount;
    private List<string> _titles = new();
    private bool _closableForAll;
    private HashSet<int> _closableIndices = new();
    private int _requestedSelectedIndex;
    private bool _isRebuilding;
    private bool _syncScheduled;

        public int SelectedIndex => _selectedIndex;
        public int TabCount => _tabs.Count;

        public new class UxmlFactory : UxmlFactory<TabView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlIntAttributeDescription _tabCount = new() { name = "tab-count", defaultValue = 3 };
            private readonly UxmlStringAttributeDescription _tabTitles = new() { name = "tab-titles", defaultValue = "" };
            private readonly UxmlBoolAttributeDescription _closable = new() { name = "closable", defaultValue = false };
            private readonly UxmlStringAttributeDescription _closableTabs = new() { name = "closable-tabs", defaultValue = "" };
            private readonly UxmlIntAttributeDescription _selectedIndex = new() { name = "selected-index", defaultValue = 0 };
            private readonly UxmlBoolAttributeDescription _allowNoneSelected = new() { name = "allow-none-selected", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var tabView = (TabView)ve;
                var tabCount = Mathf.Max(0, _tabCount.GetValueFromBag(bag, cc));
                var tabTitles = _tabTitles.GetValueFromBag(bag, cc);
                var closable = _closable.GetValueFromBag(bag, cc);
                var closableTabs = _closableTabs.GetValueFromBag(bag, cc);
                var selectedIndex = _selectedIndex.GetValueFromBag(bag, cc);
                var allowNoneSelected = _allowNoneSelected.GetValueFromBag(bag, cc);

                tabView.BuildTabs(
                    tabCount,
                    ParseTitles(tabTitles),
                    closable,
                    ParseIndices(closableTabs),
                    selectedIndex,
                    allowNoneSelected);
            }
        }

    public override VisualElement contentContainer => this;

        public TabView()
        {
            AddToClassList(USSClassName);
            RegisterCallback<AttachToPanelEvent>(_ => ScheduleSyncFromChildren());
        }

        public void BuildTabs(
            int tabCount,
            List<string> titles,
            bool closableForAll,
            HashSet<int> closableIndices,
            int selectedIndex,
            bool allowNoneSelected)
        {
            _tabCount = tabCount;
            _titles = titles ?? new List<string>();
            _closableForAll = closableForAll;
            _closableIndices = closableIndices ?? new HashSet<int>();
            _requestedSelectedIndex = selectedIndex;
            _tabs.Clear();
            var pages = CollectTabPages();
            if (pages.Count > 0)
            {
                BuildTabsFromPages(pages, allowNoneSelected);
                return;
            }

            var contentRoots = CollectContentRoots();
            if (contentRoots.Count > 0)
            {
                BuildTabsFromContentRoots(
                    contentRoots,
                    titles,
                    closableForAll,
                    closableIndices,
                    selectedIndex,
                    allowNoneSelected);
                return;
            }

            for (int i = 0; i < tabCount; i++)
            {
                var title = i < titles.Count ? titles[i] : $"Tab {i + 1}";
                var closable = closableForAll || closableIndices.Contains(i + 1);
                _tabs.Add(new TabData(title, closable));
            }

            _allowNoneSelected = allowNoneSelected;
            _selectedIndex = NormalizeSelectedIndex(selectedIndex, allowNoneSelected, _tabs.Count);

            RebuildVisualTree(null);
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                return;
            }

            _selectedIndex = index;
            RefreshSelection();
        }

        public VisualElement GetContentPanel(int index)
        {
            if (_content == null || index < 0 || index >= _content.childCount)
            {
                return null;
            }

            var panel = _content[index];
            return panel.Q<VisualElement>($"tab-panel-{index + 1}-content");
        }

        private void CloseTab(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                return;
            }

            _tabs.RemoveAt(index);
            _selectedIndex = NormalizeSelectedIndex(_selectedIndex, _allowNoneSelected, _tabs.Count);
            var contentRoots = CollectCurrentPanelContents();
            if (index >= 0 && index < contentRoots.Count)
            {
                contentRoots.RemoveAt(index);
            }
            RebuildVisualTree(contentRoots);
        }

        private void RebuildVisualTree(List<VisualElement> contentRoots)
        {
            _isRebuilding = true;
            hierarchy.Clear();
            AddToClassList(USSClassName);

            _header = new VisualElement
            {
                name = "tab-header"
            };
            _header.AddToClassList(HeaderClassName);

            _content = new VisualElement
            {
                name = "tab-content"
            };
            _content.AddToClassList(ContentClassName);

            hierarchy.Add(_header);
            hierarchy.Add(_content);

            for (int i = 0; i < _tabs.Count; i++)
            {
                CreateTab(i, _tabs[i]);
                var existingContent = contentRoots != null && i < contentRoots.Count ? contentRoots[i] : null;
                CreatePanel(i, existingContent);
            }

            RefreshSelection();
            _isRebuilding = false;
        }

        private void CreateTab(int index, TabData data)
        {
            var tab = new VisualElement
            {
                name = $"tab-{index + 1}"
            };
            tab.AddToClassList(TabClassName);

            var label = new Label(data.Title)
            {
                name = $"tab-{index + 1}-label"
            };
            label.AddToClassList(TabLabelClassName);
            tab.Add(label);

            if (data.Closable)
            {
                var closeButton = new Button
                {
                    name = $"tab-{index + 1}-close",
                    text = "×"
                };
                closeButton.AddToClassList(TabCloseClassName);
                closeButton.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    CloseTab(index);
                });
                tab.Add(closeButton);
            }

            tab.RegisterCallback<ClickEvent>(_ => SelectTab(index));
            _header.Add(tab);
        }

        private void CreatePanel(int index, VisualElement contentRoot)
        {
            var panel = new VisualElement
            {
                name = $"tab-panel-{index + 1}"
            };
            panel.AddToClassList(PanelClassName);

            var panelContent = contentRoot ?? new VisualElement();
            panelContent.name = $"tab-panel-{index + 1}-content";
            if (!panelContent.ClassListContains(PanelContentClassName))
            {
                panelContent.AddToClassList(PanelContentClassName);
            }
            if (panelContent.parent != null)
            {
                panelContent.RemoveFromHierarchy();
            }
            panel.Add(panelContent);
            _content.Add(panel);
        }

        private void RefreshSelection()
        {
            if (_header == null || _content == null)
            {
                return;
            }

            for (int i = 0; i < _header.childCount; i++)
            {
                var tab = _header[i];
                if (i == _selectedIndex)
                {
                    tab.AddToClassList(TabSelectedClassName);
                }
                else
                {
                    tab.RemoveFromClassList(TabSelectedClassName);
                }
            }

            for (int i = 0; i < _content.childCount; i++)
            {
                var panel = _content[i];
                if (i == _selectedIndex)
                {
                    panel.AddToClassList(PanelSelectedClassName);
                }
                else
                {
                    panel.RemoveFromClassList(PanelSelectedClassName);
                }
            }
        }

        private static int NormalizeSelectedIndex(int selectedIndex, bool allowNoneSelected, int tabCount)
        {
            if (tabCount <= 0)
            {
                return -1;
            }

            if (allowNoneSelected && selectedIndex < 0)
            {
                return -1;
            }

            return Mathf.Clamp(selectedIndex, 0, tabCount - 1);
        }

        private static List<string> ParseTitles(string csv)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return result;
            }

            var parts = csv.Split(',');
            foreach (var part in parts)
            {
                var title = part.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    result.Add(title);
                }
            }

            return result;
        }

        private static HashSet<int> ParseIndices(string csv)
        {
            var result = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return result;
            }

            var parts = csv.Split(',');
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out var index) && index > 0)
                {
                    result.Add(index);
                }
            }

            return result;
        }

        private class TabData
        {
            public string Title { get; }
            public bool Closable { get; }

            public TabData(string title, bool closable)
            {
                Title = title;
                Closable = closable;
            }
        }

        private List<TabPage> CollectTabPages()
        {
            var pages = new List<TabPage>();
            foreach (var child in EnumerateUserChildren())
            {
                if (child is TabPage page)
                {
                    pages.Add(page);
                }
            }

            return pages;
        }

        private List<VisualElement> CollectContentRoots()
        {
            var roots = new List<VisualElement>();
            foreach (var child in EnumerateUserChildren())
            {
                if (child is TabPage)
                {
                    continue;
                }

                roots.Add(child);
            }

            return roots;
        }

        private void BuildTabsFromPages(List<TabPage> pages, bool allowNoneSelected)
        {
            var selectedIndex = -1;
            var contentRoots = new List<VisualElement>();

            for (int i = 0; i < pages.Count; i++)
            {
                var page = pages[i];
                var title = string.IsNullOrWhiteSpace(page.Title) ? $"Tab {i + 1}" : page.Title;
                _tabs.Add(new TabData(title, page.Closable));

                if (selectedIndex < 0 && page.Selected)
                {
                    selectedIndex = i;
                }

                var panelContent = new VisualElement();
                panelContent.AddToClassList(PanelContentClassName);
                var children = page.Children().ToList();
                foreach (var child in children)
                {
                    panelContent.Add(child);
                }
                page.Clear();
                contentRoots.Add(panelContent);
            }

            _allowNoneSelected = allowNoneSelected;
            if (selectedIndex < 0)
            {
                selectedIndex = allowNoneSelected ? -1 : 0;
            }
            _selectedIndex = NormalizeSelectedIndex(selectedIndex, allowNoneSelected, _tabs.Count);
            RebuildVisualTree(contentRoots);
        }

        private void BuildTabsFromContentRoots(
            List<VisualElement> contentRoots,
            List<string> titles,
            bool closableForAll,
            HashSet<int> closableIndices,
            int selectedIndex,
            bool allowNoneSelected)
        {
            var panelRoots = new List<VisualElement>();

            for (int i = 0; i < contentRoots.Count; i++)
            {
                var root = contentRoots[i];
                var title = i < titles.Count
                    ? titles[i]
                    : (!string.IsNullOrWhiteSpace(root.name) ? root.name : $"Tab {i + 1}");
                var closable = closableForAll || closableIndices.Contains(i + 1);
                _tabs.Add(new TabData(title, closable));

                root.name = $"tab-panel-{i + 1}-content";
                if (!root.ClassListContains(PanelContentClassName))
                {
                    root.AddToClassList(PanelContentClassName);
                }
                if (root.parent != null)
                {
                    root.RemoveFromHierarchy();
                }
                panelRoots.Add(root);
            }

            _allowNoneSelected = allowNoneSelected;
            _selectedIndex = NormalizeSelectedIndex(selectedIndex, allowNoneSelected, _tabs.Count);
            RebuildVisualTree(panelRoots);
        }

        private IEnumerable<VisualElement> EnumerateUserChildren()
        {
            foreach (var child in Children())
            {
                if (child == _header || child == _content)
                {
                    continue;
                }

                yield return child;
            }
        }

        private void ScheduleSyncFromChildren()
        {
            if (_isRebuilding || _syncScheduled)
            {
                return;
            }

            _syncScheduled = true;
            schedule.Execute(() =>
            {
                _syncScheduled = false;
                SyncFromChildrenIfNeeded();
            });
        }

        private void SyncFromChildrenIfNeeded()
        {
            if (_isRebuilding)
            {
                return;
            }

            var pages = CollectTabPages();
            if (pages.Count > 0)
            {
                _tabs.Clear();
                BuildTabsFromPages(pages, _allowNoneSelected);
                return;
            }

            var contentRoots = CollectContentRoots();
            if (contentRoots.Count == 0)
            {
                return;
            }

            _tabs.Clear();
            BuildTabsFromContentRoots(
                contentRoots,
                _titles,
                _closableForAll,
                _closableIndices,
                _requestedSelectedIndex,
                _allowNoneSelected);
        }

        private List<VisualElement> CollectCurrentPanelContents()
        {
            var result = new List<VisualElement>();
            if (_content == null)
            {
                return result;
            }

            for (int i = 0; i < _content.childCount; i++)
            {
                var panel = _content[i];
                var panelContent = panel.Q<VisualElement>($"tab-panel-{i + 1}-content");
                if (panelContent != null)
                {
                    result.Add(panelContent);
                }
            }

            return result;
        }
    }
}
