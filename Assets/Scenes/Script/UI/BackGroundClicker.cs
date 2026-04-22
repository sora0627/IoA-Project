using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class BackGroundClicker : MonoBehaviour
    {
        private void OnMouseDown()
        {
            // SpriteScaler クラスの静的メソッドを呼び出し、拡大中のカードをすべてリセットする
            ImageScaler.ResetAllCards();
        }
    }
}
