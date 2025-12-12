using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [SerializeField]
    public class CardData : MonoBehaviour
    {
        public string CardName;
        public int outTime;
        public int outTime1;
        public Move.CardType Type;
    }
}
