using Cards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class ImageScaler : MonoBehaviour
    {
        private static ImageScaler currentMagnifiedSprite = null;

        private float magnifiedScale = 1.2f;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        private void OnMouseDown()
        {
            if (Player.PlayerManager.instance.SelectCard != gameObject)
            {
                Debug.Log("click");
                if (currentMagnifiedSprite != null && currentMagnifiedSprite != this)
                {
                    currentMagnifiedSprite.ResetScale();
                }

                if (transform.localScale.x < magnifiedScale)
                {
                    transform.localScale = originalScale * magnifiedScale;
                    currentMagnifiedSprite = this;
                }

                Player.PlayerManager.instance.SelectCard = gameObject;
            }
            else
            {
                if (!System.GameManager.instance.IsSelectCard) return;

                List<CardData> cardDatas = Player.PlayerManager.instance.hands;
                CardData cardData = gameObject.GetComponent<CardData>();
                Debug.Log(cardDatas.IndexOf(cardData));
                Player.PlayerManager.instance.UseHand(cardDatas.IndexOf(cardData));
            }
        }

        public void ResetScale()
        {
            transform.localScale = originalScale;
        }
    }
}
