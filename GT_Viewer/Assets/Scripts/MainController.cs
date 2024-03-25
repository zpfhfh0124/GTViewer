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

        private void Awake()
        {
            Instance = this;
            SetMode(ViewMode.VIDEO);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                FileDragAndDrop.UseLog = !FileDragAndDrop.UseLog;
            }
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
    }
}