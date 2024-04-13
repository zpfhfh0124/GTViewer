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
    public class VideoViewer : SingletonMB<VideoViewer>
    {
        // 동영상 렌더러
        VideoPlayer _videoPlayer;
        RawImage _rawImage;
        RectTransform _rawImgRectTransform;
        RectTransform _canvasRectTransform;
        [SerializeField] Canvas _canvas;
        [SerializeField] GameObject _videoBoard;
        Texture2D _videoTexture; // 웹에서 받은 비디오 텍스쳐 저장용

        // 현재 스크린 사이즈 저장용
        float _screenWidth;
        float _screenHeight;

        // 버튼
        [SerializeField] Button _btn_ImageViewer;
        [SerializeField] Button _btn_play;
        [SerializeField] Button _btn_playList;
        [SerializeField] Button _btn_prevVideo;
        [SerializeField] Button _btn_nextVideo;
        [SerializeField] Button _btn_URL;

        // 오브젝트
        [SerializeField] GameObject _obj_img_play;
        [SerializeField] GameObject _obj_img_guide;

        // 슬라이더
        [SerializeField] Slider _slider_time;
        [SerializeField] Slider _slider_volume;

        // 텍스트
        [SerializeField] TextMeshProUGUI _text_lengthTime;
        [SerializeField] TextMeshProUGUI _text_currentTime;
        [SerializeField] TextMeshProUGUI _text_fileName;

        // 외부 클래스
        FileDragAndDrop _cs_fileDragAndDrop;
        [SerializeField] VideoPlayList _cs_videoPlayList;
        [SerializeField] VideoUrlSetting _cs_videoUrlSettings;

        // URL 로컬 저장 디렉토리
        string _downloadPath;
        
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

            _btn_prevVideo.onClick.AddListener(() =>
            {
                OnNextPrevVideo(true);
            });

            _btn_nextVideo.onClick.AddListener(() =>
            {
                OnNextPrevVideo();
            });

            _btn_URL.onClick.AddListener(() =>
            {
                OnUrlSettings();
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
            else if (Input.GetKeyDown(KeyCode.PageUp))
            {
                OnNextPrevVideo(true);
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                OnNextPrevVideo();
            }

            // 재생중일 경우 
            if (_videoPlayer != null && _videoPlayer.isPlaying)
            {
                // 재생시간 계산
                SetTime(_videoPlayer.length, _videoPlayer.time);

                // 타임슬라이더 현재 타임라인으로 조정
                float rate = (float)_videoPlayer.time / (float)_videoPlayer.length;
                _slider_time.SetValueWithoutNotify(rate);
            }

            // 화면 사이즈가 조정되었을 경우 (크기 조절중에는 호출 안되도록)
            if( !Input.GetMouseButton(0) && _canvasRectTransform != null &&
                (_screenWidth != _canvasRectTransform.sizeDelta.x || _screenHeight != _canvasRectTransform.sizeDelta.y) )
            {
                ResizeVideo();
            }
        }

        public void SetEnable(bool isEnable)
        {
            _videoBoard.SetActive(isEnable);
            gameObject.SetActive(isEnable);
        }

        void Init()
        {
            _text_fileName.text = "동영상 파일을 드래그 앤 드롭";
            _obj_img_play.SetActive(false);
            _obj_img_guide.SetActive(true);
            _cs_videoPlayList.SetEnable(false);
            _cs_videoUrlSettings.gameObject.SetActive(false);

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

            // 동영상 재생 완료 이벤트 등록
            _videoPlayer.loopPointReached += (e) => { OnNextPrevVideo(); };
        }

        public void SetVideo(string filePath, bool isLeft = false)
        {
            // 파일이 동영상 파일인지 검사(avi, mp4, wav, flv...)
            if (MainController.Instance.CheckFileExtension(ViewMode.VIDEO, filePath) == false) 
                return;

            _videoTexture = null;
            _videoPlayer.url = MainController.Instance.GetLocalFileURL(filePath);
            _text_fileName.text = MainController.Instance.GetFileName(_videoPlayer.url);
            _obj_img_guide.SetActive(false);
            _videoPlayer.Prepare();
            StartCoroutine(CoroutineWaitVideoPrepared());
        }

        IEnumerator CoroutineWaitVideoPrepared()
        {
            yield return new WaitUntil(() => _videoPlayer.isPrepared);
            InitPlayVideo();
        }

        void InitPlayVideo()
        {
            SetVideoRenderer();
            ResizeVideo();
            _slider_volume.SetValueWithoutNotify(_videoPlayer.GetDirectAudioVolume(0));
            SetTime(_videoPlayer.length, _videoPlayer.time);
            SetPlayPause();
            SetPlayList();
        }
        
        void SetVideoRenderer()
        {
            _videoPlayer.targetTexture = new RenderTexture((int)_videoPlayer.width, (int)_videoPlayer.height, 1);
            _rawImage = _videoBoard.GetComponent<RawImage>();
            if (_rawImage == null)
            {
                Debug.LogError($"{_videoBoard.name}의 RawImage가 Null이다!");
            }

            // Texture로 재생할 경우
            if (_videoTexture != null) 
            {
                _rawImage.texture = _videoTexture;
            }
            else
            {
                _rawImage.texture = _videoPlayer.targetTexture;
            }

            _rawImgRectTransform = _videoBoard.GetComponent<RectTransform>();
            if (_rawImgRectTransform == null)
            {
                Debug.LogError($"{_videoBoard.name}의 RectTransform Null이다!");
            }

            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
            if (_canvasRectTransform == null)
            {
                Debug.LogError($"{_canvas.name}의 RectTransform Null이다!");
            }

            _screenWidth = _canvasRectTransform.sizeDelta.x;
            _screenHeight = _canvasRectTransform.sizeDelta.y;
        }

        // 동영상 비율에 맞춰서 크기 조정 (빌드 앱에서만)
        void ResizeVideo()
        {
//#if UNITY_STANDALONE && !UNITY_EDITOR
            // 비율 맞추기
            // 현재 화면의 가로와 세로 길이
            float screenWidth = _canvasRectTransform.sizeDelta.x;
            float screenHeight = _canvasRectTransform.sizeDelta.y;/* - 150; // 창 최대화하면 위 아래가 잘리기 때문에 조정*/
            Debug.Log($"현재 화면 크기 : {screenWidth}*{screenHeight}");

            // 현재 동영상의 너비와 높이
            float videoWidth = _videoPlayer.width;
            float videoHeight = _videoPlayer.height;
            Debug.Log($"동영상 크기 : {videoWidth}*{videoHeight}");
            
            // 동영상 가로 기준 비율
            float aspectWidthRatio = videoWidth / videoHeight;
            // 동영상 세로 기준 비율
            float aspectHeightRatio = videoHeight / videoWidth;

            // RawImage의 크기를 화면에 꽉 차도록 설정
            float rawImageWidth = screenWidth;
            float rawImageHeight = screenHeight;

            // 가로가 긴 영상
            if( videoWidth > videoHeight )
            {
                rawImageHeight = aspectHeightRatio * screenWidth;
            }
            // 세로가 긴 영상
            else
            {
                rawImageWidth = aspectWidthRatio * screenHeight;
            }
            
            // RawImage의 크기 설정
            _screenWidth = _canvasRectTransform.sizeDelta.x;
            _screenHeight = _canvasRectTransform.sizeDelta.y;
            _rawImgRectTransform.sizeDelta = new Vector2(rawImageWidth, rawImageHeight);
            Debug.Log($"수정된 화면 크기 : {_screenWidth}*{_screenHeight}");
            Debug.Log($"수정된 동영상 크기 : {_rawImgRectTransform.sizeDelta.x}*{_rawImgRectTransform.sizeDelta.y}");
//#endif
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

        void OnNextPrevVideo(bool isPrev = false)
        {
            string curFilePath = MainController.Instance.SetFileProtocolFileURL(_videoPlayer.url, true);
            List<string> curFileList = _cs_videoPlayList.GetVideoPlayList();
            string findFile = MainController.Instance.FindNextPrevFile(curFileList, curFilePath, ViewMode.VIDEO, isPrev);
            if (_videoPlayer.isPlaying) SetPlayPause();
            SetVideo(findFile);
        }

        void OnUrlSettings()
        {
            _cs_videoUrlSettings.gameObject.SetActive(true);
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
