using System.Collections;
using System.Collections.Generic;
using Gpm.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GT
{
    public class VideoPlayListItemData : InfiniteScrollData
    {
        public string fileName;
        public string fileFullPath;
    }

    public class VideoPlayListItem : InfiniteScrollItem
    {
        [SerializeField] Button _btn_play;

        [SerializeField] TextMeshProUGUI _text_fileName;

        VideoPlayListItemData _curItemData = new VideoPlayListItemData();

        public override void UpdateData(InfiniteScrollData scrollData)
        {
            base.UpdateData(scrollData);

            VideoPlayListItemData data = scrollData as VideoPlayListItemData;

            _curItemData = data;

            SetFileName(data.fileName);
            SetPlayButton();
        }

        void SetFileName(string fileName)
        {
            _text_fileName.text = fileName;
        }

        void OnPlay()
        {
            VideoViewer.Instance.SetVideo(_curItemData.fileFullPath);
        }

        void SetPlayButton()
        {
            _btn_play.onClick.AddListener(() =>
            {
                OnPlay();
            });
        }
    }
}