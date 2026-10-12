using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameRecorder
{
    public class Hand : MonoBehaviour
    {
        private HandSo _handSo;
        private Transform _icon;
        private List<Sprite> _sprites;
        private SpriteRenderer _handSpriteRenderer;

        private void Awake()
        {
            int selected = SessionState.GetInt("selected", 0);
            _handSo = Resources.Load<HandSo>("Editor/HandSo");
            _sprites = _handSo.Hands[selected].Sprites;
            _icon = transform.Find("Icon");
            _icon.localScale = Vector3.one * SessionState.GetFloat("size", 0.5f);
            for (int i = 0; i < _icon.childCount; i++)
            {
                Transform trans = _icon.GetChild(i);
                if (i == selected)
                {
                    trans.gameObject.SetActive(true);
                    _handSpriteRenderer = trans.GetComponent<SpriteRenderer>();
                }
                else
                {
                    trans.gameObject.SetActive(false);
                }
            }

            if (_handSpriteRenderer != null)
            {
                _handSpriteRenderer.sortingLayerName = "Hand";
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _handSpriteRenderer.sprite = _sprites[1];
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _handSpriteRenderer.sprite = _sprites[0];
            }

            MoveToMouse();
        }

        private void MoveToMouse()
        {
            Vector3 screenPos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            transform.position = worldPos;
        }
    }
}