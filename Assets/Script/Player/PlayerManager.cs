using Cards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField]
        public List<CardData> hands;

        [SerializeField]
        private GameObject parent;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SelectCard()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameObject clickedObject = null;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);

                if (hit2d)
                {
                    clickedObject = hit2d.transform.gameObject;
                }
            }
        }

        void SetCard()
        {
            foreach (Transform child in parent.transform)
            {
                 
            }
        }

        public void UseHand(int index)
        {
            hands.RemoveAt(index);
        }
    }
}
