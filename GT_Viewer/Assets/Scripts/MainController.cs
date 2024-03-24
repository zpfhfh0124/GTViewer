using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] GT.VideoViewer _videoPlayer;

        static List<string> _dontDestroyObjectNames = new List<string>();

        private void Awake()
        {
            Instance = this;
            SetMode(ViewMode.IMAGE);
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
    }
}