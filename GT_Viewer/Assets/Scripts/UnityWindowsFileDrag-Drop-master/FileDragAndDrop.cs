using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using UnityEngine.UI;

public class FileDragAndDrop : MonoBehaviour
{
    // GUI 이미지 렌더러
    [SerializeField] Image _img_left;
    [SerializeField] Image _img_right;

    List<string> log = new List<string>();
    void OnEnable ()
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
        if(aFiles.Count <= 0 || aFiles == null || aPos.x < 0 || aPos.y < 0)
        {
            string fileName = (aFiles != null && aFiles.Count > 0) ? aFiles[0] : "no file";
            Debug.LogWarning($"파일이 제대로 드롭되지 않았습니다. FilePathName : {fileName} | POINT : ({aPos.x}{aPos.y}) ");
            return;
        }
        else
        {
            // do something with the dropped file names. aPos will contain the 
            // mouse position within the window where the files has been dropped.
            string str = $"드롭된 파일 수 : { aFiles.Count }. 드롭된 POINT : { aPos } \n\t FilePathName : {aFiles[0]}";
            Debug.Log(str);
            log.Add(str);
        }

        // 현재 스크린의 크기
        int screenWidth = Screen.currentResolution.width;
        bool isLeft = (aPos.x <= screenWidth / 2);

        // 파일이 이미지 파일인지 검사(jpeg, jpg, png, gif...)
        bool isImageFile = false;
        string extension = aFiles[0].Split('.').Last();
        List<string> list_img_extension = new List<string>{ "jpg", "jpeg", "png", "gif" };
        if(list_img_extension.Exists(x => x == extension))
        {
            SetImg(aFiles[0]);
            Debug.Log($"드롭된 파일의 확장자가 {extension} 이미지 드롭 성공!");
        }
        else
        {
            Debug.LogWarning($"드롭된 파일의 확장자가 {extension}으로 이미지 파일이 아닙니다.");
            return;
        }
        
    }

    void SetImg(string imgFilePath)
    {

    }

    void ResetImg()
    {

    }

    private void OnGUI()
    {
        if (GUILayout.Button("clear log"))
            log.Clear();
        foreach (var s in log)
            GUILayout.Label(s);
    }
}
