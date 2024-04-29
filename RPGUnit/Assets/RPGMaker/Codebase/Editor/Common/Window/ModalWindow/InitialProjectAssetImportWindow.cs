using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.Window.ModalWindow
{
    public class InitialProjectAssetImportWindow : BaseModalWindow
    {
        // テキスト
        private const string TITLE_TEXT = "WORD_2100";
        //private const string BASIC_ASSETS = "WORD_2105";
        //private const string FULL_GAME = "WORD_2106";
        private const string DESCRIPTION_TEXT = "WORD_5005"; //"作成するプロジェクトを選択してください";
        private const string DESCRIPTION_TEXT2 = "WORD_5010"; //"指定言語のプロジェクトテンプレートがインストール\nされていません";
        private const string PROJECT_NAME_TEXT = "WORD_5008"; //"プロジェクト名";
        private const string PATH_TEXT = "WORD_5009"; //"保存場所";
        private static readonly string[] LANGUAGE_NAMES = new[] {
            "WORD_2102",
            "WORD_2103",
            "WORD_2104"
        };

        private const string DEFAULT_PROJECT_NAME = "New Unite Project";
        private const string VERSION = "1.0.8";
        private const string PROJECT_BASE = "project_base_v" + VERSION + ".zip";
        //private const string MASTERDATA_COMMON = "masterdata_common_v";
        private const string MASTERDATA_JP = "masterdata_jp_v" + VERSION + ".zip";
        private const string MASTERDATA_EN = "masterdata_en_v" + VERSION + ".zip";
        private const string MASTERDATA_CN = "masterdata_ch_v" + VERSION + ".zip";
        //private const string DEFAULTGAME_COMMON = "defaultgame_common_v";
        private const string DEFAULTGAME_JP = "defaultgame_jp_v" + VERSION + ".zip";
        private const string DEFAULTGAME_EN = "defaultgame_en_v" + VERSION + ".zip";
        private const string DEFAULTGAME_CN = "defaultgame_ch_v" + VERSION + ".zip";

        private VisualElement _labelFromUxml;
        private ImTextField _path;
        private ImTextField _projectName;
        private Label _projectNameLabel;
        private Label _pathLabel;
        private Button _okButton;
        private Button _pathButton;

        private int _selectNum;
        private bool _isLanguageData;

        protected override string ModalUxml => "Assets/RPGMaker/Codebase/Editor/Common/Window/ModalWindow/Uxml/initialize_project_asset_import.uxml";

        public void ShowWindow() {
            var wnd = GetWindow<InitialProjectAssetImportWindow>();
            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(TITLE_TEXT));
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(600, 480);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {

            // 各zipファイル
#if UNITY_EDITOR_WIN
            // 2階層上
            var folderSub = "/../../";
#else
            // 4階層上
            var folderSub = "/../../../../";
#endif
            var _folderPath = Path.Combine(Application.persistentDataPath + folderSub, ".RPGMaker/");

            // Viewの作成
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            _labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(_labelFromUxml);
            root.Add(_labelFromUxml);

            RadioButton radioBasicButton = root.Q<RadioButton>("radio_basic_asset");
            RadioButton radioFullButton = root.Q<RadioButton>("radio_full_asset");
            radioFullButton.value = false;

            _projectNameLabel = _labelFromUxml.Query<Label>("project_name_label");
            _projectName = _labelFromUxml.Query<ImTextField>("project_name");
            _pathLabel = _labelFromUxml.Query<Label>("path_label");
            _path = _labelFromUxml.Query<ImTextField>("path");
            _pathButton = _labelFromUxml.Query<Button>("path_button");

            // プロジェクト名とパス名の設定

            // テキスト設定
            _projectNameLabel.text = EditorLocalize.LocalizeText(PROJECT_NAME_TEXT);
            _pathLabel.text = EditorLocalize.LocalizeText(PATH_TEXT);
            _projectName.value = DEFAULT_PROJECT_NAME;
            _path.value = Application.dataPath.Replace("/Assets", "");
            _path.isReadOnly = true;

            VisualElement langSelect = _labelFromUxml.Query<VisualElement>("langSelect");
            var languages = LANGUAGE_NAMES.Select(lang => EditorLocalize.LocalizeText(lang)).ToList();

            {
                // 各zipファイルが存在しているかどうかを確認し、無ければ説明欄の文言を切り替え、OKボタンを押下不可とする
                // 現在の言語設定
                var _lang2 = (SystemLanguage) typeof(EditorWindow).Assembly.GetType("UnityEditor.LocalizationDatabase").GetProperty("currentEditorLanguage").GetValue(null);
                switch (_lang2)
                {
                    case SystemLanguage.Japanese:
                        _selectNum = 0;
                        break;
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                    case SystemLanguage.ChineseTraditional:
                        _selectNum = 2;
                        break;
                    default:
                        _selectNum = 1;
                        break;
                }
            }

            var langSelectPopupField = new PopupFieldBase<string>(languages, _selectNum);
            langSelect.Clear();
            langSelect.Add(langSelectPopupField);
            langSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                _selectNum = langSelectPopupField.index;
                CheckLanguageData(_folderPath);
            });

            // ボタン設定
            {
                string iconPath = EditorGUIUtility.isProSkin ? "Dark" : "Light";
                _pathButton.style.backgroundImage = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>("Assets/RPGMaker/Storage/Images/System/Menu/" + iconPath + "/Active/uibl_icon_menu_002.png");
                _pathButton.style.width = 25;
                _pathButton.clicked += () =>
                {
                    var result = EditorUtility.OpenFolderPanel("Open Folder", _path.value, "");
                    if(!string.IsNullOrEmpty(result))
                    {
                        _path.value = result;
                    }
                };
            }

            //OKボタン
            _okButton = _labelFromUxml.Query<Button>("OK_button");
            _okButton.clicked += () =>
            {
                var ProjectPath = Path.Combine(_path.value, _projectName.value);
                if (!Directory.Exists(ProjectPath))
                {
                    Directory.CreateDirectory(ProjectPath);
                }
                ZipFile.ExtractToDirectory(_folderPath + PROJECT_BASE, ProjectPath, true);
                // 新規作成時は、ここでPackages/jp.ggg.rpgmaker.unite/下は、version.txt以外、削除する
                // もともとTryCatchだったのをExistにしてますが理由は不明…
                if (Directory.Exists(ProjectPath + "/Packages/jp.ggg.rpgmaker.unite/Editor"))
                {
                    Directory.Delete(ProjectPath + "/Packages/jp.ggg.rpgmaker.unite/Editor", true);
                }
                var files = Directory.GetFiles(ProjectPath + "/Packages/jp.ggg.rpgmaker.unite");
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                //バージョン追加
                var localVersionPath = ProjectPath + "/Packages/jp.ggg.rpgmaker.unite/version.txt";
                {
                    using var fs = new FileStream(localVersionPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    using var writer = new StreamWriter(fs, Encoding.UTF8);
                    writer.Write(VERSION);
                }

                // Storageの展開処理
                var StoragePath = Path.Combine(ProjectPath, "Assets/RPGMaker/Storage");
                if (!Directory.Exists(StoragePath))
                {
                    Directory.CreateDirectory(StoragePath);
                }
                // masterdata_jpは必須になります
                var installList = new List<string>
                {
                    MASTERDATA_JP
                };
                if (radioFullButton.value) {
                    installList.Add(DEFAULTGAME_JP);
                }
                if (_selectNum == 2)
                {
                    installList.Add(radioFullButton.value ? DEFAULTGAME_CN : MASTERDATA_CN);
                }
                else if (_selectNum != 0)
                {
                    installList.Add(radioFullButton.value ? DEFAULTGAME_EN : MASTERDATA_EN);
                }

                foreach (var install in installList)
                {
                    ZipFile.ExtractToDirectory(_folderPath + install, StoragePath, true);
                }


                //保存
                EditorApplication.ExecuteMenuItem("File/Save");

                //このWindowを閉じる
                Close();

                //作成したPJを開く
                EditorApplication.OpenProject(ProjectPath);
            };

            //CANCELボタン
            Button cancelButton = _labelFromUxml.Query<Button>("CANCEL_button");
            cancelButton.clicked += () => { Close(); };

            //初期状態
            CheckLanguageData(_folderPath);

            _path.RegisterValueChangedCallback(evt =>
            {
                UpdateOKButtonEnable();
            });
            _projectName.RegisterValueChangedCallback(evt =>
            {
                UpdateOKButtonEnable();
            });
        }

        private void CheckLanguageData(string rootPath) {
            Label descriptionLabel = _labelFromUxml.Query<Label>("description_text");

            bool exist = true;
            //if (File.Exists(_folderPath + GetFileName(MASTERDATA_COMMON)) == false)
            //    exist = false;
            //if (File.Exists(_folderPath + GetFileName(DEFAULTGAME_COMMON)) == false)
            //    exist = false;

            if (_selectNum == 0)
            {
                if (File.Exists(rootPath + MASTERDATA_JP) == false)
                    exist = false;
                if (File.Exists(rootPath + DEFAULTGAME_JP) == false)
                    exist = false;
            }
            else if (_selectNum == 2)
            {
                if (File.Exists(rootPath + MASTERDATA_CN) == false)
                    exist = false;
                if (File.Exists(rootPath + DEFAULTGAME_CN) == false)
                    exist = false;
            }
            else
            {
                if (File.Exists(rootPath + MASTERDATA_EN) == false)
                    exist = false;
                if (File.Exists(rootPath + DEFAULTGAME_EN) == false)
                    exist = false;
            }

            if (!exist)
            {
                descriptionLabel.text = EditorLocalize.LocalizeText(DESCRIPTION_TEXT2);
                _okButton.SetEnabled(false);
                _isLanguageData = false;
            }
            else
            {
                descriptionLabel.text = EditorLocalize.LocalizeText(DESCRIPTION_TEXT);
                _okButton.SetEnabled(true);
                _isLanguageData = true;
            }

            UpdateOKButtonEnable();
        }

        /// <summary>
        /// OKボタンの有効/無効切替
        /// </summary>
        private void UpdateOKButtonEnable() {
            if (!_isLanguageData)
            {
                _okButton.SetEnabled(false);
                return;
            }

            // プロジェクト名の重複チェック
            if (Directory.Exists(_path.value + "/" + _projectName.value))
            {
                _okButton.SetEnabled(false);
            }
            else
            {
                _okButton.SetEnabled(true);
            }
        }

    }
}