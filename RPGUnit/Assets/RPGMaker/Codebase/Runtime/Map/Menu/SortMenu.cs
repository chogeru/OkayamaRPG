using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Item;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class SortMenu : WindowBase
    {
        //右に描画するアクター
        private                  string[]   _actorID;
        [SerializeField] private GameObject _afterPartyObject;

        //左のアクター
        private string[] _beforActorItem;
        [SerializeField] private GameObject _beforePartyObject;

        //何番目に選択されているか
        private int _selected;

        private MenuBase _base;
        private RuntimeSaveDataModel _runtimeSaveDataModel;


        public override void Update() {
            base.Update();
        }

        public void Init(MenuBase @base) {
            _base = @base;
            _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            Display();
            //共通のウィンドウの適応
            Init();
        }

        //今のパーティを表示させる
        private void Display() {
            _actorID = new string[_runtimeSaveDataModel.runtimePartyDataModel.actors.Count];
            _beforActorItem = new string[_runtimeSaveDataModel.runtimePartyDataModel.actors.Count];
            _selected = 0;

            for (var i = 0; i < 4; i++)
                if (i < DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors.Count)
                {
                    for (var j = 0; j < DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.Count; j++)
                        if (DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[j].actorId ==
                            DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors[i])
                        {
                            var characterItem = _beforePartyObject.transform.Find("Actor" + (i + 1)).gameObject
                                .AddComponent<CharacterItem>();
                            characterItem.Init(DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels[j]);
                            _beforePartyObject.transform.Find("Actor" + (i + 1)).gameObject.SetActive(true);
                            _afterPartyObject.transform.Find("Actor" + (i + 1)).gameObject.SetActive(true);
                            break;
                        }
                }
                else
                {
                    _beforePartyObject.transform.Find("Actor" + (i + 1)).gameObject.SetActive(false);
                    _afterPartyObject.transform.Find("Actor" + (i + 1)).gameObject.SetActive(false);
                }
            
            var selects = _beforePartyObject.GetComponentsInChildren<Button>().ToList();
            for (var i = 0; i < selects.Count; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Count - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Count];

                selects[i].navigation = nav;
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
            }

            if (selects.Count > 0)
                selects[0].Select();
        }

        public void SortAnimation(GameObject obj) {
            ButtonRrocessing(obj);
        }

        private void SortActor(GameObject obj) {
            //押されたアクターIDの取得
            var actorId = obj.GetComponent<CharacterItem>().PartyId();
            //配列につめる
            _actorID[_selected] = actorId;
            _beforActorItem[_selected] = obj.name.Substring(obj.name.Length - 1);
            //次に込める配列用に増やしておく
            _selected++;
            //すべて選択し終わった場合
            if (_selected >= _runtimeSaveDataModel.runtimePartyDataModel.actors.Count)
            {
                //全員が選択されたらメインメニューに戻る
                ReturnMenu(_actorID);
            }
            else
            {
                _afterPartyObject.transform.Find("Actor" + _selected + "/Content").gameObject.SetActive(true);
                _afterPartyObject.transform.Find("Actor" + _selected + "/Mask").gameObject.SetActive(false);
                var characterItem = _afterPartyObject.transform.Find("Actor" + _selected + "/Content").gameObject
                    .AddComponent<CharacterItem>();
                var actors = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
                for (var i = 0; i < actors.Count; i++)
                    if (actors[i].actorId == actorId)
                    {
                        characterItem.Init(actors[i]);
                        break;
                    }

                obj.GetComponent<Button>().interactable = false;
            }
        }

        public void Cancel() {
            if (_selected <= 0)
            {
                _base.BackMenu();
            }
            else
            {
                //キャンセルのSE鳴動
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cancel);
                SoundManager.Self().PlaySe();

                _afterPartyObject.transform.Find("Actor" + _selected + "/Content").gameObject.SetActive(false);
                _afterPartyObject.transform.Find("Actor" + _selected + "/Mask").gameObject.SetActive(true);
                _selected--;
                var obj = _beforePartyObject.transform.Find("Actor" + _beforActorItem[_selected]).gameObject;
                obj.GetComponent<Button>().interactable = true;
            }
        }

        //セーブ箇所_actorIDが入ってくる
        private void Save(string[] ID) {
            for (var i = 0; i < ID.Length; i++) _runtimeSaveDataModel.runtimePartyDataModel.actors[i] = ID[i];
        }

        //再読み込み(_actorIDが入ってくる)
        private void Reload(string[] ID) {
            for (; _selected > 0; _selected--)
            {
                var obj = _beforePartyObject.transform.Find("Actor" + _selected).gameObject;
                obj.GetComponent<Button>().interactable = true;
                obj.transform.Find("Mask").gameObject.SetActive(false);
                _afterPartyObject.transform.Find("Actor" + _selected + "/Content").gameObject.SetActive(false);
                _afterPartyObject.transform.Find("Actor" + _selected + "/Mask").gameObject.SetActive(true);
            }

            Save(ID);
            Display();
        }

        private void ReturnMenu(string[] ID) {
            for (; _selected > 0; _selected--)
            {
                var obj = _beforePartyObject.transform.Find("Actor" + _selected).gameObject;
                obj.GetComponent<Button>().interactable = true;
                _afterPartyObject.transform.Find("Actor" + _selected + "/Content").gameObject.SetActive(false);
                _afterPartyObject.transform.Find("Actor" + _selected + "/Mask").gameObject.SetActive(true);
            }

            Save(ID);
            MapManager.SortActor();
            _base.BackMenu();
        }

        private void ButtonRrocessing(GameObject obj) {
            SortActor(obj);
        }
    }
}