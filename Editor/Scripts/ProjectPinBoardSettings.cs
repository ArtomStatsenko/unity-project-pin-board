using System;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public static class ProjectPinBoardSettings
    {
        [Serializable]
        private class Settings
        {
            public int version = 0;
            public bool topFolder = true;
            public bool enablePreview = true;
            public float dragLinePos = 250f;
            public bool syncSelection = false;
        }

        private static Settings s_Settings;

        private static Settings settings => (s_Settings ??= GetLocal());

        public static bool topFolder
        {
            get => settings.topFolder;
            set
            {
                settings.topFolder = value;
                Save();
            }
        }

        public static bool enablePreview
        {
            get => settings.enablePreview;
            set
            {
                settings.enablePreview = value;
                Save();
            }
        }

        public static float dragLinePos
        {
            get => settings.dragLinePos;
            set
            {
                settings.dragLinePos = value;
                Save();
            }
        }

        public static bool syncSelection
        {
            get => settings.syncSelection;
            set
            {
                settings.syncSelection = value;
                Save();
            }
        }

        #region Basic Interface

        public static void Save()
        {
            SetLocal(settings);
        }

        public static void Reload()
        {
            s_Settings = GetLocal();
        }

        public static void Reset()
        {
            SetLocal(s_Settings = new Settings());
        }

        #endregion

        #region Serialization & Deserialization

        internal static readonly string SerializedFilePath = string.Format(
            ProjectPinBoardManager.LocalFilePathTemplate,
            "settings"
        );

        private static Settings GetLocal()
        {
            return PipiUtility.GetLocal<Settings>(SerializedFilePath);
        }

        private static void SetLocal(Settings value)
        {
            PipiUtility.SetLocal(SerializedFilePath, value);
        }

        #endregion
    }
}
