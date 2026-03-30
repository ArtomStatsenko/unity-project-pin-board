using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        #region Asset

        [SerializeField]
        private VisualTreeAsset visualTree = null;

        #endregion

        #region Instance

        private const string k_Title = "Project Pin Board";

        private void ApplyTitleContent()
        {
            titleContent = new GUIContent()
            {
                text = k_Title,
                image = PipiUtility.GetIcon("Favorite"),
            };
        }

        public static bool HasOpenInstances()
        {
            return HasOpenInstances<ProjectPinBoardWindow>();
        }

        public static ProjectPinBoardWindow GetOpenedInstance()
        {
            return HasOpenInstances() ? GetWindow<ProjectPinBoardWindow>() : null;
        }

        public static ProjectPinBoardWindow CreateInstance()
        {
            ProjectPinBoardWindow window = GetOpenedInstance();
            if (window != null)
            {
                window.Close();
            }
            window = CreateWindow<ProjectPinBoardWindow>();
            window.ApplyTitleContent();
            window.minSize = new Vector2(100, 100);
            window.SetSize(600, 500);
            window.SetCenter();
            return window;
        }

        public void SetSize(int width, int height)
        {
            Rect pos = position;
            pos.width = width;
            pos.height = height;
            position = pos;
        }

        public void SetCenter(int offsetX = 0, int offsetY = 0)
        {
            Rect mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = position;
            float centerOffsetX = (mainWindowPos.width - pos.width) * 0.5f;
            float centerOffsetY = (mainWindowPos.height - pos.height) * 0.5f;
            pos.x = mainWindowPos.x + centerOffsetX + offsetX;
            pos.y = mainWindowPos.y + centerOffsetY + offsetY;
            position = pos;
        }

        private void ShowNotification(string content, double fadeoutWait = 1f)
        {
            ShowNotification(new GUIContent(content), fadeoutWait);
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            ProjectPinBoardData.Reload();
            ProjectPinBoardSettings.Reload();
        }

        private void OnEnable()
        {
            ApplyTitleContent();

            ProjectPinBoardManager.dataUpdated += RefreshData;
            ProjectPinBoardManager.pinned += OnPinned;

            EditorApplication.projectChanged += RefreshData;
        }

        private void OnDisable()
        {
            ProjectPinBoardManager.dataUpdated -= RefreshData;
            ProjectPinBoardManager.pinned -= OnPinned;

            EditorApplication.projectChanged -= RefreshData;
        }

        private void CreateGUI()
        {
            visualTree.CloneTree(rootVisualElement);
            Init();
        }

        #endregion

        private void Init()
        {
            InitToolbar();
            InitContent();

            InitDropArea();

            RegisterHotkeys();

            RefreshData();

            ApplySettings();
        }

        #region Data

        private readonly List<string> m_ItemTypeList = new List<string>();

        private readonly List<string> m_ItemTagList = new List<string>();

        private void CollectInfo()
        {
            m_ItemTypeList.Clear();
            m_ItemTagList.Clear();
            foreach (ItemInfo itemInfo in ProjectPinBoardData.items)
            {
                string type = itemInfo.Type;
                if (!string.IsNullOrEmpty(type) && !m_ItemTypeList.Contains(type))
                {
                    m_ItemTypeList.Add(type);
                }
                foreach (string tag in itemInfo.tags)
                {
                    if (!m_ItemTagList.Contains(tag))
                        m_ItemTagList.Add(tag);
                }
            }
            m_ItemTypeList.Sort((Comparison<string>)Comparison);
            m_ItemTagList.Sort((Comparison<string>)Comparison);

            int Comparison(string a, string b)
            {
                return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private void RefreshData()
        {
            if (!IsContentInited())
                return;

            CollectInfo();

            UpdateContent();
        }

        private void OnPinned(string[] guids)
        {
            if (!IsContentInited())
                return;

            if (guids.Length > 0)
            {
                SetAssetListSelection(guids[0]);
            }
        }

        #endregion

        #region Settings

        private void ApplySettings()
        {
            if (!IsContentInited())
                return;

            m_ToolbarTopFolderToggle.SetValueWithoutNotify(ProjectPinBoardSettings.topFolder);
            m_ToolbarPreviewToggle.SetValueWithoutNotify(ProjectPinBoardSettings.enablePreview);
            m_ToolbarSyncSelectionToggle.SetValueWithoutNotify(
                ProjectPinBoardSettings.syncSelection
            );

            ApplySettings_DragLine();
        }

        private void ApplySettings_DragLine()
        {
            if (!IsContentInited())
                return;

            float rootWidth = rootVisualElement.worldBound.width;
            float leftPaneMinWidth = m_AssetList.style.minWidth.value.value;
            float rightPaneMinWidth = m_PreviewPane.style.minWidth.value.value;
            float dragLinePos = ProjectPinBoardSettings.dragLinePos;
            if (dragLinePos < leftPaneMinWidth || dragLinePos > rootWidth - rightPaneMinWidth)
            {
                dragLinePos = leftPaneMinWidth;
            }
            else
            {
                if (m_ContentSplitView.fixedPaneIndex == 1)
                {
                    dragLinePos = rootWidth - dragLinePos;
                }
            }
            m_ContentSplitView.fixedPaneInitialDimension = dragLinePos;
        }

        #endregion
    }
}
