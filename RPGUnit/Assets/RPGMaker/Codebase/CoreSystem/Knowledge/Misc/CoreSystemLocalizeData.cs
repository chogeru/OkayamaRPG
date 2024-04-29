namespace RPGMaker.Codebase.CoreSystem.Knowledge.Misc
{
    /**
     * エディターローカライズデータ クラス。
     */
    public static class CoreSystemLocalizeData
    {
        public enum DataType
        {
            Key,
            Japanese,
            ValueTop = Japanese,
            English,
            Chinese,
            ValueBottom = Chinese,
            UsePlace1,
            UsePlace2,
            UsePlace3,
            UsePlace4,
        }

        public const int ValueCount = DataType.ValueBottom - DataType.ValueTop + 1;

        public static readonly string[,] LocaliseData = new string[,]
        {
            // キー,日本語,英語,中国語,使用箇所1,使用箇所2,使用箇所3,使用箇所4
            // import
            {"WORD_0001","画像インポート","Import Image","图片导入","","","",""},
            {"WORD_0002","閉じる","Close","关闭","","","",""},
            {"WORD_0003","データのインポートが完了しました","Data import complete.","已完成数据输入。","","","",""},
            {"WORD_0004","データが読み込めませんでした","Couldn't load data.","无法读取数据。","","","",""},
            {"WORD_0005","データのインポートをキャンセルしました","Data import cancelled.","已取消数据输入。","","","",""},
            {"WORD_0006","既にファイルが存在します、上書きしますか？","This file already exists. Overwrite and save?","有文件已存在，是否覆盖？","","","",""},
            {"WORD_0007","ファイルが見つかりませんでした、続行しますか？","File could not be found. Continue?","没有找到文件，是否继续？","","","",""},
            {"WORD_0008","prefabとefkefcデータが混在しています","File contains both prefab and efkefc data.","混杂着prefab和efkefc的数据。","","","",""},
            {"WORD_0009","はい","Yes","是","","","",""},
            {"WORD_0010","いいえ","No","否","","","",""},
            {"WORD_0011","この画像は推奨サイズに準拠していないため、期待通りに表示されない可能性があります。\n推奨サイズについては、ヘルプを参照してください。","There is a chance that the image you have chosen to import will not be displayed correctly due to not being a recommended size.\n Please see Help for more details.","此图片因没有遵照推荐尺寸，有可能不会按照预期所显示。\n 关于推荐尺寸，请参照帮助。）","","","",""},
            // export
            {"WORD_0051","エクスポート","Export","输出","","","",""},
            {"WORD_0052","閉じる","Close","关闭","","","",""},
            {"WORD_0053","データのエクスポートが完了しました","Data export completed.","已完成数据输出。","","","",""},
            {"WORD_0054","データのエクスポートをキャンセルしました","Data export cancelled.","已取消数据输出。","","","",""},
            {"WORD_0055","既にファイルが存在します、上書きしますか？","File already exists, overwrite?","文件已经存在，覆盖？","","","",""},
            {"WORD_0056","保存先を選択してください","Select save location.","请选择保存位置。","","","",""},
            {"WORD_0057","はい","Yes","是","","","",""},
            {"WORD_0058","いいえ","No","否","","","",""},

            // tile
            {"WORD_0101","タイルのインポート","Import Tiles","输入图块","","","",""},
            {"WORD_0102","閉じる","Close","关闭","","","",""},
            {"WORD_0103","画像サイズが足りない為、タイルを作成できませんでした","Tile could not be created as the image is too small.","由于图片尺寸不足，因此无法创建图块。","","","",""},
            {"WORD_0104","指定された番地が存在しませんでした、画像サイズが足りません","The specified coordinates do not exist as the image is too small.","由于指定的地址并不存在，因此画像的尺寸不足。","","","",""},
            {"WORD_0105","画像サイズが[X = %1, Y = %2]で割り切れません、続けますか？\n（画像が途切れたり、想定通りにタイルが分割されない可能性があります）","This image cannot be divided into [X = %1, Y = %2] tiles. Continue anyway?\n(Portions of the image may be cut off or not divided as expected.)","图片尺寸[X = %1, Y = %2]无法分割，是否仍然继续？\n（可能会出现图片部分无法显示，或与预想的图块无法分割的情况。）","","","",""},
            {"WORD_0106","はい","Yes","是","","","",""},
            {"WORD_0107","いいえ","No","否","","","",""},
            {"WORD_0108","タイルの数が100000を超えています。動作が遅くなる可能性がありますが続けますか？","The number of tiles exceeds 100000. It may slow down, do you want to continue?","瓷砖数量超过 100000。是否要继续，即使它可能会慢下来？","","","",""},

            // Ediror文言でCoreSystem側でも利用するもの
            {"WORD_1518","名称未設定","Undefined","无","","","",""},
            {"WORD_3104","を手に入れた！","You got.","获得了。","popup","initial import","",""},
        };
    }
}