using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository
{
    public class TileImageRepository
    {
        private static bool                     _cacheUsable; // falseの場合キャッシュを利用しない
        private static List<TileImageDataModel> _tileImageDataModels;

        /**
         * タイル用画像をインポートする
         */
        public void ImportTileImageFile() {
            AssetManageImporter.StartToFile("png", PathManager.MAP_TILE_IMAGE);
            _cacheUsable = false;
        }

        /**
         * インポート済みのタイルエンティティを削除する
         */
        public void RemoveTileImageEntity(TileImageDataModel tileImageDataModel) {
            var destPath = PathManager.MAP_TILE_IMAGE + Path.GetFileName(tileImageDataModel.filename);

            if (!File.Exists(destPath)) throw new Exception("ファイルが見つかりません " + destPath);

            UnityEditorWrapper.FileUtilWrapper.DeleteFileOrDirectory(destPath);

            _cacheUsable = false;
        }

        public void ResetTileImageEntity() {
            _tileImageDataModels = null;
        }

        public static Texture2D ReadImageFromPath(string path) {
            return ReadImage(PathManager.MAP_TILE_IMAGE + path);
        }

        //-------------------------------------------------------------------------------------
        // private methods
        //-------------------------------------------------------------------------------------
        private static Texture2D ReadImage(string path) {
            return ReadPng(path);
        }

        private static Texture2D ReadPng(string path) {
            var readBinary = ReadPngFile(path);

            var pos = 16; // 16バイトから開始

            var width = 0;
            for (var i = 0; i < 4; i++) width = width * 256 + readBinary[pos++];

            var height = 0;
            for (var i = 0; i < 4; i++) height = height * 256 + readBinary[pos++];

            var texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);

            return texture;
        }

        private static byte[] ReadPngFile(string path) {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var bin = new BinaryReader(fileStream);
            var values = bin.ReadBytes((int) bin.BaseStream.Length);

            bin.Close();

            return values;
        }
    }
}