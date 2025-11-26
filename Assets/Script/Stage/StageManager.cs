using Cards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    public class StageManager : Singleton<StageManager>
    {
        [SerializeField]
        private GameObject Human;

        [SerializeField]
        private Transform GenerationPos;

        [SerializeField]
        private Transform parent;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CharacterGeneration(CardData SelectCard)
        {
            string cardName = SelectCard.CardName;
            GameObject cloneObject = null;

            if (cardName.Equals("Normal"))
            {
                cloneObject = Instantiate(Human, GenerationPos.position, Quaternion.identity);
                cloneObject.transform.parent = parent;
            }

            if (cardName.Equals("Friend"))
            {

            }

            if (cardName.Equals("OldMan"))
            {

            }

            if (cardName.Equals("Family"))
            {

            }
        }
    }
}
