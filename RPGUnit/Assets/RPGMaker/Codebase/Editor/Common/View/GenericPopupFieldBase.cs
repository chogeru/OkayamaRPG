using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common.Enum;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    /// <summary>
    /// 選択項目がジェネリックなPopupFieldBase。
    /// </summary>
    /// <remarks>
    /// PopupFieldBase<T>は一見ジェネリックだが実際はstring専用なので本クラスを用意した。
    /// フィールドに PopupFieldBase<string> を持って使用している。
    /// </remarks>
    public class GenericPopupFieldBase<T> : VisualElement
        where T : IChoice
    {
        private readonly PopupFieldBase<string> popupFieldBase;
        private readonly List<T> choices;
        private int previousIndex = -1;
        private EventCallback<ChangeEvent<T>> changeEventCallback;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="choices">選択項目列。</param>
        /// <param name="defaultIndex">既定インデックス。</param>
        public GenericPopupFieldBase(List<T> choices, int defaultIndex)
        {
            this.choices = choices;

            var choiceIndex = defaultIndex < 0 || defaultIndex >= choices.Count ? 0 : defaultIndex;

            popupFieldBase = new PopupFieldBase<string>(
                choices.Select(choice => choice.Name).ToList(), choiceIndex);

            popupFieldBase.RegisterValueChangedCallback(stringChangeEvent =>
            {
                StringChangeEvent = stringChangeEvent;

                changeEventCallback?.Invoke(
                    ChangeEvent<T>.GetPooled(
                        previousIndex >= 0 && previousIndex < choices.Count ? choices[previousIndex] : default(T),
                        choices[popupFieldBase.index]));

                previousIndex = popupFieldBase.index;
            });
        }

        public ChangeEvent<string> StringChangeEvent { get; private set; }

        /// <summary>
        /// GenericPopupFieldBaseをVisualElementツリーに追加する。
        /// </summary>
        /// <param name="rootVisualElement">追加先のルートVisualElement</param>
        /// <param name="containerName">追加先のVisualElementコンテナ名。</param>
        /// <param name="choices">選択項目列。</param>
        /// <param name="defaultId">既定の選択中の項目のid。</param>
        /// <returns>追加したGenericPopupFieldBase。</returns>
        public static GenericPopupFieldBase<T> Add(
            VisualElement rootVisualElement,
            string containerName,
            List<T> choices,
            string defaultId)
        {
            return Add(rootVisualElement.Q<VisualElement>(containerName), choices, defaultId);
        }

        /// <summary>
        /// GenericPopupFieldBaseをVisualElementツリーに追加する。
        /// </summary>
        /// <param name="container">追加先VisualElement</param>
        /// <param name="choices">選択項目列。</param>
        /// <param name="defaultId">既定の選択中の項目のid。</param>
        /// <returns>追加したGenericPopupFieldBase。</returns>
        public static GenericPopupFieldBase<T> Add(
            VisualElement container,
            List<T> choices,
            string defaultId)
        {
            var defaultIndex = choices.GenericIndexOf(choice => defaultId != null && choice.Id == defaultId);
            if (defaultIndex < 0 && choices.Any())
            {
                defaultIndex = 0;
            }

            var popupFieldGeneric = new GenericPopupFieldBase<T>(choices, defaultIndex);
            container.Clear();
            container.Add(popupFieldGeneric.popupFieldBase);
            return popupFieldGeneric;
        }

        public int index => popupFieldBase.index;
        public T value => choices[index];

        public void ForceSetValue(T value)
        {
            popupFieldBase.ForceSetIndex(choices.IndexOf(value));
        }

        public bool RegisterValueChangedCallback(EventCallback<ChangeEvent<T>> callback)
        {
            changeEventCallback = callback;
            return true;
        }
    }

    public interface IChoice
    {
        public string Name { get; }
        public string Id { get; }
    }

    public class MapDataChoice : IChoice
    {
        public readonly MapDataModel mapDataModel;

        public MapDataChoice(MapDataModel mapDataModel)
        {
            this.mapDataModel = mapDataModel;
        }

        public MapDataModel MapDataModel => mapDataModel;
        public string Name => MapDataModel != null ? MapDataModel.name : EditorLocalize.LocalizeText("WORD_0113");
        public string Id => MapDataModel != null ? MapDataModel.id : "-1";

        public static List<MapDataChoice> GenerateChoices()
        {
            var choices = new List<MapDataChoice>
            {
                // 選択項目『なし』。
                new MapDataChoice(null)
            };

            choices.AddRange(
                Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps().Select(mapDataModel => new MapDataChoice(mapDataModel)));
            return choices;
        }
    }

    /// <summary>
    /// 対象キャラクター選択項目クラス。
    /// </summary>
    public class TargetCharacterChoice : IChoice
    {
        public readonly EventMapDataModel eventMapDataModel;

        public TargetCharacterChoice(EventMapDataModel eventMapDataModel)
        {
            this.eventMapDataModel = eventMapDataModel;
        }

        public EventMapDataModel EventMapDataModel => eventMapDataModel;
        public string Name =>
            EventMapDataModel.eventId == Commons.TargetType.Player.GetTargetCharacterId() ||
            EventMapDataModel.eventId == Commons.TargetType.ThisEvent.GetTargetCharacterId() ?
                EventMapDataModel.name :
                AbstractCommandEditor.GetEventDisplayName(EventMapDataModel);
        public string Id => EventMapDataModel.eventId;

        /// <summary>
        /// 選択可能な対象キャラクター列を生成する。
        /// </summary>
        /// <param name="mapId">このidのイベントが配置されているマップの</param>
        /// <param name="excludePlayer">選択項目『プレイヤー』を含めない。</param>
        /// <returns></returns>
        public static List<TargetCharacterChoice> GenerateChoices(string mapId, bool excludePlayer = false)
        {
            var choices = new List<TargetCharacterChoice>
            {
                // 選択項目『プレイヤー』。
                {
                    new TargetCharacterChoice(
                        new EventMapDataModel()
                        {
                            name = EditorLocalize.LocalizeText("WORD_0860"),
                            eventId = Commons.TargetType.Player.GetTargetCharacterId(),
                        })
                },
                // 選択項目『このイベント』。
                {
                    new TargetCharacterChoice(
                        new EventMapDataModel()
                        {
                            name = EditorLocalize.LocalizeText("WORD_0920"),
                            eventId = Commons.TargetType.ThisEvent.GetTargetCharacterId()
                        })
                },
            };

            if (excludePlayer)
            {
                choices.RemoveAt(0);
            }

            choices.AddRange(
                AbstractCommandEditor.GetEventMapDataModelsInMap(mapId).
                    Select(eventMapModel => new TargetCharacterChoice(eventMapModel)));

            return choices;
        }
    }

    /// <summary>
    /// サウンドリスト生成クラス
    /// </summary>
    public class SoundDataList
    {
        /// <summary>
        /// 選択可能なサウンドデータを生成する
        /// </summary>
        /// <param name="soundTypes">BGM,BGS,ME,SEの配列</param>
        /// <param name="soundFileName">初期値</param>
        /// <returns></returns>
        public static List<SoundDataChoice> GenerateChoices(List<SoundType> soundTypes) {
            List<SoundDataChoice> choices = new List<SoundDataChoice>();

            //リストの先頭に「なし」を追加する
            choices.Add(new SoundDataChoice(EditorLocalize.LocalizeText("WORD_0113"), ""));

            //サウンドタイプ分、データを追加する
            for (int i = 0; i < soundTypes.Count; i++)
            {
                choices.AddRange(GetSoundList(soundTypes[i]));
            }

            //作成したサウンドデータのリストを返却
            return choices;
        }

        /// <summary>
        /// サウンドのリスト取得
        /// </summary>
        /// <returns></returns>
        private static List<SoundDataChoice> GetSoundList(SoundType soundType) {
            DirectoryInfo dir;
            if (soundType == SoundType.Bgm)
                dir = new DirectoryInfo(PathManager.SOUND_BGM);
            else if (soundType == SoundType.Bgs)
                dir = new DirectoryInfo(PathManager.SOUND_BGS);
            else if (soundType == SoundType.Me)
                dir = new DirectoryInfo(PathManager.SOUND_ME);
            else if (soundType == SoundType.Se)
                dir = new DirectoryInfo(PathManager.SOUND_SE);
            else 
                return null;

            List<SoundDataChoice> soundData = new List<SoundDataChoice>();

            var info = dir.GetFiles("*.ogg");
            foreach (var f in info) soundData.Add(new SoundDataChoice(f.Name.Replace(".ogg", ""), "ogg"));
            info = dir.GetFiles("*.wav");
            foreach (var f in info) soundData.Add(new SoundDataChoice(f.Name.Replace(".wav", ""), "wav"));

            return soundData;
        }

        /// <summary>
        /// 対象の音データ選択項目クラス
        /// </summary>
        public class SoundDataChoice : IChoice
        {
            public string filename;
            public string extention;

            string IChoice.Name => filename;

            string IChoice.Id => extention == "" ? filename : filename + "." + extention;

            public SoundDataChoice(string filename, string extention) {
                this.filename = filename;
                this.extention = extention;
            }
        }
    }
}
