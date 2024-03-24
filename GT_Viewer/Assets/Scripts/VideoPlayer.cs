using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GT
{
    public class VideoPlayer : MonoBehaviour
    {
        UnityEngine.Video.VideoPlayer _unityVideoPlayer;
        RenderTexture _renderTexture;

        [SerializeField] Button _btn_ImageViewer;

        void Start()
        {
            _btn_ImageViewer.onClick.AddListener(() =>
            {
                GoToImageViewerScene();
            });

            Init();
        }

        private void OnEnable()
        {
            FileDragAndDrop.DropedFileEvent += SetVideo;
        }

        private void OnDisable()
        {
            FileDragAndDrop.DropedFileEvent -= SetVideo;
        }

        void Init()
        {
            _unityVideoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
            if(_unityVideoPlayer == null)
            {
                Debug.LogError($"현재 {gameObject.name}에 UnityEngine VideoPlayer 컴포넌트가 안붙어있다.");
                return;
            }

            _renderTexture = _unityVideoPlayer.targetTexture;
            if(_renderTexture == null)
            {
                Debug.LogError($"Render Texture가 Null이다!");
                return;
            }
        }

        void SetVideo(string filePath, bool isLeft)
        {
            // 파일이 동영상 파일인지 검사(avi, mp4, wav, flv...)
            string extension = filePath.Split('.').Last();
            extension = extension.ToLower();
            List<string> list_video_extension = new List<string> { "avi", "mp4", "mov", "wmv", "flv", "mkv" };
            if (list_video_extension.Exists(x => x == extension))
            {
                Debug.Log($"드롭된 파일의 확장자가 {extension} 동영상 드롭 성공!");
            }
            else
            {
                Debug.LogWarning($"드롭된 파일의 확장자가 {extension}으로 동영상 파일이 아닙니다.");
                return;
            }

            // VideoPlayer URL에 세팅 -> url형식 맞춰주기
            // file:// 문자열 앞에 붙혀야함
            // \ -> / 문자로 교체
            Debug.Log($"{filePath}");

            filePath = filePath.Replace('\\', '/');
            filePath = $"file://{filePath}";
            
            Debug.Log($"정제된 path - {filePath}");
            _unityVideoPlayer.url = filePath;
        }

        void GoToImageViewerScene()
        {
            SceneControler.Instance.SceneChange(SceneList.SCENE_IMAGE_VIEWER);
        }
    }
}
