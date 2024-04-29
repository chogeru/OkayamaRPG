using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement.ValueObject
{
    public class AutoTileRuleB
    {
        public static readonly Dictionary<int, List<int>> SlicedTextureIndexesByTileType =
            new Dictionary<int, List<int>>
            {
                {0, new List<int> {10, 9, 6, 5}},
                {1, new List<int> {8, 9, 4, 5}},
                {2, new List<int> {2, 1, 6, 5}},
                {3, new List<int> {0, 1, 4, 5}},
                {4, new List<int> {10, 11, 6, 7}},
                {5, new List<int> {8, 11, 4, 7}},
                {6, new List<int> {2, 3, 6, 7}},
                {7, new List<int> {0, 3, 4, 7}},
                {8, new List<int> {10, 9, 14, 13}},
                {9, new List<int> {8, 9, 12, 13}},
                {10, new List<int> {2, 1, 14, 13}},
                {11, new List<int> {0, 1, 12, 13}},
                {12, new List<int> {10, 11, 14, 15}},
                {13, new List<int> {8, 11, 12, 15}},
                {14, new List<int> {2, 3, 14, 15}},
                {15, new List<int> {0, 3, 12, 15}}
            };


        public static readonly Dictionary<List<int>, int> TileShapeBySurroundings = new Dictionary<List<int>, int>
        {
            {new List<int> {1, 1, 1, 1}, 0}, // 下・右・上・左が同じタイルの場合
            {new List<int> {1, 1, 1, 0}, 1},
            {new List<int> {1, 1, 0, 1}, 2},
            {new List<int> {1, 1, 0, 0}, 3},
            {new List<int> {1, 0, 1, 1}, 4},
            {new List<int> {1, 0, 1, 0}, 5},
            {new List<int> {1, 0, 0, 1}, 6},
            {new List<int> {1, 0, 0, 0}, 7},
            {new List<int> {0, 1, 1, 1}, 8},
            {new List<int> {0, 1, 1, 0}, 9},
            {new List<int> {0, 1, 0, 1}, 10},
            {new List<int> {0, 1, 0, 0}, 11},
            {new List<int> {0, 0, 1, 1}, 12},
            {new List<int> {0, 0, 1, 0}, 13},
            {new List<int> {0, 0, 0, 1}, 14},
            {new List<int> {0, 0, 0, 0}, 15}
        };

        public static List<int> GetSlicedTextureIndexesOfThumbnail() {
            return new List<int> {0, 0, 0, 0};
        }

        public static List<int> GetSlicedTextureIndexesByShapeType(int shapeType) {
            return SlicedTextureIndexesByTileType[shapeType];
        }
    }
}