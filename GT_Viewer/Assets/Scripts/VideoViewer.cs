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

        float _elapsedTime = 0;

        void Start()
        {
            _btn_ImageViewer.onClick.AddListener(() =>
            {
                MainController.Instance.SetMode(ViewMode.IMAGE);
            });

            _slider.onValueChanged.AddListener((value) =>
            {
                SetTimeSlider(value);
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
            // 키 입력
            // 입력 키에 따른 함수 호출
            if (Input.GetKeyDown(KeyCode.Space))
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
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                SetExplorationTimeline(-10.0f);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                SetExplorationTimeline(10.0f);
            }

            // 재생중일 경우 재생시간 계산 및 타임슬라이더 현재 타임라인으로 조정
            if (_unityVideoPlayer.isPlaying)
            {
                SetTime(_unityVideoPlayer.length, _unityVideoPlayer.time);
                
                float rate = (float)_unityVideoPlayer.time / (float)_unityVideoPlayer.length;
                _slider.SetValueWithoutNotify(rate);
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
            // file:// 문자열 앞에 붙혀야함 \ -> / 문자로 교체
            Debug.Log($"{filePath}");
            filePath = filePath.Replace('\\', '/');
            filePath = $"file://{filePath}";
            Debug.Log($"정제된 path - {filePath}");
            _unityVideoPlayer.url = filePath;

            SetVideoRatio();
            SetTime(_unityVideoPlayer.length, _unityVideoPlayer.time);
            
            _unityVideoPlayer.Play();

        }

        void SetVideoRatio()
        {
            RectTransform rectTransform = _videoBoard.GetComponent<RectTransform>();
            if(rectTransform == null)
            {
                Debug.LogError($"VideoBoard에 RectTransform이 Null이다!");
                return;
            }

            Texture texture = _videoBoard.GetComponent<RawImage>().texture;
            if(texture == null)
            {
                Debug.LogError($"VideoBoard에 RawImage의 Texture가 Null이다!");
                return;
            }

            float widthRatio = (float)rectTransform.sizeDelta.x / texture.width;
            float rectHeight = widthRatio * texture.height;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectHeight);
        }

        /// <summary>
        /// 영상 조작 관련
        /// </summary>
        void SetPlayPause()
        {
            if (_unityVideoPlayer.isPlaying)
            {
                _unityVideoPlayer.Pause();
            }
            else
            {
                _unityVideoPlayer.Play();
            }
        }

        void SetVolume(float value)
        {
            if (value == 0) return;

            _unityVideoPlayer.SetDirectAudioVolume(0, _unityVideoPlayer.GetDirectAudioVolume(0) + value);
        }

        void SetExplorationTimeline(float value)
        {
            _unityVideoPlayer.time += value;
        }

        void SetTimeSlider(float sliderRate)
        {
            if (_unityVideoPlayer == null) return;

            _unityVideoPlayer.time = sliderRate * _unityVideoPlayer.length;
        }

        void SetTime(double lengthTime, double currentTime)
        {
            var lengthDateTime = TimeSpan.FromSeconds(lengthTime);
            var currentDateTime = TimeSpan.FromSeconds(currentTime);

            _lengthTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", lengthDateTime.Hours, lengthDateTime.Minutes, lengthDateTime.Seconds);
            _currentTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentDateTime.Hours, currentDateTime.Minutes, currentDateTime.Seconds);
        }
    }
}
