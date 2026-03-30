using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private void RegisterHotkeys()
        {
            rootVisualElement.RegisterCallback<KeyDownEvent>(
                (evt) =>
                {
                    bool stopEvent = true;

                    if (evt.altKey && evt.shiftKey && evt.keyCode == KeyCode.R)
                    {
                        if (!string.IsNullOrWhiteSpace(m_FirstSelectedItemGuid))
                        {
                            PipiUtility.ShowInExplorer(m_FirstSelectedItemGuid);
                        }
                    }
                    else if (evt.ctrlKey && evt.keyCode == KeyCode.F)
                    {
                        FocusToSearchField();
                    }
                    else if (evt.keyCode == KeyCode.F2)
                    {
                        ListItem item = GetSelectedAssetListItem();
                        item?.ShowNameTextField();
                    }
                    else if (evt.keyCode == KeyCode.F5)
                    {
                        Menu_Reload();
                    }
                    else if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
                    {
                        string[] names = GetSelectedItemInfos()
                            .Select(v => $"- {v.Name}")
                            .ToArray();
                        bool isOk = EditorUtility.DisplayDialog(
                            "[Project Pin Board] Unpin assets",
                            $"Are you sure to unpin the following assets?\n{string.Join("\n", names)}",
                            "Confirm!",
                            "Cancel"
                        );
                        if (isOk)
                        {
                            ProjectPinBoardManager.Unpin(GetSelectedItemGuids());
                        }
                    }
                    else
                    {
                        stopEvent = false;
                    }

                    if (stopEvent)
                    {
                        evt.PreventDefault();
                        evt.StopImmediatePropagation();
                    }
                }
            );
        }
    }
}
