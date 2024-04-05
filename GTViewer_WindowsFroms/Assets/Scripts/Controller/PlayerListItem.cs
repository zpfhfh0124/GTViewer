using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GT
{
    public class PlayerListItem : MonoBehaviour
    {
        [SerializeField] TextMesh _text_fileName;
        [SerializeField] Button _button_play;

        string _filePath = string.Empty;
        public string FilePath { get { return _filePath; } }

        Action<string> _playCallback;

        void Start()
        {
            _button_play.onClick.AddListener(() =>
            {
                OnPlay();
            });
        }

        public void SetFileName(string filePath)
        {
            _filePath = filePath;
            string fileName = filePath.Split('/').Last();
            _text_fileName.text = fileName;
        }

        void OnPlay()
        {
            _playCallback(_filePath);
        }

        public void SetPlayCallback(Action<string> callback)
        {
            _playCallback = callback;
        }
    }
}