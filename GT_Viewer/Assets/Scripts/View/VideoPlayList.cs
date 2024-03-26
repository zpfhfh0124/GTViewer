using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GT
{
    public class VideoPlayList : MonoBehaviour
    {
        [SerializeField] Button _btn_close;
        [SerializeField] GameObject _prefab_playListItem;

        private void Start()
        {
            _btn_close.onClick.AddListener(() =>
            {
                SetEnable(false);
            });
        }

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public void SetVideoPlayList(List<string> fileList)
        {

        }
    }

}
