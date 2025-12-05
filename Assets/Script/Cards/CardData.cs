using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [SerializeField]
    public class CardData : MonoBehaviour
    {
        public string CardName;
        public bool IsRestrictedType;
        public int coolTime;

        //public Move.CardType Type;
    }
}
