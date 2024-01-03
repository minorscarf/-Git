using UnityEngine;

public class Setting : MonoBehaviour
{
    private int playerRating = 1500;

    public static int CalculateRank(int rating)
    {
        return Mathf.Max(1, Mathf.FloorToInt((rating / 100f) - 10));
    }

    public static int[] CalculateKeys(int rank, int keyWidth)
    {
        // プレイヤーのランク±keyWidthの範囲でキーを計算
        int[] keys = new int[2 * keyWidth + 1];
        for (int i = -keyWidth; i <= keyWidth; i++)
        {
            keys[i + keyWidth] = rank + i;
        }
        return keys;
    }
}

