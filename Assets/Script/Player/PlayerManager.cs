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
        private Transform parent;

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

        public void SetCard()
        {
            for (int i = 0; i < hands.Count; i++)
            {
                GameObject card = Instantiate(hands[i].gameObject, new Vector2(-3 + 3 * i, -3), Quaternion.identity);
                card.transform.parent = parent;
            }
        }

        public void UseHand(int index)
        {
            hands.RemoveAt(index);
        }
    }
}
