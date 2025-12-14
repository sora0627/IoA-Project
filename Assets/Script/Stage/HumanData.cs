using Move;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Systems;
using UnityEngine;

namespace Stage
{
    public class HumanData : MonoBehaviour
    {
        private bool IsReduse = false;

        public int checkoutTime;

        private void Update()
        {
            if (GameManager.instance.IsSelect && !IsReduse)
            {
                checkoutTime -= 1;
                IsReduse = true;
            }

            if (!GameManager.instance.IsSelect)
            {
                IsReduse = false;
            }

            TimeOut(checkoutTime);
        }

        /// <summary>
        /// カウントが０になったときの処理
        /// トイレが出ていく
        /// </summary>
        /// <param name="time"></param>
        void TimeOut(int time)
        {
            if (time > 0) return;
            MouseDrag mouseDrag = this.GetComponent<MouseDrag>();
            ToiletHighlight toilet = mouseDrag.CurrentSlot;
            toilet.Vacate();
            gameObject.SetActive(false);
        }
    }
}
