using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GT
{
    public class ImageViewer : MonoBehaviour
    {
        [SerializeField] GameObject _imageBoard;

        // UGUI Canvas
        [SerializeField] Canvas _canvas;
        RectTransform _canvasRectTransform;

        // GUI 이미지 렌더러
        [SerializeField] Image _img_left;
        [SerializeField] Image _img_right;

        // 현재 스크린 사이즈 저장용
        float _screenWidth;
        float _screenHeight;

        // 버튼
        [SerializeField] Button _btn_reset;
        [SerializeField] Button _btn_quit;
        [SerializeField] Button _btn_video;
        [SerializeField] Button _btn_leftPrev;
        [SerializeField] Button _btn_leftNext;
        [SerializeField] Button _btn_rightPrev;
        [SerializeField] Button _btn_rightNext;

        // 현재 로드된 이미지 파일 경로
        string _imgFile_left;
        string _imgFile_right;

        // 현재 로드된 이미지의 디렉토리 하위 이미지 파일 리스트
        List<string> _imgFiles_left = new List<string>();
        List<string> _imgFiles_right = new List<string>();

        void Start()
        {
            // 버튼 리스너 등록
            _btn_reset.onClick.AddListener(() =>
            {
                ResetImg();
            });

            _btn_quit.onClick.AddListener(() =>
            {
                QuitApp();
            });

            _btn_leftPrev.onClick.AddListener(() =>
            {
                OnNextPrevImg(true, true);
            });

            _btn_leftNext.onClick.AddListener(() =>
            {
                OnNextPrevImg(false, true);
            });

            _btn_rightPrev.onClick.AddListener(() =>
            {
                OnNextPrevImg(true, false);
            });

            _btn_rightNext.onClick.AddListener(() =>
            {
                OnNextPrevImg(false, false);
            });

            _btn_video.onClick.AddListener(() =>
            {
                MainController.Instance.SetMode(ViewMode.VIDEO);
            });

            InitCanvas();
        }

        private void Update()
        {
            // 화면 사이즈가 조정되었을 경우 (크기 조절중에는 호출 안되도록)
            if (!Input.GetMouseButton(0) && _canvasRectTransform != null &&
                (_screenWidth != _canvasRectTransform.sizeDelta.x || _screenHeight != _canvasRectTransform.sizeDelta.y))
            {
                ResizeScreen();
            }
        }

        private void OnEnable()
        {
            // 파일 드래그 앤 드롭의 이벤트를 추가
            FileDragAndDrop.DropedFileEvent += SetImg;
        }

        private void OnDisable()
        {
            FileDragAndDrop.DropedFileEvent -= SetImg;
        }

        public void SetEnable(bool isEnable)
        {
            _imageBoard.SetActive(isEnable);
            gameObject.SetActive(isEnable);
        }

        void ResetImg()
        {
            Texture2D default_texture = Resources.Load("album", typeof(Texture2D)) as Texture2D;

            // 스프라이트로 변환
            Rect rect = new Rect(0, 0, default_texture.width, default_texture.height);
            Sprite default_sprite = Sprite.Create(default_texture, rect, new Vector2(0.5f, 0.5f));

            _img_left.sprite = default_sprite;
            _img_right.sprite = default_sprite;
        }

        void SetImg(string imgFilePath, bool isLeft)
        {
            // 파일이 이미지 파일인지 검사(jpeg, jpg, png, gif...)
            if (MainController.Instance.CheckFileExtension(ViewMode.IMAGE, imgFilePath) == false)
                return;

            Image image;
            if (isLeft)
            {
                image = _img_left;
                _imgFile_left = imgFilePath;
            }
            else
            {
                image = _img_right;
                _imgFile_right = imgFilePath;
            }

            SetImgList(isLeft);

            // 리소스 탐색 후 텍스쳐로 변환해서 저장
            byte[] byteTexture = File.ReadAllBytes(imgFilePath);
            if (byteTexture.Length > 0)
            {
                // 텍스쳐로 저장
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(byteTexture);

                // 스프라이트로 변환
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
            }
        }

        void InitCanvas()
        {
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        }

        void ResizeScreen()
        {
            // 비율 맞추기
            // 현재 화면의 가로와 세로 길이
            float screenWidth = _canvasRectTransform.sizeDelta.x;
            float screenHeight = _canvasRectTransform.sizeDelta.y;/* - 150; // 창 최대화하면 위 아래가 잘리기 때문에 조정*/
            Debug.Log($"현재 화면 크기 : {screenWidth}*{screenHeight}");

            // 이미지 좌/우 조정   
        }

        void SetImgList( bool isLeft )
        {
            List<string> imgFiles = new List<string>();
            if (isLeft)
            {
                imgFiles = MainController.Instance.GetDirectoryFileList(_imgFile_left, ViewMode.IMAGE);
                _imgFiles_left = imgFiles;
            }
            else
            {
                imgFiles = MainController.Instance.GetDirectoryFileList(_imgFile_right, ViewMode.IMAGE);
                _imgFiles_right = imgFiles;
            }
        }

        void OnNextPrevImg(bool isPrev, bool isLeft)
        {
            List<string> curFileList = isLeft ? _imgFiles_left : _imgFiles_right;
            string curFilePath = isLeft ? _imgFile_left : _imgFile_right;
            string findFile = MainController.Instance.FindNextPrevFile(curFileList, curFilePath, ViewMode.IMAGE, isPrev);
            SetImg(findFile, isLeft);
        }

        void QuitApp()
        {
            Application.Quit();
        }
    }
}