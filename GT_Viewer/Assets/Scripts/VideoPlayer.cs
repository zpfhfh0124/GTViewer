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
                Debug.LogError($"���� {gameObject.name}�� UnityEngine VideoPlayer ������Ʈ�� �Ⱥپ��ִ�.");
                return;
            }

            _renderTexture = _unityVideoPlayer.targetTexture;
            if(_renderTexture == null)
            {
                Debug.LogError($"Render Texture�� Null�̴�!");
                return;
            }
        }

        void SetVideo(string filePath, bool isLeft)
        {
            // ������ ������ �������� �˻�(avi, mp4, wav, flv...)
            string extension = filePath.Split('.').Last();
            extension = extension.ToLower();
            List<string> list_video_extension = new List<string> { "avi", "mp4", "mov", "wmv", "flv", "mkv" };
            if (list_video_extension.Exists(x => x == extension))
            {
                Debug.Log($"��ӵ� ������ Ȯ���ڰ� {extension} ������ ��� ����!");
            }
            else
            {
                Debug.LogWarning($"��ӵ� ������ Ȯ���ڰ� {extension}���� ������ ������ �ƴմϴ�.");
                return;
            }

            // VideoPlayer URL�� ���� -> url���� �����ֱ�
            // file:// ���ڿ� �տ� ��������
            // \ -> / ���ڷ� ��ü
            Debug.Log($"{filePath}");

            filePath = filePath.Replace('\\', '/');
            filePath = $"file://{filePath}";
            
            Debug.Log($"������ path - {filePath}");
            _unityVideoPlayer.url = filePath;
        }

        void GoToImageViewerScene()
        {
            SceneControler.Instance.SceneChange(SceneList.SCENE_IMAGE_VIEWER);
        }
    }
}
