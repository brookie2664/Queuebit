using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTable : MonoBehaviour
{
    private static Dictionary<int, int>[] lengthTable = new Dictionary<int, int>[]{
        new Dictionary<int, int>{
            {1, 0},
            {2, 2},
            {3, 6},
            {4, 12},
            {5, 20}
        }
    };

    private static Dictionary<int, int>[] powerTable = new Dictionary<int, int>[]{
        new Dictionary<int, int>{
            {1, 1},
            {2, 4},
            {3, 7},
            {4, 10},
            {5, 15}
        }
    };

    public static int getAtkLevel(int weapon, int length, int atkPower) {
        int level = 0;
        for (; level < lengthTable[weapon].Count && level <= powerTable[weapon].Count && length >= lengthTable[weapon][level + 1] && atkPower >= powerTable[weapon][level + 1]; level++) {}
        return level;
    }
}
