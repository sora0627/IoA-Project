using Cards;
using Move;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    public class StageManager : Singleton<StageManager>
    {

        [Header("人のプレハブ")]
        [SerializeField] private GameObject Normal;
        [SerializeField] private GameObject OldMan;
        [SerializeField] private GameObject Friend;
        [SerializeField] private GameObject Family_p;
        [SerializeField] private GameObject Family_c;

        [SerializeField]
        private Transform GenerationPos;

        [SerializeField]
        private Transform parent;

        [Header("Toilet Settings")]
        [SerializeField] private GameObject toiletPrefab; 
        [SerializeField] private Transform toiletParent;  
        [SerializeField] private int toiletCount = 5;

        [SerializeField] private float spacing = 1.5f;

        public List<GameObject> toilet = new List<GameObject>();

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
            GameObject cloneObject1 = null;

            if (cardName.Equals("Normal"))
            {
                cloneObject = Instantiate(Normal, GenerationPos.position, Quaternion.identity);
                cloneObject.transform.parent = parent;
            }

            if (cardName.Equals("Friend"))
            {
                cloneObject = Instantiate(Friend, GenerationPos.position + new Vector3(-1, 0, 0), Quaternion.identity);
                cloneObject1 = Instantiate(Friend, GenerationPos.position + new Vector3(1, 0, 0), Quaternion.identity);
                cloneObject.transform.parent = parent;
                cloneObject1.transform.parent = parent;

                SetPartner(cloneObject, cloneObject1);
            }

            if (cardName.Equals("OldMan"))
            {
                cloneObject = Instantiate(OldMan, GenerationPos.position, Quaternion.identity);
                cloneObject.transform.parent = parent;
            }

            if (cardName.Equals("Family"))
            {
                cloneObject = Instantiate(Family_c, GenerationPos.position + new Vector3(-1, 0, 0), Quaternion.identity);
                cloneObject1 = Instantiate(Family_p, GenerationPos.position + new Vector3(1, 0, 0), Quaternion.identity);
                cloneObject.transform.parent = parent;
                cloneObject1.transform.parent = parent;

                SetPartner(cloneObject, cloneObject1);
            }

            SetCheckOutTime(SelectCard, cloneObject, cloneObject1);
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
                toilet.Add(obj);
            }
        }

        void SetPartner(GameObject obj, GameObject obj1)
        {
            MouseDrag mouseDrag = obj.GetComponent<MouseDrag>();
            MouseDrag mouseDrag1 = obj1.GetComponent<MouseDrag>();

            mouseDrag.partnerCard = mouseDrag1;
            mouseDrag1.partnerCard = mouseDrag;
        }

        /// <summary>
        /// カードに記載されている退出時間をオブジェクトに書き込む
        /// </summary>
        /// <param name="cardData"></param>
        /// <param name="obj"></param>
        /// <param name="obj1"></param>
        private void SetCheckOutTime(CardData cardData , GameObject obj, GameObject obj1)
        {
            List<GameObject> objects = new List<GameObject>() { obj,  obj1 };
            int count = 0;

            foreach (GameObject gameObject in objects)
            {
                if (cardData == null || gameObject == null) continue;

                HumanData humanData = gameObject.GetComponent<HumanData>();
                if (humanData == null) continue;

                if (count == 0) humanData.outTime = cardData.outTime;
                else humanData.outTime = cardData.outTime1;

                count++;
            }
        }
    }
}
