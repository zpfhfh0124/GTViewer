using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    // GUI 이미지 렌더러
    [SerializeField] Image _img_left;
    [SerializeField] Image _img_right;

    // 버튼
    [SerializeField] Button _btn_reset;
    [SerializeField] Button _btn_quit;

    void Start()
    {
        // 버튼 리스너 등록
        _btn_reset.onClick.AddListener(() =>
        {
            ResetImg();
        });

        _btn_quit.onClick.AddListener(() =>
        {
            QuitApp();
        });

        // 파일 드래그 앤 드롭의 이벤트를 추가
        FileDragAndDrop.DropedFileEvent += SetImg;
    }

    private void ResetImg()
    {
        Texture2D default_texture = Resources.Load("album", typeof(Texture2D)) as Texture2D;

        // 스프라이트로 변환
        Rect rect = new Rect(0, 0, default_texture.width, default_texture.height);
        Sprite default_sprite = Sprite.Create(default_texture, rect, new Vector2(0.5f, 0.5f));

        _img_left.sprite = default_sprite;
        _img_right.sprite = default_sprite;
    }

    public void SetImg(string imgFilePath, bool isLeft)
    {
        Image image = isLeft ? _img_left : _img_right;

        // 리소스 탐색 후 텍스쳐로 변환해서 저장
        byte[] byteTexture = File.ReadAllBytes(imgFilePath);
        if (byteTexture.Length > 0)
        {
            // 텍스쳐로 저장
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(byteTexture);

            // 스프라이트로 변환
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
        }
    }

    void _ResetImg()
    {
        Texture2D default_texture = Resources.Load("album", typeof(Texture2D)) as Texture2D;

        // 스프라이트로 변환
        Rect rect = new Rect(0, 0, default_texture.width, default_texture.height);
        Sprite default_sprite = Sprite.Create(default_texture, rect, new Vector2(0.5f, 0.5f));

        _img_left.sprite = default_sprite;
        _img_right.sprite = default_sprite;
    }

    void QuitApp()
    {
        Application.Quit();
    }
}
