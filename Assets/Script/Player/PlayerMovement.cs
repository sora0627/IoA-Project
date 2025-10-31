using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // インスペクタで設定する「最大」の移動速度
    public float maxMoveSpeed = 10f;

    // 動きが止まる中央の範囲（ピクセル単位）
    public float deadZoneWidth = 20f;

    private float screenCenterX; // 画面中央のX座標

    // 速度を計算するための最大有効距離
    // (画面中央から画面端までの距離 - デッドゾーンの幅)
    private float maxEffectiveDistance;

    void Start()
    {
        screenCenterX = Screen.width / 2f;

        // 速度計算に使う「最大有効距離」をあらかじめ計算しておきます
        maxEffectiveDistance = screenCenterX - deadZoneWidth;

        // デッドゾーンが広すぎて動ける範囲がない場合のためのチェック
        if (maxEffectiveDistance <= 0)
        {
            Debug.LogWarning("Dead Zone Width is too large! Object may not move.");
        }
    }

    void Update()
    {
        // 1. 現在のマウスカーソルのX座標を取得
        float mouseX = Input.mousePosition.x;

        // 2. マウスが中央からどれだけ離れているか計算
        float distanceFromCenter = mouseX - screenCenterX;

        // 3. デッドゾーンの判定
        // マウスが中央（deadZoneWidthで指定した範囲内）にあれば、何もしない（停止）
        if (Mathf.Abs(distanceFromCenter) <= deadZoneWidth)
        {
            return; // Updateメソッドをここで終了
        }

        // 4. 速度の割合(0.0 ～ 1.0)を計算

        // デッドゾーンの外側の、純粋な距離
        // 例: 中央から100px離れていて、デッドゾーンが20pxなら、有効距離は 80px
        float effectiveDistance = Mathf.Abs(distanceFromCenter) - deadZoneWidth;

        // 割合を計算 (有効距離 / 最大有効距離)
        float speedRatio = 0f;
        if (maxEffectiveDistance > 0)
        {
            speedRatio = effectiveDistance / maxEffectiveDistance;
        }

        // 割合が1.0を超えないように制限します (0.0～1.0の間に収める)
        // これにより、速度が maxMoveSpeed を超えることがなくなります
        speedRatio = Mathf.Clamp01(speedRatio);

        // 5. 最終的な速度を計算
        // (最大速度 × 速度の割合)
        float currentSpeed = maxMoveSpeed * speedRatio;

        // 6. 移動方向を決定
        // distanceFromCenter がプラスなら 1 (右)、マイナスなら -1 (左)
        float direction = Mathf.Sign(distanceFromCenter);

        // 7. オブジェクトを移動
        // (方向 × 現在の速度 × Time.deltaTime)
        transform.Translate(direction * currentSpeed * Time.deltaTime, 0, 0);
    }
}