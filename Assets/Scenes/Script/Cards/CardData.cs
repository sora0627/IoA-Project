using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [SerializeField]
    public class CardData : MonoBehaviour
    {
        public string CardName;
        public int checkoutTime;
        public int checkoutTime1;
        public Move.CardType Type;
    }
}
