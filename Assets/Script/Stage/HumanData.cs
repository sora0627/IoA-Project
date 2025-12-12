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

        public int outTime;

        private void Update()
        {
            if (GameManager.instance.IsSelect && IsReduse)
            {
                outTime -= 1;
                IsReduse = true;
            }

            if (!GameManager.instance.IsSelect)
            {
                IsReduse = false;
            }

            TimeOut(outTime);
        }

        void TimeOut(int time)
        {
            if (time > 0) return;

        }
    }
}
