using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud
{
    public class Picture : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/Picture.prefab";

        private DatabaseManagementService _databaseManagementService;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Dictionary<int, List<string>> _pictureName;
        private Dictionary<int, PictureData> _pictureList;

        private Dictionary<int, MoveData> _moveDictionary = new Dictionary<int, MoveData>();
        private Dictionary<int, ChangeColorData> _changeColorDictionary = new Dictionary<int, ChangeColorData>();
        private Dictionary<int, ChangeSizeData> _changeSizeDictionary = new Dictionary<int, ChangeSizeData>();
        private Dictionary<int, RotationData> _rotationDictionary = new Dictionary<int, RotationData>();

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject  _prefab;

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------


        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init() {
            _pictureName = new Dictionary<int, List<string>>();
            _pictureList = new Dictionary<int, PictureData>();
            _moveDictionary ??= new Dictionary<int, MoveData>();
            _changeColorDictionary ??= new Dictionary<int, ChangeColorData>();
            _changeSizeDictionary ??= new Dictionary<int, ChangeSizeData>();
            _rotationDictionary ??= new Dictionary<int, RotationData>();
            if (_prefab == null)
            {
                var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
                _prefab = Instantiate(
                    loadPrefab,
                    gameObject.transform,
                    true
                );
                UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            }

            _databaseManagementService = new DatabaseManagementService();
        }

        public void AddPictureParameter(int pictureNumber, List<string> parameters) {
            if (!_pictureName.ContainsKey(pictureNumber))
                _pictureName.Add(pictureNumber, parameters);
        }

        public void AddPicture(int pictureNumber, string pictureName) {
            if (_pictureName.ContainsKey(pictureNumber))
            {
                //同一の画像を表示中である場合には、移動等のイベントのみを破棄する
                if (_pictureName[pictureNumber][1] == pictureName)
                {
                    DeletePicture(pictureNumber, true);
                    return;
                }
                //別の画像を表示中である場合には、画像を破棄する
                else
                {
                    DeletePicture(pictureNumber);
                }
            }

            var texture2D =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/Pictures/" + pictureName + ".png");
            var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
                Vector2.zero);
            var obj = new GameObject();

            //表示するCanvasにレイヤー順を追加する
            var canvas = _prefab.transform.Find("Canvas").transform.gameObject;
            canvas.transform.SetParent(_prefab.transform.Find("Canvas").transform.parent, false);
            canvas.transform.localScale = Vector3.one;
            var rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1920, 1080);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            canvas.transform.localPosition = Vector3.zero;

            obj.transform.SetParent(canvas.transform);
            //子内で、ソートする
            obj.name = "PictureNumber_" + pictureNumber;
            obj.transform.SetSiblingIndex(pictureNumber - 1);
            obj.AddComponent<Image>();
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(texture2D.width, texture2D.height);
            obj.transform.localScale = new Vector3(1, 1, 1);
            var pictureData = new PictureData();
            var image = obj.GetComponent<Image>();
            image.sprite = sprite;
            pictureData.image = image;
            pictureData.size = new Vector2(texture2D.width, texture2D.height);
            _pictureList.Add(pictureNumber, pictureData);

            //子のオブジェクト名と、ピクチャ番号を紐づけて、Canvas下の子全てをソートしなおす
            var pictures = _pictureList.Keys.ToList();
            var numbers = new List<int>();
            for (int i = 0; i < pictures.Count; i++)
            {
                numbers.Add(pictures[i]);
            }
            numbers.Sort();
            for (int i = 0; i < numbers.Count; i++)
            {
                _pictureList[numbers[i]].image.transform.SetSiblingIndex(numbers[i] - 1);
            }
        }

        public Image GetPicture(int pictureNumber) {
            if (_pictureList == null || _pictureList.Count < 0 || !_pictureList.ContainsKey(pictureNumber))
                return null;

            return _pictureList[pictureNumber].image;
        }

        public void SetPivot(int pictureNumber, int pivot) {
            var changePivot = new Vector2(0, 0);
            if (pivot == 0)
                changePivot = new Vector2(0, 1);
            else
                changePivot = new Vector2(0.5f, 0.5f);

            var size = _pictureList[pictureNumber].image.GetComponent<RectTransform>().rect.size;
            var deltaPivot = _pictureList[pictureNumber].image.GetComponent<RectTransform>().pivot - changePivot;
            var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            _pictureList[pictureNumber].image.GetComponent<RectTransform>().pivot = changePivot;
            _pictureList[pictureNumber].image.transform.localPosition -= deltaPosition;
        }

        public void SetAnchor(int pictureNumber, int anchor) {
            var changePivot = new Vector2(0, 0);
            var changeAnchor = new Vector2(0, 1);
            if (anchor == 1)
                changePivot = new Vector2(0.5f, 0.5f);
            else
                changePivot = new Vector2(0, 1);

            _pictureList[pictureNumber].image.GetComponent<RectTransform>().pivot = changePivot;
            _pictureList[pictureNumber].image.GetComponent<RectTransform>().anchorMax = changeAnchor;
            _pictureList[pictureNumber].image.GetComponent<RectTransform>().anchorMin = changeAnchor;
        }

        public void SetPosition(int pictureNumber, int type, string x, string y) {
            float xValue = 0;
            float yValue = 0;

            //定数指定時
            if (type == 0)
            {
                xValue = float.Parse(x);
                yValue = float.Parse(y);
            }
            //変数指定時
            else
            {
                var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

                var flagDataModel = _databaseManagementService.LoadFlags();
                for (var i = 0; i < flagDataModel.variables.Count; i++)
                    if (flagDataModel.variables[i].id == x)
                    {
                        xValue = float.Parse(runtimeSaveDataModel.variables.data[i]);
                        break;
                    }

                for (var i = 0; i < flagDataModel.variables.Count; i++)
                    if (flagDataModel.variables[i].id == y)
                    {
                        yValue = float.Parse(runtimeSaveDataModel.variables.data[i]);
                        break;
                    }
            }

            _pictureList[pictureNumber].image.rectTransform.anchoredPosition3D = new Vector3(xValue, -yValue, -9);
        }

        public void SetPictureSize(int pictureNumber, int widthDiameter, int heightDiameter) {
            _pictureList[pictureNumber].image.GetComponent<RectTransform>().sizeDelta =
                _pictureList[pictureNumber].size * new Vector2(widthDiameter * 0.01f, heightDiameter * 0.01f);
        }

        public void SetPictureOpacity(int pictureNumber, int opacity) {
            var color = _pictureList[pictureNumber].image.GetComponent<Image>().color;
            _pictureList[pictureNumber].image.color = new Color(color.r, color.g, color.b, opacity / 255f);
        }

        public void SetProcessingType(int pictureNumber, int processingType) {
            var image = _pictureList[pictureNumber].image.transform.GetComponent<Image>();
            var material = new Material(image.material.shader);
            image.material = material;

            switch (processingType)
            {
                case 0:
                    image.material.shader = Shader.Find("UI/Default");
                    break;
                case 1:
                    image.material.shader = Shader.Find("UI/DefaultAdd");
                    break;
                case 2:
                    image.material.shader = Shader.Find("UI/DefaultMultiply");
                    break;
                case 3:
                    image.material.shader = Shader.Find("UI/DefaultScreen");
                    break;
            }
        }

        public void StartMove(
            Action action,
            int pictureNumber,
            int moveType,
            int type,
            string x,
            string y,
            int frame,
            bool toggle
        ) {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

            var flagDataModel = _databaseManagementService.LoadFlags();

            if (type == 1)
            {
                for (var i = 0; i < flagDataModel.variables.Count; i++)
                    if (flagDataModel.variables[i].id == x)
                    {
                        x = runtimeSaveDataModel.variables.data[i];
                        break;
                    }

                for (var i = 0; i < flagDataModel.variables.Count; i++)
                    if (flagDataModel.variables[i].id == y)
                    {
                        y = runtimeSaveDataModel.variables.data[i];
                        break;
                    }
            }

            var pos = new Vector2Int(int.Parse(x), int.Parse(y));
            if (type != 0)
            {
                if (x.All(char.IsDigit))
                    pos.x = int.Parse(x);
                if (y.All(char.IsDigit))
                    pos.y = int.Parse(y);
            }

            pos.y = -pos.y;
            
            var moveData = new MoveData
            {
                MoveAction = action,
                MoveType = moveType,
                StartPos = new Vector2(_pictureList[pictureNumber].image.rectTransform.anchoredPosition3D.x,
                    _pictureList[pictureNumber].image.rectTransform.anchoredPosition3D.y),
                TargetPos = pos,
                MoveTime = 0,
                MoveTargetTime = frame / 60.0f,
                MoveToggle = toggle
            };
            if (!_moveDictionary.ContainsKey(pictureNumber))
            {
                _moveDictionary.Add(pictureNumber, moveData);
            }
            else
            {
                _moveDictionary[pictureNumber] = moveData;
            }
            
            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(MoveProcess);

            if (!_moveDictionary[pictureNumber].MoveToggle)
            {
                _moveDictionary[pictureNumber].MoveAction.Invoke();
            }
        }

        private void MoveProcess() {
            var keys = _moveDictionary.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                int key = keys[i];
                var moveData = _moveDictionary[key];
                moveData.MoveTime += Time.deltaTime;
                _moveDictionary[key] = moveData;
                if (moveData.MoveTime >= moveData.MoveTargetTime)
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        _pictureList[key].image.rectTransform.anchoredPosition3D =
                            Vector2.Lerp(moveData.StartPos, moveData.TargetPos, 1.0f);
                    }
                    _moveDictionary.Remove(key);

                    if (_moveDictionary.Count == 0)
                    {
                        TimeHandler.Instance.RemoveTimeAction(MoveProcess);
                    }

                    try
                    {
                        if (moveData.MoveToggle && moveData.MoveAction != null)
                        {
                            moveData.MoveToggle = false;
                            ExecuteCallback(moveData.MoveAction);
                            moveData.MoveAction = null;
                            //moveData.MoveAction?.Invoke();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        switch (moveData.MoveType)
                        {
                            case 0:
                                _pictureList[key].image.rectTransform.anchoredPosition3D =
                                    Vector2.Lerp(moveData.StartPos, moveData.TargetPos,
                                        moveData.MoveTime / moveData.MoveTargetTime);
                                break;
                            case 1:
                                _pictureList[key].image.rectTransform.anchoredPosition3D =
                                    Vector2.Lerp(moveData.StartPos, moveData.TargetPos,
                                        EaseInQuad(moveData.MoveTime / moveData.MoveTargetTime));
                                break;
                            case 2:
                                _pictureList[key].image.rectTransform.anchoredPosition3D =
                                    Vector2.Lerp(moveData.StartPos, moveData.TargetPos,
                                        EaseOutQuad(moveData.MoveTime / moveData.MoveTargetTime));
                                break;
                            case 3:
                                _pictureList[key].image.rectTransform.anchoredPosition3D =
                                    Vector2.Lerp(moveData.StartPos, moveData.TargetPos,
                                        EaseInOutQuad(moveData.MoveTime / moveData.MoveTargetTime));
                                break;
                        }
                    }
                }
            }
        }

        public void StartRotation(int pictureNumber, int rotation) {
            if (_pictureList.ContainsKey(pictureNumber))
            {
                RotationData work = new RotationData();
                work.rotation = rotation * -1;
                _rotationDictionary.Add(pictureNumber, work);
                SetAnchor(pictureNumber, 1);
                TimeHandler.Instance.AddTimeActionEveryFrame(RotationProcess);
            }
        }
        
        private void RotationProcess() {
            foreach (var data in _rotationDictionary)
            {
                if (_pictureList.ContainsKey(data.Key))
                    _pictureList[data.Key].image.transform.Rotate(new Vector3(0, 0, data.Value.rotation));
            }
        }

        public void StartChangeColor(
            Action action,
            Color color,
            int pictureNumber,
            float gray,
            int frame,
            bool toggle
        ) {

            var nowColor = _pictureList[pictureNumber].image.color * 255f;
            var chaneColorData = new ChangeColorData
            {
                ChangeColorAction = action,
                ChangeColorTime = 0,
                ChangeColorTargetTime = frame / 60.0f,
                ChangeColorToggle = toggle,
                TargetColor = color,
                TargetGray = gray,
                NowColor = nowColor,
                Gray = nowColor.a
            };
            
            if (!_changeColorDictionary.ContainsKey(pictureNumber))
            {
                _changeColorDictionary.Add(pictureNumber, chaneColorData);
            }
            else
            {
                _changeColorDictionary[pictureNumber] = chaneColorData;
            }


            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(ChangeColorProcess);

            if (!_changeColorDictionary[pictureNumber].ChangeColorToggle)
            {
                _changeColorDictionary[pictureNumber].ChangeColorAction?.Invoke();
            }
        }

        public void ChangeColorProcess() {
            var keys = _changeColorDictionary.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                int key = keys[i];
                var changeColorData = _changeColorDictionary[key];
                changeColorData.ChangeColorTime += Time.deltaTime;
                _changeColorDictionary[key] = changeColorData;
                
                
                if (changeColorData.ChangeColorTime >= changeColorData.ChangeColorTargetTime)
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        _pictureList[key].image.color = new Color(changeColorData.TargetColor.r / 255f, changeColorData.TargetColor.g / 255f,
                            changeColorData.TargetColor.b / 255f, changeColorData.TargetGray / 255f);
                    }
                    _changeColorDictionary.Remove(key);

                    if (_changeColorDictionary.Count == 0)
                    {
                        TimeHandler.Instance.RemoveTimeAction(ChangeColorProcess);
                    }

                    try
                    {
                        if (changeColorData.ChangeColorToggle && changeColorData.ChangeColorAction != null)
                        {
                            changeColorData.ChangeColorToggle = false;
                            ExecuteCallback(changeColorData.ChangeColorAction);
                            changeColorData.ChangeColorAction = null;
                            //changeColorData.ChangeColorAction?.Invoke();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        _pictureList[key].image.color = new Color(
                            ((changeColorData.TargetColor.r - changeColorData.NowColor.r) * (changeColorData.ChangeColorTime / changeColorData.ChangeColorTargetTime) + changeColorData.NowColor.r) /
                            255f,
                            ((changeColorData.TargetColor.g - changeColorData.NowColor.g) * (changeColorData.ChangeColorTime / changeColorData.ChangeColorTargetTime) + changeColorData.NowColor.g) /
                            255f,
                            ((changeColorData.TargetColor.b - changeColorData.NowColor.b) * (changeColorData.ChangeColorTime / changeColorData.ChangeColorTargetTime) + changeColorData.NowColor.b) /
                            255f,
                            ((changeColorData.TargetGray - changeColorData.Gray) * (changeColorData.ChangeColorTime / changeColorData.ChangeColorTargetTime) + changeColorData.Gray) / 255.0f);
                    }
                }
            }
        }

        public void StartChangeSize(
            int pictureNumber,
            int frame,
            int x,
            int y
        ) {
            var changeSizeData = new ChangeSizeData()
            {
                ChangeSizeTime = 0,
                ChangeSizeTargetTime = frame / 60.0f,
                StartSize = _pictureList[pictureNumber].image.GetComponent<RectTransform>().sizeDelta,
                TargetSize = _pictureList[pictureNumber].size * new Vector2(x * 0.01f, y * 0.01f)
            };


            if (!_changeSizeDictionary.ContainsKey(pictureNumber))
            {
                _changeSizeDictionary.Add(pictureNumber, changeSizeData);
            }
            else
            {
                _changeSizeDictionary[pictureNumber] = changeSizeData;
            }

            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(ChangeSizeProcess);
        }

        public void ChangeSizeProcess() {
            
            var keys = _changeSizeDictionary.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                int key = keys[i];
                var changeSizeData = _changeSizeDictionary[key];
                changeSizeData.ChangeSizeTime += Time.deltaTime;
                _changeSizeDictionary[key] = changeSizeData;

                if (changeSizeData.ChangeSizeTime >= changeSizeData.ChangeSizeTargetTime)
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        var rect = _pictureList[key].image.GetComponent<RectTransform>();
                        rect.sizeDelta = Vector2.Lerp(changeSizeData.StartSize, changeSizeData.TargetSize, 1.0f);
                    }
                    _changeSizeDictionary.Remove(key);

                    if (_changeSizeDictionary.Count == 0)
                    {
                        TimeHandler.Instance.RemoveTimeAction(ChangeSizeProcess);
                    }

                    try
                    {
                        if (_moveDictionary.ContainsKey(key))
                        {
                            var moveData = _moveDictionary[key];
                            if (moveData.MoveToggle && moveData.MoveAction != null)
                            {
                                moveData.MoveToggle = false;
                                ExecuteCallback(moveData.MoveAction);
                                moveData.MoveAction = null;
                                //moveData.MoveAction?.Invoke();
                                _moveDictionary[key] = moveData;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    if (_pictureList.ContainsKey(key))
                    {
                        var rect = _pictureList[key].image.GetComponent<RectTransform>();
                        rect.sizeDelta = Vector2.Lerp(changeSizeData.StartSize, changeSizeData.TargetSize, changeSizeData.ChangeSizeTime / changeSizeData.ChangeSizeTargetTime);
                    }
                }
            }
        }

        public void DeletePicture(int pictureNumber, bool onlyEvent = false) {
            //指定したキーに何もなかったら何もしない
            if (!_pictureList.ContainsKey(pictureNumber))
                return;

            if (_moveDictionary.ContainsKey(pictureNumber))
            {
                _moveDictionary.Remove(pictureNumber);
                if (_moveDictionary.Count == 0)
                {
                    TimeHandler.Instance.RemoveTimeAction(MoveProcess);
                }
            }

            if (_changeColorDictionary.ContainsKey(pictureNumber))
            {
                _changeColorDictionary.Remove(pictureNumber);
                if (_changeColorDictionary.Count == 0)
                {
                    TimeHandler.Instance.RemoveTimeAction(ChangeColorProcess);
                }
            }

            if (_changeSizeDictionary.ContainsKey(pictureNumber))
            {
                _changeSizeDictionary.Remove(pictureNumber);
                if (_changeSizeDictionary.Count == 0)
                {
                    TimeHandler.Instance.RemoveTimeAction(ChangeSizeProcess);
                }
            }

            if (_rotationDictionary.ContainsKey(pictureNumber))
            {
                _rotationDictionary.Remove(pictureNumber);
                if (_rotationDictionary.Count == 0)
                {
                    TimeHandler.Instance.RemoveTimeAction(RotationProcess);
                }
            }

            if (!onlyEvent)
            {
                //イベント以外の全てを破棄する場合
                Destroy(_pictureList[pictureNumber].image.gameObject.transform.gameObject);
                _pictureList.Remove(pictureNumber);
                _pictureName.Remove(pictureNumber);
            }
            else
            {
                //イベントのみを破棄する場合は、データを初期化
                //obj の初期化
                _pictureList[pictureNumber].image.transform.parent.transform.localScale = new Vector3(1, 1, 1);

                //image 関連の初期化
                _pictureList[pictureNumber].image.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                _pictureList[pictureNumber].image.transform.localRotation = Quaternion.Euler(0, 0, 0);
                _pictureList[pictureNumber].image.color = new Color(1, 1, 1, 1);
            }
        }

        private async void ExecuteCallback(Action action) {
            await Task.Delay(1);
            action?.Invoke();
        }


        public float EaseInQuad(float f) {
            return f * f;
        }

        public float EaseOutQuad(float f) {
            return 1 - (1 - f) * (1 - f);
        }

        public float EaseInOutQuad(float f) {
            return f < 0.5 ? 2 * f * f : 1 - Mathf.Pow(-2 * f + 2, 2) / 2;
        }

        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(MoveProcess);
            TimeHandler.Instance?.RemoveTimeAction(ChangeColorProcess);
            TimeHandler.Instance?.RemoveTimeAction(ChangeSizeProcess);
            TimeHandler.Instance?.RemoveTimeAction(RotationProcess);

            foreach(var data in _pictureList.Values)
            {
                Destroy(data.image.gameObject);
            }
            Destroy(_prefab);
        }

        public void SavePicture() {
            //ScreenPicture
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.pictureData.Clear();
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.moveData.Clear();
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeColorData.Clear();
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeSizeData.Clear();
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.rotateData.Clear();

            if (_pictureName != null)
            {
                foreach (var data in _pictureName)
                {
                    CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.PictureData work = new CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.PictureData();
                    work.key = data.Key;
                    work.parameters = data.Value;
                    work.posX = _pictureList[work.key].image.rectTransform.anchoredPosition3D.x;
                    work.posY = _pictureList[work.key].image.rectTransform.anchoredPosition3D.y;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.pictureData.Add(work);
                }
            }

            if (_moveDictionary != null)
            {
                foreach (var data in _moveDictionary)
                {
                    CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.MoveData work = new CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.MoveData();
                    work.key = data.Key;
                    work.MoveType = data.Value.MoveType;
                    work.StartPosX = data.Value.StartPos.x;
                    work.StartPosY = data.Value.StartPos.y;
                    work.TargetPosX = data.Value.TargetPos.x;
                    work.TargetPosY = data.Value.TargetPos.y;
                    work.MoveTime = data.Value.MoveTime;
                    work.MoveTargetTime = data.Value.MoveTargetTime;
                    work.MoveToggle = data.Value.MoveToggle;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.moveData.Add(work);
                }
            }

            if (_changeColorDictionary != null)
            {
                foreach (var data in _changeColorDictionary)
                {
                    CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.ChangeColorData work = new CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.ChangeColorData();
                    work.key = data.Key;
                    work.ChangeColorTime = data.Value.ChangeColorTime;
                    work.ChangeColorTargetTime = data.Value.ChangeColorTargetTime;
                    work.ChangeColorToggle = data.Value.ChangeColorToggle;
                    work.NowColorR = data.Value.NowColor.r;
                    work.NowColorG = data.Value.NowColor.g;
                    work.NowColorB = data.Value.NowColor.b;
                    work.TargetColorR = data.Value.TargetColor.r;
                    work.TargetColorG = data.Value.TargetColor.g;
                    work.TargetColorB = data.Value.TargetColor.b;
                    work.Gray = data.Value.Gray;
                    work.TargetGray = data.Value.TargetGray;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeColorData.Add(work);
                }
            }

            if (_changeSizeDictionary != null)
            {
                foreach (var data in _changeSizeDictionary)
                {
                    CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.ChangeSizeData work = new CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.ChangeSizeData();
                    work.key = data.Key;
                    work.ChangeSizeTime = data.Value.ChangeSizeTime;
                    work.ChangeSizeTargetTime = data.Value.ChangeSizeTargetTime;
                    work.StartSizeX = data.Value.StartSize.x;
                    work.StartSizeY = data.Value.StartSize.y;
                    work.TargetSizeX = data.Value.TargetSize.x;
                    work.TargetSizeY = data.Value.TargetSize.y;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeSizeData.Add(work);
                }
            }

            if (_rotationDictionary != null)
            {
                foreach (var data in _rotationDictionary)
                {
                    CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.RotationData work = new CoreSystem.Knowledge.DataModel.Runtime.RuntimeScreenDataModel.RotationData();
                    work.key = data.Key;
                    work.rotation = data.Value.rotation;
                    work.nowRotation = _pictureList[data.Key].image.transform.rotation.z;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.rotateData.Add(work);
                }
            }
        }

        public void LoadPicture() {
            _pictureName = new Dictionary<int, List<string>>();
            _pictureList = new Dictionary<int, PictureData>();
            _moveDictionary ??= new Dictionary<int, MoveData>();
            _changeColorDictionary ??= new Dictionary<int, ChangeColorData>();
            _changeSizeDictionary ??= new Dictionary<int, ChangeSizeData>();
            _rotationDictionary ??= new Dictionary<int, RotationData>();

            foreach (var data in DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.pictureData)
            {
                //画像の番号
                var pictureNumber = data.key;
                //既に他のが画像が表示されていた場合一度消す
                DeletePicture(pictureNumber);
                //画像の表示
                AddPicture(pictureNumber, data.parameters[1]);
                //アンカー
                SetAnchor(pictureNumber, int.Parse(data.parameters[2]));
                //座標なのか、変数なのか
                SetPosition(pictureNumber,
                    int.Parse(data.parameters[3]),
                    data.parameters[4], data.parameters[5]);
                //幅,高さ
                SetPictureSize(pictureNumber,
                    int.Parse(data.parameters[6]),
                    int.Parse(data.parameters[7]));
                //不透明度
                SetPictureOpacity(pictureNumber, int.Parse(data.parameters[8]));
                //"通常", "加算", "乗算", "スクリーン";
                SetProcessingType(pictureNumber, int.Parse(data.parameters[9]));
                //元の座標を復元
                _pictureList[pictureNumber].image.rectTransform.anchoredPosition3D = new Vector3(data.posX, data.posY, -9);
                _pictureName.Add(pictureNumber, data.parameters);
            }

            foreach (var data in DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.moveData)
            {
                MoveData work = new MoveData();
                work.MoveType = data.MoveType;
                work.StartPos = new Vector2(data.StartPosX, data.StartPosY);
                work.TargetPos = new Vector2(data.TargetPosX, data.TargetPosY);
                work.MoveTargetTime = data.MoveTargetTime;
                work.MoveTime = data.MoveTime;
                work.MoveToggle = data.MoveToggle;
                _moveDictionary.Add(data.key, work);
            }

            foreach (var data in DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeColorData)
            {
                ChangeColorData work = new ChangeColorData();
                work.ChangeColorTime = data.ChangeColorTime;
                work.ChangeColorTargetTime = data.ChangeColorTargetTime;
                work.ChangeColorToggle = data.ChangeColorToggle;
                work.NowColor = new Color(data.NowColorR, data.NowColorG, data.NowColorB);
                work.TargetColor = new Color(data.TargetColorR, data.TargetColorG, data.TargetColorB);
                work.Gray = data.Gray;
                work.TargetGray = data.TargetGray;
                _changeColorDictionary.Add(data.key, work);
            }

            foreach (var data in DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.changeSizeData)
            {
                ChangeSizeData work = new ChangeSizeData();
                work.ChangeSizeTime = data.ChangeSizeTime;
                work.ChangeSizeTargetTime = data.ChangeSizeTargetTime;
                work.StartSize = new Vector2(data.StartSizeX, data.StartSizeY);
                work.TargetSize = new Vector2(data.TargetSizeX, data.TargetSizeY);
                _changeSizeDictionary.Add(data.key, work);
            }

            foreach (var data in DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.rotateData)
            {
                RotationData work = new RotationData();
                work.rotation = data.rotation;
                _rotationDictionary.Add(data.key, work);
                SetAnchor(data.key, 1);
            }

            if (_moveDictionary.Count > 0)
            {
                TimeHandler.Instance.AddTimeActionEveryFrame(MoveProcess);
            }
            if (_changeColorDictionary.Count > 0)
            {
                TimeHandler.Instance.AddTimeActionEveryFrame(ChangeColorProcess);
            }
            if (_changeSizeDictionary.Count > 0)
            {
                TimeHandler.Instance.AddTimeActionEveryFrame(ChangeSizeProcess);
            }
            if (_rotationDictionary.Count > 0)
            {
                TimeHandler.Instance.AddTimeActionEveryFrame(RotationProcess);
            }
        }

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        public struct PictureData
        {
            public Image   image;
            public Vector2 size;
        }
        
        public struct MoveData
        {
            public Action  MoveAction;
            public int     MoveType;
            public Vector2 StartPos;
            public Vector2 TargetPos;
            public float   MoveTime;
            public float   MoveTargetTime;
            public bool    MoveToggle;
        }
        
        public struct ChangeColorData
        {
            public Action  ChangeColorAction;
            public float   ChangeColorTime;
            public float   ChangeColorTargetTime;
            public bool    ChangeColorToggle;
            public Color   TargetColor;
            public Color   NowColor;
            public float   Gray;
            public float   TargetGray;
        }
        
        public struct ChangeSizeData
        {
            public float ChangeSizeTime;
            public float ChangeSizeTargetTime;
            public Vector2 StartSize;
            public Vector2 TargetSize;
        }

        public struct RotationData
        {
            public float rotation;
        }
    }
}