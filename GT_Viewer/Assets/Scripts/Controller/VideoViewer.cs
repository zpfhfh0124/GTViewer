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

        [SerializeField] VideoPlayList _cs_videoPlayList;

        [SerializeField] GameObject _videoBoard;

        [SerializeField] Button _btn_ImageViewer;
        [SerializeField] Button _btn_play;
        [SerializeField] Button _btn_playList;

        [SerializeField] GameObject _obj_img_play;
        [SerializeField] GameObject _obj_img_guide;

        [SerializeField] Slider _slider_time;
        [SerializeField] Slider _slider_volume;

        [SerializeField] Text _lengthTime;
        [SerializeField] Text _currentTime;

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
            if (_unityVideoPlayer.isPlaying)
            {
                // 재생시간 계산
                SetTime(_unityVideoPlayer.length, _unityVideoPlayer.time);

                // 타임슬라이더 현재 타임라인으로 조정
                float rate = (float)_unityVideoPlayer.time / (float)_unityVideoPlayer.length;
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
            if (MainController.Instance.CheckFileExtension(ViewMode.VIDEO, filePath) == false) 
                return;

            SetLocalFileURL(filePath);

            _obj_img_guide.SetActive(false);

            SetVideoRatio();
            _unityVideoPlayer.Prepare();
            StartCoroutine(CoroutineWaitVideoPrepared());
        }

        void SetLocalFileURL(string filePath)
        {
            // VideoPlayer URL에 세팅 -> url형식 맞춰주기
            // file:// 문자열 앞에 붙혀야함 \ -> / 문자로 교체
            Debug.Log($"{filePath}");
            filePath = filePath.Replace('\\', '/');
            filePath = $"file://{filePath}";
            Debug.Log($"정제된 path - {filePath}");
            _unityVideoPlayer.url = filePath;
        }

        IEnumerator CoroutineWaitVideoPrepared()
        {
            yield return new WaitUntil(() => _unityVideoPlayer.isPrepared);

            _slider_volume.SetValueWithoutNotify(_unityVideoPlayer.GetDirectAudioVolume(0));
            SetTime(_unityVideoPlayer.length, _unityVideoPlayer.time);
            SetPlayPause();
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

        void OnPlayList()
        {
            _cs_videoPlayList.SetEnable(true);
        }

        /// <summary>
        /// 영상 조작 관련
        /// </summary>
        void SetPlayPause()
        {
            if (_unityVideoPlayer.isPlaying)
            {
                _unityVideoPlayer.Pause();
                _obj_img_play.SetActive(true);
            }
            else
            {
                _unityVideoPlayer.Play();
                _obj_img_play.SetActive(false);
            }
        }

        void SetVolume(float value, bool isSlider = false)
        {
            if (value == 0) return;

            if(isSlider) _unityVideoPlayer.SetDirectAudioVolume(0, value);
            else
            {
                _unityVideoPlayer.SetDirectAudioVolume(0, _unityVideoPlayer.GetDirectAudioVolume(0) + value);
                _slider_volume.value = _unityVideoPlayer.GetDirectAudioVolume(0);
            }

            //float cur_volume = _unityVideoPlayer.GetDirectAudioVolume(0);
            //Debug.Log($"현재 볼륨 : {cur_volume} | 음량조절슬라이더 : {sliderValue}");
        }

        void SetExplorationTimeline(float value)
        {
            _unityVideoPlayer.time += value;
        }

        void SetTime(double lengthTime, double currentTime)
        {
            var lengthDateTime = TimeSpan.FromSeconds(lengthTime);
            var currentDateTime = TimeSpan.FromSeconds(currentTime);

            _lengthTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", lengthDateTime.Hours, lengthDateTime.Minutes, lengthDateTime.Seconds);
            _currentTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentDateTime.Hours, currentDateTime.Minutes, currentDateTime.Seconds);
        }

        /// <summary>
        /// 슬라이더 조작 관련
        /// </summary>
        void SetTimeSlider(float sliderRate)
        {
            if (_unityVideoPlayer == null) return;

            _unityVideoPlayer.time = sliderRate * _unityVideoPlayer.length;
        }
    }
}
