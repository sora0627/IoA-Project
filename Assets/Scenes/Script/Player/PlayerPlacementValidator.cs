using Cards;
using Move;
using Stage;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// プレイヤーが選択したカードが現在の盤面に配置可能かどうかを判定するクラス。
    /// PlayerManagerから複雑な判定ロジックを切り離し、保守性を高めています。
    /// </summary>
    public class PlayerPlacementValidator
    {
        /// <summary>
        /// カードが配置可能かチェックします。
        /// </summary>
        public bool CanPlaceCard(CardData cardData, List<ToiletHighlight> slots)
        {
            if (slots == null || slots.Count == 0) return false;

            CardType type = cardData.Type;

            // 1枚のカードから生成されるオブジェクトが配置可能か判定
            switch (type)
            {
                case CardType.Family:
                    // 2体生成：隣接必須、親子ルールあり
                    return CheckFamilyPairAvailability(slots);

                case CardType.Friend:
                    // 2体生成：場所不問、他人隣NG
                    return CheckFriendPairAvailability(slots);

                case CardType.OldMan:
                    // 1体生成：どこでもOK
                    return slots.Exists(s => !s.IsOccupied);

                case CardType.Normal:
                default:
                    // 1体生成：他人の隣NG
                    return CheckNormalAvailability(slots);
            }
        }

        /// <summary>
        /// Friendのペア（2体）が置ける場所があるかチェック
        /// 条件：2つの空きマスを選び、それぞれが「他人の隣」でなければOK（相方の隣はOK）
        /// </summary>
        private bool CheckFriendPairAvailability(List<ToiletHighlight> slots)
        {
            // 空きスロットのインデックスをリスト化
            List<int> emptyIndices = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsOccupied) emptyIndices.Add(i);
            }

            if (emptyIndices.Count < 2) return false; // 2体置く場所がない

            // 2つの組み合わせを総当たり
            for (int i = 0; i < emptyIndices.Count; i++)
            {
                for (int j = i + 1; j < emptyIndices.Count; j++)
                {
                    int idxA = emptyIndices[i];
                    int idxB = emptyIndices[j];

                    // idxA のチェック: 両隣が「他人」であってはならない（空き or idxB ならOK）
                    bool aSafe = IsSafeForFriend(slots, idxA, idxB);
                    // idxB のチェック
                    bool bSafe = IsSafeForFriend(slots, idxB, idxA);

                    if (aSafe && bSafe) return true;
                }
            }
            return false;
        }

        private bool IsSafeForFriend(List<ToiletHighlight> slots, int targetIdx, int partnerIdx)
        {
            // 左隣チェック
            if (targetIdx > 0)
            {
                // 左が埋まっていて、かつそれが相方(partnerIdx)でなければNG（＝他人）
                if (slots[targetIdx - 1].IsOccupied) return false;
            }

            // 右隣チェック
            if (targetIdx < slots.Count - 1)
            {
                if (slots[targetIdx + 1].IsOccupied) return false;
            }

            return true;
        }

        /// <summary>
        /// Familyのペア（2体）が置ける場所があるかチェック
        /// 条件：隣接する2つの空きマスが必要
        /// [子][親] または [親][子] の並びで、子の外側が空いている必要がある
        /// </summary>
        private bool CheckFamilyPairAvailability(List<ToiletHighlight> slots)
        {
            // 隣接する2つの空きスロットを探す
            for (int i = 0; i < slots.Count - 1; i++)
            {
                if (!slots[i].IsOccupied && !slots[i + 1].IsOccupied)
                {
                    // パターン1: [子][親] (i=子, i+1=親) -> 子(i)の左隣(i-1)が空き(or端)ならOK
                    bool pattern1OK = (i == 0) || !slots[i - 1].IsOccupied;

                    // パターン2: [親][子] (i=親, i+1=子) -> 子(i+1)の右隣(i+2)が空き(or端)ならOK
                    bool pattern2OK = (i + 1 == slots.Count - 1) || !slots[i + 2].IsOccupied;

                    if (pattern1OK || pattern2OK) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Normal（1体）が置ける場所があるかチェック
        /// 条件：他人の隣NG（両隣が空いていること）
        /// </summary>
        private bool CheckNormalAvailability(List<ToiletHighlight> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;

                bool leftOk = (i == 0) || !slots[i - 1].IsOccupied;
                bool rightOk = (i == slots.Count - 1) || !slots[i + 1].IsOccupied;

                if (leftOk && rightOk) return true;
            }
            return false;
        }
    }
}