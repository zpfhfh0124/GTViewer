using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gpm.Ui;

namespace GT
{
    public class VideoPlayList : MonoBehaviour
    {
        [SerializeField] Button _btn_loadFiles;
        [SerializeField] Button _btn_close;

        [SerializeField] InfiniteScroll _infiniteScroll;

        static List<string> _playList = new List<string>();

        private void Start()
        {
            _btn_close.onClick.AddListener(() =>
            {
                SetEnable(false);
            });

            _btn_loadFiles.onClick.AddListener(() =>
            {
                ShowPlayList();
            });
        }

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public void SetVideoPlayList(List<string> fileList)
        {
            _playList = fileList;
        }

        public List<string> GetVideoPlayList()
        {
            return _playList;
        }

        void ShowPlayList()
        {
            foreach (var item in _playList)
            {
                Debug.Log($"SetPlayList 현재 경로 - 파일 {item}");
            }

            SetVideoPlayListData();
        }

        public void SetVideoPlayListData()
        {
            _infiniteScroll.ClearData();

            foreach (var item in _playList)
            {
                VideoPlayListItemData data = new VideoPlayListItemData();
                data.fileFullPath = item;
                data.fileName = MainController.Instance.GetFileName(item);

                _infiniteScroll.InsertData(data);
            }
        }
    }

}

