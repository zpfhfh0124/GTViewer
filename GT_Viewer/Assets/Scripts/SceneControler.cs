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
                Debug.Log($"해당 객체는 이미 DontDestroyOnLoad에 등록이 되어있습니다. Object Name : {obj.name}");
                return;
            }

            DontDestroyOnLoad(obj);
            Debug.Log($"DontDestroyOnLoad로 등록 성공!. 현재 오브젝트 : {obj.name}");
        }

        // DontDestroyOnLoad로 추가가 되어있는지 체크
        // true : 이미 있음. false : 아직 없음 추가 가능
        public bool CheckDontDestroyObjs(GameObject checkObj)
        {
            bool duplicate = false;
            bool isEquals = false;
            int checkObjID = checkObj.GetInstanceID();
            Debug.Log($"중복 검사할 오브젝트 :[{checkObj.name}|InstanceID-{checkObjID}]");
            var allObjs_in_scene = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjs_in_scene)
            {
                isEquals = obj.Equals(checkObj);
                if (isEquals && obj.GetInstanceID() != checkObjID)
                {
                    duplicate = true;
                    Debug.Log($"이미 존재하는 오브젝트인데 새로 생성되었으므로 파괴. 파괴 오브젝트 :[{obj.name}|InstanceID-{obj.GetInstanceID()}|SceneName-{obj.scene.name}]");
                    Destroy(obj);
                    break;
                }
                else if(isEquals && obj.GetInstanceID() == checkObjID && obj.scene.name == "DontDestroyOnLoad")
                {
                    duplicate = true;
                    Debug.Log($"비교 대상 객체이다. 이미 DontDestroy 처리 되어있음. {obj.name} | {obj.scene.name}");
                    break;
                }
            }

            return duplicate;
        }
    }
}