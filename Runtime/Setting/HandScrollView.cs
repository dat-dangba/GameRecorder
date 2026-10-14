using System.Collections.Generic;
using UnityEngine;

namespace GameRecorder
{
    public class HandScrollView : MonoBehaviour
    {
        [SerializeField] private HandItem _prefab;
        [SerializeField] private Transform _content;

        private HandSo _handSo;
        private List<HandItem> _items = new();

        private void Reset()
        {
            _prefab = Resources.Load<HandItem>("Item");
            _content = transform.Find("Viewport").Find("Content");
        }

        private void Awake()
        {
            _handSo = Resources.Load<HandSo>("HandSo");
            int selected = PlayerPrefs.GetInt("hand_selected", 0);
            for (int i = 0; i < _handSo.Hands.Count; i++)
            {
                HandItem item = Instantiate(_prefab, _content);
                item.SetImage(_handSo.Hands[i].Sprites[0]);
                item.Select(selected == i);
                _items.Add(item);
            }
        }

        private void OnEnable()
        {
            HandItem.OnItemClick += OnItemClick;
        }

        private void OnDisable()
        {
            HandItem.OnItemClick -= OnItemClick;
        }

        private void OnItemClick(int index)
        {
            PlayerPrefs.SetInt("hand_selected", index);
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Select(i == index);
            }

            Hand.OnHandSelected?.Invoke(index);
        }
    }
}