using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;

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

            if ( request.result == UnityWebRequest.Result.ConnectionError ||
                 request.result == UnityWebRequest.Result.ProtocolError )
            {
                Debug.LogError($"다운로드 실패 : {url} \n {request.error}");
            }
            else
            {
                File.WriteAllBytes(_createFilePath, request.downloadHandler.data);
                Debug.Log($"다운로드 성공 : {url} \n {request.result} \n {_createFilePath}");

                yield return null;

                callback(url);
            }
        }
    }
}