using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System;

public class FileDragAndDrop : MonoBehaviour
{
    // 드롭된 시점의 파일 경로를 저장할 변수
    static string _dropedFileFullPath = string.Empty;
    // 드롭된 시점의 위치
    static bool _isLeft = false;

    // 외부에서 드롭 시점에 실행시킬 이벤트 접근자
    public delegate void DropedEventHandler(string filePath, bool isLeft);
    static public event DropedEventHandler DropedFileEvent;

    GUIStyle _log_guiStyle;
    List<string> _log = new List<string>();
    void OnEnable ()
    {
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;

        _log_guiStyle = new GUIStyle("label");
        _log_guiStyle.fontSize = 15;
        _log_guiStyle.fontStyle = FontStyle.Bold;
        _log_guiStyle.normal.textColor = Color.magenta;
        _log_guiStyle.normal.background = new Texture2D(Screen.width / 2, Screen.height / 2);
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        // 드롭된 파일이 없거나, 위치가 잘못된 경우 예외처리
        if(aFiles.Count <= 0 || aFiles == null || aPos.x < 0 || aPos.y < 0)
        {
            string fileName = (aFiles != null && aFiles.Count > 0) ? aFiles[0] : "no file";
            string log = $"파일이 제대로 드롭되지 않았습니다. FilePathName : {fileName} | POINT : ({aPos.x}{aPos.y}";
            Debug.LogWarning($"{log}");
            _log.Add(log);
            return;
        }
        else
        {
            // do something with the dropped file names. aPos will contain the 
            // mouse position within the window where the files has been dropped.
            string str = $"드롭된 파일 수 : { aFiles.Count }. 드롭된 POINT : { aPos } \n\t FilePathName : {aFiles[0]}";
            Debug.Log(str);
            _log.Add(str);
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

    private void OnGUI()
    {
        if (GUILayout.Button("clear log"))
            _log.Clear();

        //GUILayout.BeginArea(new Rect(50, 50, 300, 300));
        //GUILayout.Box("Log Area");
            foreach (var s in _log)
            {
                GUILayout.Label( s, _log_guiStyle );
            }
        //GUILayout.EndArea();
    }
}
