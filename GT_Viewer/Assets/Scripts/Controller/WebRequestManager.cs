using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Text.RegularExpressions;

namespace GT
{
    public class WebRequestManager : SingletonMB<WebRequestManager>
    {
        string _createFilePath = "Assets/Downloads/new_file.png";

        void SetCreateFilePath(string filePath)
        {
            _createFilePath = filePath;
            Debug.Log($"웹 다운로드 받은 파일을 받을 디렉토리 : {_createFilePath}로 세팅");
        }

        public string GetCreateFilePath()
        {
            return _createFilePath;
        }

        public void WebRequestGet(string url, string downloadPath, Action<string> callback)
        {
            SetCreateFilePath(downloadPath);
            File.Create(_createFilePath); 
            
            StartCoroutine(DownLoadGet(url, callback));
        }

        IEnumerator DownLoadGet(string url, Action<string> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if ( request.result == UnityWebRequest.Result.Success )
            {
                File.WriteAllBytes(_createFilePath, request.downloadHandler.data);

                yield return null;
                Debug.Log($"다운로드 성공 : {url} \n {request.result} \n {_createFilePath}");

                callback(url);
            }
            else
            {
                Debug.LogError($"다운로드 실패 : {url} \n {request.error}");
            }
        }

        string ExtractVideoURL(string html)
        {
            // 동영상 태그에서 소스(src) 속성을 찾아 동영상 URL 추출
            Regex regex = new Regex(@"<video.*?src=[""']([^""']+)[""'].*?>");
            Match match = regex.Match(html);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}