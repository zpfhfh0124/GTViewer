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
        _log_guiStyle.fontSize = 20;
        _log_guiStyle.fontStyle = FontStyle.Italic;
        _log_guiStyle.normal.textColor = Color.cyan;
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

        // 파일이 이미지 파일인지 검사(jpeg, jpg, png, gif...)
        string filePath = aFiles[0];
        string extension = filePath.Split('.').Last();
        extension = extension.ToLower();
        List<string> list_img_extension = new List<string>{ "jpg", "jpeg", "png", "gif" };
        if(list_img_extension.Exists(x => x == extension))
        {
            Debug.Log($"드롭된 파일의 확장자가 {extension} 이미지 드롭 성공!");
        }
        else
        {
            Debug.LogWarning($"드롭된 파일의 확장자가 {extension}으로 이미지 파일이 아닙니다.");
            return;
        }

        // 각 참조하는 이벤트를 호출
        DropedFileEvent.Invoke(filePath, isLeft);
    }

    void ReleaseDropFileEvent()
    {
        DropedFileEvent = null;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("clear log"))
            _log.Clear();

        foreach (var s in _log)

            GUILayout.Label( s );
    }
}
