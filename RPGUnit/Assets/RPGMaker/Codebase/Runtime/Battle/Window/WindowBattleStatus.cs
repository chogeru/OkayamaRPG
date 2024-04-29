using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Battle.Asset.Script;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 戦闘時のパーティメンバーのステータス表示ウィンドウ
    /// </summary>
    public class WindowBattleStatus : WindowSelectable
    {
        /// <summary>
        /// セーブデータ
        /// </summary>
        private RuntimeSaveDataModel _runtimeSave;
        [SerializeField] public GameObject _uiObject;

        /// <summary>
        /// 空アイコン
        /// </summary>
        private const string EMPTY_ICON = "IconSet_000.png";

        /// <summary>
        /// 表示するアイコンの切り替え用
        /// </summary>
        private int frameCount = 0;

        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            _runtimeSave = DataManager.Self().GetRuntimeSaveDataModel();

            //サイドビュー、フロントビューの切り替え
            ViewMode();

            //共通UIの適応を開始
            Init();

            base.Initialize();
            Refresh();
            Openness = 0;
            frameCount = 0;

            TimeHandler.Instance?.AddTimeAction(1.0f, UpdateDrawIconEverySec, true);
        }

        /// <summary>
        /// 破棄時処理
        /// </summary>
        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateDrawIconEverySec);
        }

        /// <summary>
        /// 毎秒の更新処理
        /// </summary>
        private void UpdateDrawIconEverySec() {
            //フレーム更新
            frameCount++;

            //アイコンの刷新
            for (var i = 0; i < MaxItems(); i++)
            {
                DrawItem(i);
            }
        }

        /// <summary>
        /// ウィンドウが持つ最大項目数を返す
        /// </summary>
        /// <returns></returns>
        public override int MaxItems() {
            return DataManager.Self().GetGameParty().BattleMembers().Count;
        }

        /// <summary>
        /// コンテンツの再描画
        /// </summary>
        public override void Refresh() {
            DrawAllItems();
        }

        /// <summary>
        /// 指定番号の項目を描画
        /// </summary>
        /// <param name="index"></param>
        public override void DrawItem(int index) {
            var actor = DataManager.Self().GetGameParty().BattleMembers()[index];
            var status = (ActorStatus) selectors[index];
            status.SetUp(index, actor.Name, Select, OnClickSelection);

            DrawBasicArea(index, actor);
            DrawGaugeArea(index, actor);
            DrawFace(index, actor);

            Status(index);

            //表示するべきアイコンを検索する
            List<string> showIcon = new List<string>();
            foreach (var icon in actor.AllIcons())
            {
                if (icon == EMPTY_ICON)
                {
                    continue;
                }
                showIcon.Add(icon);
            }
            //表示するべきアイコンが無ければ処理終了
            if (showIcon.Count == 0)
            {
                status.StatusIcon1.gameObject.SetActive(false);
                status.StatusIcon2.gameObject.SetActive(false);
                return;
            }

            //表示するべきアイコンがある
            //表示するアイコンの番号を決める
            //1秒間隔で表示するアイコンを切り替える
            int iconNum = frameCount % showIcon.Count;

            //Texture読込
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + showIcon[iconNum]);
            var iconTexture = iconSetTexture;
            if (iconTexture == null)
            {
                return;
            }

            //Texture読込に成功した場合は、表示処理
            status.StatusIcon1.gameObject.SetActive(true);
            status.StatusIcon2.gameObject.SetActive(false);
            status.StatusIcon1.transform.localScale = new Vector3(1f, 1f, 1f);

            var icon1 = status.StatusIcon1.GetComponent<Image>();
            if (icon1.sprite == null || icon1.sprite.texture == null || icon1.sprite.texture != iconTexture)
            {
                icon1.sprite = Sprite.Create(
                    iconTexture,
                    new Rect(0, 0, iconTexture.width, iconTexture.height),
                    Vector2.zero);
            }
        }

        /// <summary>
        /// 顔画像表示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        private void DrawFace(int index, GameActor actor) {
            GameObject obj = null;
            //アイテム表示時にイニシャライズ呼ばれないため、こっちでも設定しておく
            _runtimeSave = DataManager.Self().GetRuntimeSaveDataModel();
            ViewMode();
            
            int characterType = DataManager.Self().GetUiSettingDataModel().commonMenus[0].characterType;

            obj = _uiObject.gameObject;

            GameObject statusObj = null;

            if (obj.transform.Find("ActorStatus" + (index + 1) + "/Face")?.gameObject != null)
            {
                statusObj = obj.transform.Find("ActorStatus" + (index + 1) + "/Face").gameObject;
                var characterIcon = statusObj.GetComponent<Image>();
                characterIcon.transform.localScale = new Vector2(1f,1f);
                characterIcon.preserveAspect = false;
                if (characterType == (int) MenuIconTypeEnum.FACE)
                {
                    if (characterIcon != null)
                    {
                        characterIcon.sprite =
                            UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                "Assets/RPGMaker/Storage/Images/Faces/" + actor.FaceName + ".png");
                        characterIcon.enabled = true;
                    }
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.ActorId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    characterIcon.enabled = true;
                    characterIcon.sprite = characterGraphic.GetCurrentSprite();
                    characterIcon.color = Color.white;
                    characterIcon.material = characterGraphic.GetMaterial();
                    characterIcon.transform.localScale = characterGraphic.GetSize();

                    if (characterIcon.transform.localScale.x > 1.0f || characterIcon.transform.localScale.y > 1.0f)
                    {
                        if (characterIcon.transform.localScale.y > 1.0f)
                        {
                            characterIcon.transform.localScale = new Vector2(characterIcon.transform.localScale.x / characterIcon.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            characterIcon.transform.localScale = new Vector2(1.0f, characterIcon.transform.localScale.y / characterIcon.transform.localScale.x);
                        }
                    }

                    //characterIcon.rectTransform.pivot = new Vector2(0.5f, 1);
                    characterGraphic.gameObject.SetActive(false);
                }
                else if (characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.Actor.advImage.Contains(".png")
                        ? actor.Actor.advImage
                        : actor.Actor.advImage + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    characterIcon.enabled = true;
                    characterIcon.sprite = tex;
                    characterIcon.color = Color.white;
                    characterIcon.preserveAspect = true;
                }
                else
                {
                    characterIcon.enabled = false;
                    characterIcon.sprite = null;
                }
            }
        }

        /// <summary>
        /// 指定範囲を指定アクターで描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawBasicArea(int index, GameActor actor) {
            DrawActorName(index, actor);
            DrawActorIcons(actor);
        }

        /// <summary>
        /// 指定した[アクター]の[名前]を指定位置に描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawActorName(int index, GameActor actor) {
            ((ActorStatus) selectors[index]).label.text = actor.Name;
        }

        /// <summary>
        /// 指定範囲に指定アクターのゲージを描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawGaugeArea(int index, GameActor actor) {
            if (DataManager.Self().GetUiSettingDataModel().battleMenu.menuTp.enabled == 0)
                DrawGaugeAreaWithTp(index, actor);
            else
                DrawGaugeAreaWithoutTp(index, actor);
        }

        /// <summary>
        /// 指定範囲に指定アクターのTPを含めたゲージを描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawGaugeAreaWithTp(int index, GameActor actor) {
            DrawActorHp(index, actor);
            DrawActorMp(index, actor);
            DrawActorTp(index, actor);
        }

        /// <summary>
        /// 指定範囲に指定アクターのTPを除いたゲージを描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawGaugeAreaWithoutTp(int index, GameActor actor) {
            DrawActorHp(index, actor);
            DrawActorMp(index, actor);
            HiddenActorTp(index, actor);
        }

        /// <summary>
        /// 指定した[アクター]の[HP]を指定位置に描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawActorHp(int index, GameActor actor) {
            var status = (ActorStatus) selectors[index];
            status.hpText.text = actor.Hp.ToString();
            status.hpGauge.GetComponent<Slider>().value = 1.0f / actor.Mhp * actor.Hp;
        }

        /// <summary>
        /// 指定した[アクター]の[MP]を指定位置に描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawActorMp(int index, GameActor actor) {
            var status = (ActorStatus) selectors[index];
            status.mpText.text = actor.Mp.ToString();
            status.mpGauge.GetComponent<Slider>().value = 1.0f / actor.Mmp * actor.Mp;
        }

        /// <summary>
        /// 指定した[アクター]の[TP]を指定位置に描画
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void DrawActorTp(int index, GameActor actor) {
            var status = (ActorStatus) selectors[index];
            status.tpText.text = actor.Tp.ToString();
            status.tpGauge.GetComponent<Slider>().value = 1.0f / 100 * actor.Tp;
        }

        /// <summary>
        /// TPを非表示にする
        /// </summary>
        /// <param name="index"></param>
        /// <param name="actor"></param>
        public void HiddenActorTp(int index, GameActor actor) {
            var status = (ActorStatus) selectors[index];
            status.tpText.transform.parent.gameObject.SetActive(false);
        }

        /// <summary>
        /// 指定した[アクター]のアイコンを指定位置に描画
        /// </summary>
        /// <param name="actor"></param>
        private void DrawActorIcons(GameActor actor) {
            var icons = actor.AllIcons();
            for (var i = 0; i < icons.Count; i++) DrawIcon(actor, icons[i]);
        }

        /// <summary>
        /// 指定した番号のアイコンを指定位置に描画
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="name"></param>
        private void DrawIcon(GameActor actor, string name) {
            var status = (ActorStatus) selectors[actor.Index()];
            var path = "Assets/RPGMaker/Storage/Images/System/IconSet/" + name;
            var iconTexture = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(path);

            status.StatusIcon1.gameObject.SetActive(true);
            status.StatusIcon2.gameObject.SetActive(false);
            Image icon1 = status.StatusIcon1.GetComponent<Image>();
            status.StatusIcon1.transform.localScale = new Vector3(1f, 1f, 1f);
            status.StatusIcon2.transform.localScale = new Vector3(1f, 1f, 1f);
            if (icon1.sprite == null)
                icon1.sprite = Sprite.Create(
                    iconTexture,
                    new Rect(0, 0, iconTexture.width, iconTexture.height),
                    Vector2.zero);
        }

        /// <summary>
        /// サイドビュー、フロントビューの切り替え
        /// </summary>
        private void ViewMode() {
            _uiObject.gameObject.SetActive(true);
            StatusWords(_uiObject);
        }

        /// <summary>
        /// 各テキスト文字のローカライズ
        /// </summary>
        /// <param name="obj"></param>
        private void StatusWords(GameObject obj) {
            TextMeshProUGUI HP = null;
            TextMeshProUGUI MP = null;
            TextMeshProUGUI TP = null;

            for (var i = 1; i < 5; i++)
            {
                HP = obj.transform.Find("ActorStatus" + i + "/HPBox/HPText").GetComponent<TextMeshProUGUI>();
                MP = obj.transform.Find("ActorStatus" + i + "/MPBox/MPText").GetComponent<TextMeshProUGUI>();
                TP = obj.transform.Find("ActorStatus" + i + "/TPBox/TPText").GetComponent<TextMeshProUGUI>();

                HP.text = TextManager.hp;
                MP.text = TextManager.mp;
                TP.text = TextManager.tp;
            }
        }

        /// <summary>
        /// アクターの名前設定
        /// </summary>
        /// <param name="index"></param>
        private void Status(int index) {
            if (_runtimeSave == null || _runtimeSave.runtimePartyDataModel == null || _runtimeSave.runtimePartyDataModel.actors == null) 
                return;
            if (_runtimeSave.runtimePartyDataModel.actors.Count <= index)
                return;
            for (var i = 0; i < _runtimeSave?.runtimeActorDataModels.Count; i++)
                if (_runtimeSave.runtimeActorDataModels[i].actorId == _runtimeSave.runtimePartyDataModel.actors[index])
                    ((ActorStatus) selectors[index]).label.text = _runtimeSave.runtimeActorDataModels[i].name;
        }
    }
}