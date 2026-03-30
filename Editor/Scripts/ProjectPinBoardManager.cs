using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public static class ProjectPinBoardManager
    {
        #region File Path

        internal const string BaseName = "ProjectPinBoard";

        private static readonly string s_ProjectPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "../")
        );

        private static readonly string s_UserSettingsPath = Path.Combine(
            s_ProjectPath,
            "UserSettings"
        );

        internal static readonly string LocalFilePathTemplate = Path.GetFullPath(
            Path.Combine(s_UserSettingsPath, BaseName + ".{0}.json")
        );

        #endregion

        #region Window

        public static void Open(bool forceReopen = false)
        {
            if (!forceReopen && ProjectPinBoardWindow.HasOpenInstances())
            {
                ProjectPinBoardWindow window = ProjectPinBoardWindow.GetOpenedInstance();
                window.Show(true);
                window.Focus();
            }
            else
            {
                ProjectPinBoardWindow.CreateInstance();
            }
        }

        #endregion

        #region Pin & Unpin

        public static event Action<string[]> pinned;

        public static event Action<string[]> unpinned;

        public static void Pin(string[] guids)
        {
            foreach (string guid in guids)
            {
                ProjectPinBoardData.AddItem(guid);
            }
            SaveData();
            pinned?.Invoke(guids);
        }

        public static void Pin(string guid)
        {
            ProjectPinBoardData.AddItem(guid);
            SaveData();
            pinned?.Invoke(new[] { guid });
        }

        public static void Unpin(string[] guids)
        {
            foreach (string guid in guids)
            {
                ProjectPinBoardData.RemoveItem(guid);
            }
            SaveData();
            unpinned?.Invoke(guids);
        }

        public static void Unpin(string guid)
        {
            ProjectPinBoardData.RemoveItem(guid);
            SaveData();
            unpinned?.Invoke(new[] { guid });
        }

        public static bool IsPinned(string guid)
        {
            return ProjectPinBoardData.HasItem(guid);
        }

        #endregion

        #region Top

        public static void Top(string[] guids, bool top = true)
        {
            foreach (string guid in guids)
            {
                ItemInfo item = ProjectPinBoardData.GetItem(guid);
                if (item != null)
                    item.top = top;
            }
            SaveData();
        }

        public static void Top(string guid, bool top = true)
        {
            ItemInfo item = ProjectPinBoardData.GetItem(guid);
            if (item != null)
                item.top = top;
            SaveData();
        }

        #endregion

        #region DisplayName

        private static readonly char[] s_InvalidDisplayNameChars = new[]
        {
            '?',
            '<',
            '>',
            '\\',
            ':',
            '*',
            '|',
            '\"'
        };

        public static bool SetDisplayName(string guid, string value)
        {
            ItemInfo item = ProjectPinBoardData.GetItem(guid);
            if (item == null)
            {
                return false;
            }
            if (value.IndexOfAny(s_InvalidDisplayNameChars) != -1)
            {
                EditorUtility.DisplayDialog(
                    "[Project Pin Board] Invalid display name",
                    $"A display name can't contain any of the following characters: {s_InvalidDisplayNameChars.Join("")}",
                    "OK"
                );
                return false;
            }
            item.displayName = value.Trim();
            SaveData();
            return true;
        }

        public static void RemoveDisplayName(string guid)
        {
            ItemInfo item = ProjectPinBoardData.GetItem(guid);
            if (item == null)
                return;
            item.displayName = string.Empty;
            SaveData();
        }

        #endregion

        #region Tags

        private static readonly char[] s_InvalidTagNameChars = new[]
        {
            '/',
            '?',
            '<',
            '>',
            '\\',
            ':',
            '*',
            '|',
            '\"'
        };

        public static bool IsValidTag(string tag, bool showTips = true)
        {
            if (tag.IndexOfAny(s_InvalidTagNameChars) != -1)
            {
                if (showTips)
                {
                    EditorUtility.DisplayDialog(
                        "[Project Pin Board] Invalid tag name",
                        $"A tag name can't contain any of the following characters: {s_InvalidTagNameChars.Join()}",
                        "OK"
                    );
                }
                return false;
            }
            return true;
        }

        public static void SetTags(string guid, List<string> tags)
        {
            ItemInfo item = ProjectPinBoardData.GetItem(guid);
            if (item == null)
                return;
            item.tags.Clear();
            tags.ForEach(v => item.AddTag(v));
            SaveData();
        }

        public static void RemoveTags(string guid)
        {
            ItemInfo item = ProjectPinBoardData.GetItem(guid);
            if (item == null)
                return;
            item.tags.Clear();
            SaveData();
        }

        #endregion

        #region Data

        public static event Action dataUpdated;

        private static void SaveData(bool sendNotification = true)
        {
            ProjectPinBoardData.Save();
            if (sendNotification)
            {
                Notify();
            }
        }

        public static void ClearData()
        {
            ProjectPinBoardData.Reset();
            Notify();
        }

        public static void ReloadData()
        {
            ProjectPinBoardData.Reload();
            Notify();
        }

        private static void Notify()
        {
            dataUpdated?.Invoke();
        }

        #endregion

        #region Settings

        public static void ResetSettings()
        {
            ProjectPinBoardSettings.Reset();
        }

        public static void ReloadSettings()
        {
            ProjectPinBoardSettings.Reload();
        }

        #endregion
    }
}
