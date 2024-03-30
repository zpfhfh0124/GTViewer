using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GT
{
    public class VideoViewer : MonoBehaviour
    {
        VideoPlayer _videoPlayer;

        [SerializeField] VideoPlayList _cs_videoPlayList;
        FileDragAndDrop _cs_fileDragAndDrop;

        [SerializeField] GameObject _videoBoard;

        [SerializeField] Button _btn_ImageViewer;
        [SerializeField] Button _btn_play;
        [SerializeField] Button _btn_playList;

        [SerializeField] GameObject _obj_img_play;
        [SerializeField] GameObject _obj_img_guide;

        [SerializeField] Slider _slider_time;
        [SerializeField] Slider _slider_volume;

        [SerializeField] TextMeshProUGUI _text_lengthTime;
        [SerializeField] TextMeshProUGUI _text_currentTime;

        void Start()
        {
            _btn_ImageViewer.onClick.AddListener(() =>
            {
                MainController.Instance.SetMode(ViewMode.IMAGE);
            });

            _slider_time.onValueChanged.AddListener((value) =>
            {
                SetTimeSlider(value);
            });

            _slider_volume.onValueChanged.AddListener((value) =>
            {
                SetVolume(value, true);
            });

            _btn_play.onClick.AddListener(() =>
            {
                SetPlayPause();
            });

            _btn_playList.onClick.AddListener(() =>
            {
                OnPlayList();
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
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SetVolume(0.05f);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SetVolume(-0.05f);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                SetExplorationTimeline(-10.0f);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                SetExplorationTimeline(10.0f);
            }

            // 재생중일 경우 
            if (_videoPlayer.isPlaying)
            {
                // 재생시간 계산
                SetTime(_videoPlayer.length, _videoPlayer.time);

                // 타임슬라이더 현재 타임라인으로 조정
                float rate = (float)_videoPlayer.time / (float)_videoPlayer.length;
                _slider_time.SetValueWithoutNotify(rate);
            }
        }

        public void SetEnable(bool isEnable)
        {
            _videoBoard.SetActive(isEnable);
            gameObject.SetActive(isEnable);
        }

        void Init()
        {
            _obj_img_play.SetActive(false);
            _obj_img_guide.SetActive(true);
            _cs_videoPlayList.SetEnable(false);

            _videoPlayer = GetComponent<VideoPlayer>();
            if(_videoPlayer == null)
            {
                Debug.LogError($"현재 {gameObject.name}에 UnityEngine VideoPlayer 컴포넌트가 안붙어있다.");
                return;
            }

            _cs_fileDragAndDrop = FindObjectOfType<FileDragAndDrop>();
            if (_cs_fileDragAndDrop == null)
            {
                Debug.LogError($"현재 {gameObject.scene}에 FileDragAndDrop 오브젝트가 없다.");
                return;
            }
        }

        void SetVideo(string filePath, bool isLeft)
        {
            // 파일이 동영상 파일인지 검사(avi, mp4, wav, flv...)
            if (MainController.Instance.CheckFileExtension(ViewMode.VIDEO, filePath) == false) 
                return;

            _videoPlayer.url = MainController.Instance.GetLocalFileURL(filePath);
            _obj_img_guide.SetActive(false);

            _videoPlayer.Prepare();
            StartCoroutine(CoroutineWaitVideoPrepared());
        }

        IEnumerator CoroutineWaitVideoPrepared()
        {
            yield return new WaitUntil(() => _videoPlayer.isPrepared);
            ResizeVideo();
            _slider_volume.SetValueWithoutNotify(_videoPlayer.GetDirectAudioVolume(0));
            SetTime(_videoPlayer.length, _videoPlayer.time);
            SetPlayPause();
            SetPlayList();
        }
        
        // 동영상 비율에 맞춰서 크기 조정 (빌드 앱에서만)
        void ResizeVideo()
        {
            int newWidth = (int)_videoPlayer.width;
            int newHeight = (int)_videoPlayer.height;

            _videoPlayer.targetTexture = new RenderTexture(newWidth, newHeight, 1);
            RawImage rawImage = _videoBoard.GetComponent<RawImage>();
            if(rawImage == null)
            {
                Debug.LogError($"{_videoBoard.name}의 RawImage가 Null이다!");
            }

            rawImage.texture = _videoPlayer.targetTexture;

            RectTransform rectTransform = _videoBoard.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"{_videoBoard.name}의 RectTransform Null이다!");
            }

#if UNITY_STANDALONE && !UNITY_EDITOR
            // 비율 맞추기
            // 현재 화면의 가로와 세로 길이
            float screenWidth = Screen.width;
            float screenHeight = Screen.height - 150; // 창 최대화하면 위 아래가 잘리기 때문에 조정
            Debug.Log($"현재 화면 크기 : {screenWidth}*{screenHeight}");

            // 현재 동영상의 너비와 높이
            float videoWidth = _videoPlayer.width;
            float videoHeight = _videoPlayer.height;
            Debug.Log($"동영상 크기 : {videoWidth}*{videoHeight}");
            
            // 동영상 비율
            float videoAspectRatio = videoWidth / videoHeight;

            // RawImage의 크기를 화면에 꽉 차도록 설정
            float rawImageWidth = screenWidth;
            float rawImageHeight = screenHeight;

            // 비디오의 가로/세로 비율에 따라 RawImage의 크기 조정
            if (videoAspectRatio > screenWidth / screenHeight)
            {
                // 비디오의 가로/세로 비율이 화면보다 더 가로 방향에 가까운 경우
                rawImageHeight = screenWidth / videoAspectRatio;
            }
            else
            {
                // 비디오의 가로/세로 비율이 화면보다 더 세로 방향에 가까운 경우
                rawImageWidth = screenHeight * videoAspectRatio;
            }
            // RawImage의 크기 설정
            rectTransform.sizeDelta = new Vector2(rawImageWidth, rawImageHeight);
#endif
        }

        void SetPlayList()
        {
            if ( string.IsNullOrEmpty(_videoPlayer.url) )
            {
                Debug.LogWarning("현재 재생중인 동영상 파일이 없다.");
                return;
            }

            List<string> cur_fileList = MainController.Instance.GetDirectoryFileList(_videoPlayer.url, ViewMode.VIDEO);
            _cs_videoPlayList.SetVideoPlayList(cur_fileList);
        }

        void OnPlayList()
        {
            _cs_videoPlayList.SetEnable(true);

            //_cs_videoPlayList.SetVideoPlayList()
        }

        /// <summary>
        /// 영상 조작 관련
        /// </summary>
        void SetPlayPause()
        {
            if (_videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
                _obj_img_play.SetActive(true);
            }
            else
            {
                _videoPlayer.Play();
                _obj_img_play.SetActive(false);
            }
        }

        void SetVolume(float value, bool isSlider = false)
        {
            if (value == 0) return;

            if(isSlider) _videoPlayer.SetDirectAudioVolume(0, value);
            else
            {
                _videoPlayer.SetDirectAudioVolume(0, _videoPlayer.GetDirectAudioVolume(0) + value);
                _slider_volume.value = _videoPlayer.GetDirectAudioVolume(0);
            }

            //float cur_volume = _unityVideoPlayer.GetDirectAudioVolume(0);
            //Debug.Log($"현재 볼륨 : {cur_volume} | 음량조절슬라이더 : {sliderValue}");
        }

        void SetExplorationTimeline(float value)
        {
            _videoPlayer.time += value;
        }

        void SetTime(double lengthTime, double currentTime)
        {
            var lengthDateTime = TimeSpan.FromSeconds(lengthTime);
            var currentDateTime = TimeSpan.FromSeconds(currentTime);

            _text_lengthTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", lengthDateTime.Hours, lengthDateTime.Minutes, lengthDateTime.Seconds);
            _text_currentTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentDateTime.Hours, currentDateTime.Minutes, currentDateTime.Seconds);
        }

        /// <summary>
        /// 슬라이더 조작 관련
        /// </summary>
        void SetTimeSlider(float sliderRate)
        {
            if (_videoPlayer == null) return;

            _videoPlayer.time = sliderRate * _videoPlayer.length;
        }
    }
}
