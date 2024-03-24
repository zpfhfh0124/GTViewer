using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GT
{
    public class VideoViewer : MonoBehaviour
    {
        UnityEngine.Video.VideoPlayer _unityVideoPlayer;
        RenderTexture _renderTexture;

        [SerializeField] GameObject _videoBoard;

        [SerializeField] Button _btn_ImageViewer;

        [SerializeField] Slider _slider;

        [SerializeField] Text _lengthTime;
        [SerializeField] Text _currentTime;

        void Start()
        {
            _btn_ImageViewer.onClick.AddListener(() =>
            {
                MainController.Instance.SetMode(ViewMode.IMAGE);
            });

            _slider.onValueChanged.AddListener((value) =>
            {
                SetDuration(value);
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

        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                SetPlayPause();
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                SetVolume(0.1f);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                SetVolume(-0.1f);
            }
        }

        public void SetEnable(bool isEnable)
        {
            _videoBoard.SetActive(isEnable);
            gameObject.SetActive(isEnable);
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

            SetVideoRatio();
            _unityVideoPlayer.Play();
        }

        void SetVideoRatio()
        {
            RectTransform rectTransform = _videoBoard.GetComponent<RectTransform>();
            if(rectTransform == null)
            {
                Debug.LogError($"VideoBoard�� RectTransform�� Null�̴�!");
                return;
            }

            Texture texture = _videoBoard.GetComponent<RawImage>().texture;
            if(texture == null)
            {
                Debug.LogError($"VideoBoard�� RawImage�� Texture�� Null�̴�!");
                return;
            }

            float widthRatio = (float)rectTransform.sizeDelta.x / texture.width;
            float rectHeight = widthRatio * texture.height;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectHeight);
        }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        void SetPlayPause()
        {
            if (_unityVideoPlayer.isPlaying) _unityVideoPlayer.Pause();
            else if (_unityVideoPlayer.isPaused) _unityVideoPlayer.Play();
        }

        void SetVolume(float value)
        {
            if (value == 0) return;

            _unityVideoPlayer.SetDirectAudioVolume(0, _unityVideoPlayer.GetDirectAudioVolume(0) + value);
        }

        void SetDuration(float value)
        {
            double length = _unityVideoPlayer.length;
            double setTime = length * value;
            _unityVideoPlayer.time = setTime;

            SetTime(length, setTime);
        }

        void SetDuration(double value)
        {
            double length = _unityVideoPlayer.length;
            double setTime = length * value;
            _unityVideoPlayer.time = setTime;

            SetTime(length, setTime);
        }

        void SetTime(double lengthTime, double currentTime)
        {
            var lengthDateTime = TimeSpan.FromSeconds(lengthTime);
            var currentDateTime = TimeSpan.FromSeconds(currentTime);

            _lengthTime.text = string.Format($"{lengthDateTime.Hours}:{lengthDateTime.Minutes}:{lengthDateTime.Seconds}");
            _currentTime.text = string.Format($"{currentDateTime.Hours}:{currentDateTime.Minutes}:{currentDateTime.Seconds}");
        }
    }
}
