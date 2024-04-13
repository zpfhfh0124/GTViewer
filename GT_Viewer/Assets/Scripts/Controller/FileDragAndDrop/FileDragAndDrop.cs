using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System;

namespace GT
{
    public class FileDragAndDrop : SingletonMB<FileDragAndDrop>
    {
        // 외부에서 드롭 시점에 실행시킬 이벤트 접근자
        public delegate void DropedEventHandler(string filePath, bool isLeft);
        static public event DropedEventHandler DropedFileEvent;

        // 드롭된 파일 경로 (최근 파일)
        string _dropedFilePath;
        public string DropedFilePath { get { return _dropedFilePath; } }

        void OnEnable()
        {
            // must be installed on the main thread to get the right thread id.
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        }
        void OnDisable()
        {
            UnityDragAndDropHook.UninstallHook();
        }

        void OnFiles(List<string> aFiles, POINT aPos)
        {
            // 드롭된 파일이 없거나, 위치가 잘못된 경우 예외처리
            if (aFiles.Count <= 0 || aFiles == null || aPos.x < 0 || aPos.y < 0)
            {
                string fileName = (aFiles != null && aFiles.Count > 0) ? aFiles[0] : "no file";
                string log = $"파일이 제대로 드롭되지 않았습니다. FilePathName : {fileName} | POINT : ({aPos.x}{aPos.y}";
                Debug.LogWarning($"{log}");
                GT.MainController.Instance.AddLog(log);
                return;
            }
            else
            {
                // do something with the dropped file names. aPos will contain the 
                // mouse position within the window where the files has been dropped.
                _dropedFilePath = aFiles[0];
                string str = $"드롭된 파일 수 : { aFiles.Count }. 드롭된 POINT : { aPos } \n\t FilePathName : {aFiles[0]}";
                Debug.Log(str);
                GT.MainController.Instance.AddLog(str);
            }

            // 현재 스크린의 크기
            int screenWidth = Screen.currentResolution.width;
            bool isLeft = (aPos.x <= screenWidth / 2);

            // 각 참조하는 이벤트를 호출
            DropedFileEvent.Invoke(aFiles[0], isLeft);
        }

        void ReleaseDropFileEvent()
        {
            DropedFileEvent = null;
        }
    }
}