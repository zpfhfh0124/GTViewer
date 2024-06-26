using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public class MainController : SingletonMB<MainController>
    {
        [SerializeField] ImageViewer _imageViewer;
        [SerializeField] VideoViewer _videoPlayer;

        // GUI Log 표시
        static bool _useLog = false;
        static public bool UseLog { get { return _useLog; } set { _useLog = value; } }

        GUIStyle _log_guiStyle;
        public List<string> _log = new List<string>();

        private void Awake()
        {
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
            List<string> list_video_extension = new List<string> { "avi", "mp4", "mov", "wmv", "flv", "mkv", "m4v" };
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
                //Debug.Log($"드롭된 파일의 확장자가 {extension}. {viewMode} 성공!");
                return true;
            }
            else
            {
                Debug.LogWarning($"드롭된 파일의 확장자가 {extension}으로 {viewMode} 파일이 아닙니다.");
                return false;
            }
        }

        public string SetFileProtocolFileURL(string filePath, bool isExcludeProtocol = false)
        {
            if (isExcludeProtocol == false)
            {
                filePath = $"file://{filePath}";
            }
            else
            {
                filePath = filePath.Replace("file://", "");
            }

            return filePath;
        }

        public string GetLocalFileURL(string filePath, ViewMode viewMode = ViewMode.VIDEO)
        {
            // VideoPlayer URL에 세팅 -> url형식 맞춰주기
            // file:// 문자열 앞에 붙혀야함 \ -> / 문자로 교체
            Debug.Log($"{filePath}");
            filePath = filePath.Replace('\\', '/');
            if (viewMode == ViewMode.VIDEO && !filePath.Contains("file://")) filePath = SetFileProtocolFileURL(filePath);
            Debug.Log($"정제된 path - {filePath}");
            return filePath;
        }

        // 경로 제거한 파일명 반환
        public string GetFileName(string filePath)
        {
            return filePath.Split('/').Last();
        }

        // 해당 파일의 디렉토리 하위 파일들 탐색
        public List<string> GetDirectoryFileList(string filePath, ViewMode viewMode)
        {
            // 동영상일 경우 file:// 프로토콜 제거 
            if(filePath.Contains("file://")) filePath = SetFileProtocolFileURL(filePath, true);

            if (filePath.Contains('/'))
            {
                filePath = filePath.Replace(filePath.Split('/').Last(), "");
            }
            else
            {
                filePath = filePath.Replace(filePath.Split('\\').Last(), "");
            }

            string[] cur_Directory_arr = Directory.GetFiles(filePath);
            List<string> cur_directory = cur_Directory_arr.ToList<string>();            
            if(cur_directory == null || cur_directory.Count <= 0)
            {
                Debug.LogWarning($"현재 파일{filePath} 경로에 {viewMode}파일이 없다");
                return cur_directory;
            }

            List<string> filt_list = new List<string>();
            foreach (var file in cur_directory)
            {
                bool isExtension = CheckFileExtension(viewMode, file);
                Debug.Log($"현재 경로 탐색 파일 : {file} | {viewMode} : {isExtension}");
                
                if (isExtension == true) filt_list.Add(file); 
            }

            return filt_list;
        }

        // 인접된 파일 탐색 후 재생 (이전, 다음)
        public string FindNextPrevFile(List<string> fileList, string currFile, ViewMode viewMode, bool isPrev = false)
        {
            if (isPrev) fileList = fileList.Reverse<string>().ToList();

            string findFile = string.Empty;
            
            if (!fileList.Exists(x => x == currFile)) 
            {
                Debug.LogWarning($"현재 찾으려고 하는 파일이 파일 리스트에 없다!");
                return string.Empty;
            }

            bool isFind = false;
            foreach (var file in fileList)
            {
                if(isFind)
                {
                    findFile = file;
                    break;
                }

                if (file == currFile) 
                {
                    isFind = true;
                }
            }

            if(viewMode == ViewMode.VIDEO)
            {
                findFile = SetFileProtocolFileURL(findFile);
            }

            string log = isPrev ? $"이전 파일 {findFile}을 찾았습니다." : $"다음 파일 {findFile}을 찾았습니다.";
            Debug.Log(log);

            return findFile;
        }

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