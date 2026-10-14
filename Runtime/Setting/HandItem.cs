using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameRecorder
{
    public class HandItem : MonoBehaviour
    {
        [SerializeField] private Image _item;
        [SerializeField] private Image _icon;

        public static Action<int> OnItemClick;

        private void Reset()
        {
            _item = GetComponent<Image>();
            _icon = transform.Find("Icon").GetComponent<Image>();
        }

        public void SetImage(Sprite sprite)
        {
            _icon.sprite = sprite;
        }

        public void Select(bool selected)
        {
            _item.color = selected ? Color.green : Color.clear;
        }

        public void OnClick()
        {
            OnItemClick?.Invoke(transform.GetSiblingIndex());
        }
    }
}