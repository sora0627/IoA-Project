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

        [Header("Toilet Settings")]
        [SerializeField] private GameObject toiletPrefab; 
        [SerializeField] private Transform toiletParent;  
        [SerializeField] private int toiletCount = 5;

        [SerializeField] private float spacing = 1.5f;

        // Start is called before the first frame update
        void Start()
        {
            GenerateToilets();
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
        void GenerateToilets()
        {
            float totalWidth = (toiletCount - 1) * spacing;

            float startX = -totalWidth / 2;

            for (int i = 0; i < toiletCount; i++)
            {
                GameObject obj = Instantiate(toiletPrefab, toiletParent);
                float x = startX + (i * spacing);

                obj.transform.localPosition = new Vector3(x, 0, 0);
            }
        }
    }
}
