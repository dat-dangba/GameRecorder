using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameRecorder
{
    [Serializable]
    public class HandModel
    {
        public List<Sprite> Sprites;
    }

    [CreateAssetMenu(fileName = "New HandSo", menuName = "ScriptableObject/New HandSo", order = 1)]
    public class HandSo : ScriptableObject
    {
        public List<HandModel> Hands;
    }
}