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

        public void Setup()
        {
            Hands = new List<HandModel>
            {
                new()
                {
                    Sprites = new List<Sprite>
                    {
                        Resources.Load<Sprite>("Images/Hand_1_1"),
                        Resources.Load<Sprite>("Images/Hand_1_2")
                    }
                },
                new()
                {
                    Sprites = new List<Sprite>
                    {
                        Resources.Load<Sprite>("Images/Hand_2_1"),
                        Resources.Load<Sprite>("Images/Hand_2_2")
                    }
                },
                new()
                {
                    Sprites = new List<Sprite>
                    {
                        Resources.Load<Sprite>("Images/Hand_3_1"),
                        Resources.Load<Sprite>("Images/Hand_3_2")
                    }
                },
                new()
                {
                    Sprites = new List<Sprite>
                    {
                        Resources.Load<Sprite>("Images/Hand_4_1"),
                        Resources.Load<Sprite>("Images/Hand_4_2")
                    }
                }
            };
        }
    }
}