using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public static class ProjectPinBoardData
    {
        [Serializable]
        private class UserData
        {
            public int version = 0;
            public List<ItemInfo> items = new List<ItemInfo>();
        }

        private static UserData s_UserData;

        private static UserData userData
        {
            get
            {
                if (s_UserData == null)
                {
                    s_UserData = GetLocal();
                    GenerateMapping();
                }
                return s_UserData;
            }
        }

        #region Items

        public static List<ItemInfo> items => userData.items;

        private static readonly Dictionary<string, ItemInfo> s_Guid2Item =
            new Dictionary<string, ItemInfo>();

        private static void GenerateMapping()
        {
            s_Guid2Item.Clear();
            foreach (ItemInfo item in items)
            {
                s_Guid2Item.Add(item.guid, item);
            }
        }

        public static ItemInfo GetItem(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }
            if (s_Guid2Item.TryGetValue(guid, out ItemInfo item))
            {
                return item;
            }
            return null;
        }

        public static void AddItem(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }
            if (s_Guid2Item.TryGetValue(guid, out ItemInfo item))
            {
                item.time = PipiUtility.GetTimestamp();
                return;
            }

            item = new ItemInfo() { guid = guid, time = PipiUtility.GetTimestamp(), };
            items.Add(item);
            s_Guid2Item.Add(guid, item);
        }

        public static void RemoveItem(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }
            if (s_Guid2Item.TryGetValue(guid, out ItemInfo item))
            {
                s_Guid2Item.Remove(guid);
                items.Remove(item);
            }
        }

        public static bool HasItem(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }
            return s_Guid2Item.TryGetValue(guid, out ItemInfo _);
        }

        #endregion

        #region Basic Interface

        public static void Save()
        {
            SetLocal(userData);
        }

        public static void Reload()
        {
            s_UserData = GetLocal();
            GenerateMapping();
        }

        public static void Reset()
        {
            s_Guid2Item.Clear();
            SetLocal(s_UserData = new UserData());
        }

        #endregion

        #region Serialization & Deserialization

        internal static readonly string SerializedFilePath = string.Format(
            ProjectPinBoardManager.LocalFilePathTemplate,
            "data"
        );

        private static UserData GetLocal()
        {
            return PipiUtility.GetLocal<UserData>(SerializedFilePath);
        }

        private static void SetLocal(UserData value)
        {
            PipiUtility.SetLocal(SerializedFilePath, value);
        }

        #endregion
    }

    #region ItemInfo

    [Serializable]
    public class ItemInfo
    {
        public string guid = string.Empty;

        public List<string> tags = new List<string>();

        public long time = 0;

        public string displayName = string.Empty;

        public bool top = false;

        public Object Asset => AssetDatabase.LoadAssetAtPath<Object>(Path);

        public string AssetName => PipiUtility.GetAssetName(guid);

        public string Name => (string.IsNullOrWhiteSpace(displayName) ? AssetName : displayName);

        public string Type => (AssetDatabase.GetMainAssetTypeAtPath(Path)?.Name ?? string.Empty);

        public string AssetBundle =>
            (IsValid() ? AssetDatabase.GetImplicitAssetBundleName(Path) : string.Empty);

        public string Path => AssetDatabase.GUIDToAssetPath(guid);

        public bool IsValid()
        {
            return (this.Asset != null);
        }

        public void AddTag(string tag)
        {
            this.tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return this.tags.Remove(tag);
        }

        public void RemoveAllTags()
        {
            this.tags.Clear();
        }

        public bool MatchGuid(string guid)
        {
            return this.guid.Equals(guid, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchTags(string[] tags)
        {
            foreach (string tag in tags)
            {
                if (!this.MatchTag(tag))
                    return false;
            }
            return true;
        }

        public bool MatchTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return true;
            return this.tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
        }

        public bool MatchType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return true;
            return this.Type.Equals(type, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDirectory()
        {
            return AssetDatabase.IsValidFolder(this.Path);
        }
    }

    #endregion
}
