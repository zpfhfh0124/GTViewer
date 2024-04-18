using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

namespace GT
{
    public class VideoUrlSetting : MonoBehaviour
    {
        // 텍스트
        [SerializeField] TMP_InputField _InputField_videoUrl;
        [SerializeField] TMP_InputField _InputField_downloadPath;

        // 버튼
        [SerializeField] Button _btn_inputUrlAndPath;
        [SerializeField] Button _btn_close;

        // 웹 리퀘스트 받고 처리할 콜백 (동영상 Path URL)
        Action<string> WebRequestGetCallback;

        // _InputField_downloadPath 입력 안할 때 기본 다운로드 경로
        string _download_base_path = "Assets/Resources/Downloads/Video/";

        private void Start()
        {
            _btn_close.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });

            _btn_inputUrlAndPath.onClick.AddListener(() =>
            {
                SendWebRequestUrl();
            });
        }

        void SendWebRequestUrl()
        {
            string url = _InputField_videoUrl.text;
            string localPath = _InputField_downloadPath.text;

            if (string.IsNullOrEmpty(localPath))
            {
                string formatDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                localPath = $"{_download_base_path}new_video_{formatDateTime}.mp4";
            }
            WebRequestGetCallback = (getPathUrl) =>
            {
                VideoViewer.Instance.SetVideo(getPathUrl);
            };

            WebRequestManager.Instance.WebRequestGet(url, localPath, WebRequestGetCallback);
        }

    }
}