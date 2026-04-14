using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class TurnUIController : Singleton<TurnUIController>
    {
        [Header("ターン表示用UIオブジェクト")]
        [Tooltip("[Player Turn]のUIオブジェクト")]
        [SerializeField] private GameObject playerTurnUI;

        [Tooltip("[Enemy Turn]のUIオブジェクト")]
        [SerializeField] private GameObject enemyTurnUI;

        [Header("アニメーション設定")]
        [Tooltip("回転するのにかかる時間")]
        [SerializeField] private float rotationDuration = 0.5f;

        [Tooltip("文字が見えている時間")]
        [SerializeField] private float visibleDuration = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            ResetUI(playerTurnUI);
            ResetUI(enemyTurnUI);
        }

        private void ResetUI(GameObject uiObject)
        {
            if (uiObject != null)
            {
                uiObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                uiObject.SetActive(false);
            }
        }

        public void ShowPlayerTurn()
        {
            StartCoroutine(AnimationTurnUI(playerTurnUI));
        }
        
        public void ShowEnemyTurn()
        {
            StartCoroutine(AnimationTurnUI(enemyTurnUI));
        }

        private IEnumerator AnimationTurnUI(GameObject targetUI)
        {
            if (targetUI == null) yield break;

            targetUI.SetActive(true);
            targetUI.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            yield return StartCoroutine(RotateX(targetUI.transform, 90f, 0f, rotationDuration));

            yield return new WaitForSeconds(visibleDuration);

            yield return StartCoroutine(RotateX(targetUI.transform, 0f, 90f, rotationDuration));

            targetUI.SetActive(false);
        }

        private IEnumerator RotateX(Transform targetTransform, float startAngle, float endAngle, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                targetTransform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);

                yield return null;
            }

            transform.localRotation = Quaternion.Euler(endAngle, 0f, 0f);
        }

    }
}
