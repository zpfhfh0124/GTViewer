using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GT
{
    static public class SceneList
    {
        // 씬 리스트
        public const string SCENE_IMAGE_VIEWER = "ImageDualViewerScene";
        public const string SCENE_VIDEO_PLAYER = "VideoPlayerScene";
    }

    public class SceneControler : MonoBehaviour
    {
        static public SceneControler Instance { get; private set; }

        static List<string> _dontDestroyObjectNames = new List<string>();

        private void Start()
        {
            // 인스턴스 중복 체크
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                AddDontDestroyObj(gameObject);
            }
        }

        public void SceneChange(string scene_name)
        {
            SceneManager.LoadScene(scene_name);
        }

        public void AddDontDestroyObj(GameObject obj)
        {
            // 체크 먼저...
            if (CheckDontDestroyObjs(obj))
            {
                Destroy(obj);
                return;
            }

            // DDOL 처리 하면 _dontDestroyObjectNames 리스트에 저장
            DontDestroyOnLoad(obj);
            _dontDestroyObjectNames.Add(obj.name);
            Debug.Log($"DontDestroyOnLoad로 등록 성공!. 현재 오브젝트 : {obj.name}");
        }

        // DontDestroyOnLoad로 추가가 되어있는지 체크
        // true : 이미 있음. false : 아직 없음 추가 가능
        public bool CheckDontDestroyObjs(GameObject checkObj)
        {
            Debug.Log($"중복 검사할 오브젝트 : {checkObj.name}");
            bool duplicate = false;

            foreach (var objName in _dontDestroyObjectNames)
            {
                if(objName == checkObj.name)
                {
                    duplicate = true;
                    Debug.LogWarning($"DDOL 등록하려는 오브젝트가 이미 등록되어있다. {checkObj.name}");
                    return duplicate;
                }
            }

            return duplicate;
        }
    }
}