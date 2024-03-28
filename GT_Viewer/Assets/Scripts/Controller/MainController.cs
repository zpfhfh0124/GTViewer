using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GT
{
    public enum ViewMode
    {
        IMAGE,
        VIDEO
    }

    public class MainController : MonoBehaviour
    {
        static public MainController Instance { get; private set; }

        [SerializeField] ImageViewer _imageViewer;
        [SerializeField] VideoViewer _videoPlayer;

        static List<string> _dontDestroyObjectNames = new List<string>();

        // GUI Log 표시
        static bool _useLog = false;
        static public bool UseLog { get { return _useLog; } set { _useLog = value; } }

        GUIStyle _log_guiStyle;
        public List<string> _log = new List<string>();

        private void Awake()
        {
            Instance = this;
            SetMode(ViewMode.VIDEO);
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleAppLog;

            _log_guiStyle = new GUIStyle("label");
            _log_guiStyle.fontSize = 15;
            _log_guiStyle.fontStyle = FontStyle.Bold;
            _log_guiStyle.normal.textColor = Color.gray;
            _log_guiStyle.normal.background = new Texture2D(Screen.width / 2, Screen.height / 2);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _useLog = !_useLog;
            }
        }

        public void AddLog(string log)
        {
            _log.Add(log);
        }

        void HandleAppLog(string log, string stackTrace, LogType logType)
        {
            Color color = Color.gray;

            switch (logType)
            {
                case LogType.Log:
                    color = Color.green;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    color = Color.red;
                    break;
                case LogType.Assert:
                    color = Color.blue;
                    break;
                case LogType.Warning:
                    color = Color.yellow;
                    break;
            }

            log = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{log}</color>";
            AddLog(log);
        }

        public void SetMode(ViewMode mode)
        {
            switch (mode)
            {
                case ViewMode.IMAGE:
                    _imageViewer.SetEnable(true);
                    _videoPlayer.SetEnable(false);
                    break;
                case ViewMode.VIDEO:
                    _imageViewer.SetEnable(false);
                    _videoPlayer.SetEnable(true);
                    break;
                default:
                    break;
            }
        }

        // 파일 확장자 검사
        public bool CheckFileExtension(ViewMode viewMode, string filePath)
        {
            List<string> list_img_extension = new List<string> { "jpg", "jpeg", "png", "gif" };
            List<string> list_video_extension = new List<string> { "avi", "mp4", "mov", "wmv", "flv", "mkv" };
            List<string> list_extension = new List<string>();
            switch (viewMode)
            {
                case ViewMode.IMAGE:
                    list_extension = list_img_extension;
                    break;
                case ViewMode.VIDEO:
                    list_extension = list_video_extension;
                    break;
            }

            string extension = filePath.Split('.').Last();
            extension = extension.ToLower();
            
            if (list_extension.Exists(x => x == extension))
            {
                Debug.Log($"드롭된 파일의 확장자가 {extension}. {viewMode} 드롭 성공!");
                return true;
            }
            else
            {
                Debug.LogWarning($"드롭된 파일의 확장자가 {extension}으로 {viewMode} 파일이 아닙니다.");
                return false;
            }
        }

        /*public List<string> GetDirectoryFileList(string filePath)
        {
            string cur_directory = string.Empty;
            
        }*/

        private void OnGUI()
        {
            if (_useLog)
            {
                if (GUILayout.Button("clear log"))
                    _log.Clear();

                foreach (var s in _log)
                {
                    GUILayout.Label(s);
                }
            }
        }
    }
}