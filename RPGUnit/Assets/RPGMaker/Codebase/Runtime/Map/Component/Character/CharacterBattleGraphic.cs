using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System;
using UnityEngine;


namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public class CharacterBattleGraphic : CharacterGraphic
    {
        
        private SpriteDataModel _sdAdvanceSprite;
        private SpriteDataModel _sdWaitSprite;
        private SpriteDataModel _sdChantSprite;
        private SpriteDataModel _sdDefendSprite;
        private SpriteDataModel _sdDamageSprite;
        private SpriteDataModel _sdEvadeSprite;
        private SpriteDataModel _sdThrustSprite;
        private SpriteDataModel _sdSwingSprite;
        private SpriteDataModel _sdProjectileSprite;
        private SpriteDataModel _sdSkillSprite;
        private SpriteDataModel _sdMagicSprite;
        private SpriteDataModel _sdItemSprite;
        private SpriteDataModel _sdEscapeSprite;
        private SpriteDataModel _sdWinSprite;
        private SpriteDataModel _sdNearFallenSprite;
        private SpriteDataModel _sdStatusAilmentSprite;
        private SpriteDataModel _sdSleepSprite;
        private SpriteDataModel _sdFallenSprite;
        
        public override void ChangeAsset(string assetId) {
            // 画像が設定されていなければ初期化
            if (_image == null)
            {
                Init(assetId);
            }

            if (_image != null && _image.enabled == false)
                _image.enabled = true;

            SetUpSprites(assetId);
            _currentSprite = (int)_directionEnum switch
            {
                0 => _sdAdvanceSprite,
                1 => _sdWaitSprite,
                2 => _sdChantSprite,
                3 => _sdDefendSprite,
                4 => _sdDamageSprite,
                5 => _sdEvadeSprite,
                6 => _sdThrustSprite,
                7 => _sdSwingSprite,
                8 => _sdProjectileSprite,
                9 => _sdSkillSprite,
                10 => _sdMagicSprite,
                11 => _sdItemSprite,
                12 => _sdEscapeSprite,
                13 => _sdWinSprite,
                14 => _sdNearFallenSprite,
                15 => _sdStatusAilmentSprite,
                16 => _sdSleepSprite,
                17 => _sdFallenSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            _currentSpriteIndex = 0;
            ChangeTextureSize();
            Render();
        }


        public override void ChangeDirection(CharacterMoveDirectionEnum directionEnum) {
            if (directionEnum == _directionEnum) return;

            _directionEnum = directionEnum;
            _currentSprite = (int)_directionEnum switch
            {
                0 => _sdAdvanceSprite,
                1 => _sdWaitSprite,
                2 => _sdChantSprite,
                3 => _sdDefendSprite,
                4 => _sdDamageSprite,
                5 => _sdEvadeSprite,
                6 => _sdThrustSprite,
                7 => _sdSwingSprite,
                8 => _sdProjectileSprite,
                9 => _sdSkillSprite,
                10 => _sdMagicSprite,
                11 => _sdItemSprite,
                12 => _sdEscapeSprite,
                13 => _sdWinSprite,
                14 => _sdNearFallenSprite,
                15 => _sdStatusAilmentSprite,
                16 => _sdSleepSprite,
                17 => _sdFallenSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
            _currentSpriteIndex = 0;
            ChangeTextureSize();
            Render();
        }
        
        protected override void ChangeTextureSize() {
            var spriteData = (int)_directionEnum switch
            {
                0 => _sdAdvanceSprite,
                1 => _sdWaitSprite,
                2 => _sdChantSprite,
                3 => _sdDefendSprite,
                4 => _sdDamageSprite,
                5 => _sdEvadeSprite,
                6 => _sdThrustSprite,
                7 => _sdSwingSprite,
                8 => _sdProjectileSprite,
                9 => _sdSkillSprite,
                10 => _sdMagicSprite,
                11 => _sdItemSprite,
                12 => _sdEscapeSprite,
                13 => _sdWinSprite,
                14 => _sdNearFallenSprite,
                15 => _sdStatusAilmentSprite,
                16 => _sdSleepSprite,
                17 => _sdFallenSprite,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (spriteData?.sprite?.texture != null)
                SetSize(new Vector2(
                    spriteData.sprite.texture.width / spriteData.animationFrame / TILE_SIZE,
                    spriteData.sprite.texture.height / TILE_SIZE));
        }

        protected override void SetUpSprites(string assetId) {
            if (assetId != "")
            {
                // idとファイル名は同名の為そのままロード
#if UNITY_EDITOR
                var inputString =
                    UnityEditorWrapper.AssetDatabaseWrapper
                        .LoadAssetAtPath<TextAsset>(
                            "Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + assetId + ".json");
                var assetManageData = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
#else
                var assetManageData =
					ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + assetId + ".asset") as AssetManageDataModel;
#endif
                //各方向のキャラクター画像を読み込む。画像がnullだった場合はオブジェクト用の画像から探す
                var advance = LoadCharacterSprite(assetManageData.imageSettings[0].path)
                           ?? LoadObjectSprite(assetManageData.imageSettings[0].path);
                var wait = LoadCharacterSprite(assetManageData.imageSettings[1].path)
                           ?? LoadObjectSprite(assetManageData.imageSettings[1].path);
                var chant = LoadCharacterSprite(assetManageData.imageSettings[2].path)
                            ?? LoadObjectSprite(assetManageData.imageSettings[2].path);
                var defend = LoadCharacterSprite(assetManageData.imageSettings[3].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[3].path);
                var damage = LoadCharacterSprite(assetManageData.imageSettings[4].path)
                             ?? LoadObjectSprite(assetManageData.imageSettings[4].path);
                var evade = LoadCharacterSprite(assetManageData.imageSettings[5].path)
                             ?? LoadObjectSprite(assetManageData.imageSettings[5].path);
                var thrust = LoadCharacterSprite(assetManageData.imageSettings[6].path)
                             ?? LoadObjectSprite(assetManageData.imageSettings[6].path);
                var swing = LoadCharacterSprite(assetManageData.imageSettings[7].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[7].path);
                var projectile = LoadCharacterSprite(assetManageData.imageSettings[8].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[8].path);
                var skill = LoadCharacterSprite(assetManageData.imageSettings[9].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[9].path);
                var magic = LoadCharacterSprite(assetManageData.imageSettings[10].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[10].path);
                var item = LoadCharacterSprite(assetManageData.imageSettings[11].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[11].path);
                var escape = LoadCharacterSprite(assetManageData.imageSettings[12].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[12].path);
                var win = LoadCharacterSprite(assetManageData.imageSettings[13].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[13].path);
                var nearFallen = LoadCharacterSprite(assetManageData.imageSettings[14].path)
                         ?? LoadObjectSprite(assetManageData.imageSettings[14].path);
                var statusAilment = LoadCharacterSprite(assetManageData.imageSettings[15].path)
                                    ?? LoadObjectSprite(assetManageData.imageSettings[15].path);
                var sleep = LoadCharacterSprite(assetManageData.imageSettings[16].path)
                            ?? LoadObjectSprite(assetManageData.imageSettings[16].path);
                var fallen = LoadCharacterSprite(assetManageData.imageSettings[17].path)
                            ?? LoadObjectSprite(assetManageData.imageSettings[17].path);

                //コマ数入ってくる
                var advanceIndex = assetManageData.imageSettings[0].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[0].animationFrame;
                var waitIndex = assetManageData.imageSettings[1].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[1].animationFrame;
                var chantIndex = assetManageData.imageSettings[2].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[2].animationFrame;
                var defendIndex = assetManageData.imageSettings[3].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[3].animationFrame;
                var damageIndex = assetManageData.imageSettings[4].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[4].animationFrame;
                var evadeIndex = assetManageData.imageSettings[5].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[5].animationFrame;
                var thrustIndex = assetManageData.imageSettings[6].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[6].animationFrame;
                var swingIndex = assetManageData.imageSettings[7].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[7].animationFrame;
                var projectileIndex = assetManageData.imageSettings[8].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[8].animationFrame;
                var skillIndex = assetManageData.imageSettings[9].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[9].animationFrame;
                var magicIndex = assetManageData.imageSettings[10].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[10].animationFrame;
                var itemIndex = assetManageData.imageSettings[11].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[11].animationFrame;
                var escapeIndex = assetManageData.imageSettings[12].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[12].animationFrame;
                var winIndex = assetManageData.imageSettings[13].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[13].animationFrame;
                var nearFallenIndex = assetManageData.imageSettings[14].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[14].animationFrame;
                var statusAilmentIndex = assetManageData.imageSettings[15].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[15].animationFrame;
                var sleepIndex = assetManageData.imageSettings[16].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[16].animationFrame;
                var fallenIndex = assetManageData.imageSettings[17].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[17].animationFrame;

                // アニメーション速度
                var advanceSpeed = assetManageData.imageSettings[0].animationSpeed == 0 ? 1 : assetManageData.imageSettings[0].animationSpeed;
                var waitSpeed = assetManageData.imageSettings[1].animationSpeed == 0 ? 1 : assetManageData.imageSettings[1].animationSpeed;
                var chantSpeed = assetManageData.imageSettings[2].animationSpeed == 0 ? 1 : assetManageData.imageSettings[2].animationSpeed;
                var defendSpeed = assetManageData.imageSettings[3].animationSpeed == 0 ? 1 : assetManageData.imageSettings[3].animationSpeed;
                var damageSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[4].animationSpeed;
                var evadeSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[5].animationSpeed;
                var thrustSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[6].animationSpeed;
                var swingSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[7].animationSpeed;
                var projectileSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[8].animationSpeed;
                var skillSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[9].animationSpeed;
                var magicSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[10].animationSpeed;
                var itemSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[11].animationSpeed;
                var escapeSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[12].animationSpeed;
                var winSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[13].animationSpeed;
                var nearFallenSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[14].animationSpeed;
                var statusAilmentSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[15].animationSpeed;
                var sleepSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[16].animationSpeed;
                var fallenSpeed = assetManageData.imageSettings[4].animationSpeed == 0 ? 1 : assetManageData.imageSettings[17].animationSpeed;

                _sdAdvanceSprite = new SpriteDataModel(advance, advanceIndex, advanceSpeed);
                _sdWaitSprite = new SpriteDataModel(wait, waitIndex, waitSpeed);
                _sdChantSprite = new SpriteDataModel(chant, chantIndex, chantSpeed);
                _sdDefendSprite = new SpriteDataModel(defend, defendIndex, defendSpeed);
                _sdDamageSprite = new SpriteDataModel(damage, damageIndex, damageSpeed);
                _sdEvadeSprite = new SpriteDataModel(evade, evadeIndex, evadeSpeed);
                _sdThrustSprite = new SpriteDataModel(thrust, thrustIndex, thrustSpeed);
                _sdSwingSprite = new SpriteDataModel(swing, swingIndex, swingSpeed);
                _sdProjectileSprite = new SpriteDataModel(projectile, projectileIndex, projectileSpeed);
                _sdSkillSprite = new SpriteDataModel(skill, skillIndex, skillSpeed);
                _sdMagicSprite = new SpriteDataModel(magic, magicIndex, magicSpeed);
                _sdItemSprite = new SpriteDataModel(item, itemIndex,itemSpeed);
                _sdEscapeSprite = new SpriteDataModel(escape, escapeIndex, escapeSpeed);
                _sdWinSprite = new SpriteDataModel(win, winIndex, winSpeed);
                _sdNearFallenSprite = new SpriteDataModel(nearFallen, nearFallenIndex, nearFallenSpeed);
                _sdStatusAilmentSprite = new SpriteDataModel(statusAilment, statusAilmentIndex, statusAilmentSpeed);
                _sdSleepSprite = new SpriteDataModel(sleep, sleepIndex, sleepSpeed);
                _sdFallenSprite = new SpriteDataModel(fallen, fallenIndex, fallenSpeed);
            }
            //画像のIDが入ってこなかったら画像を消す
            else
            {
                _sdAdvanceSprite = null;
                _sdWaitSprite = null;
                _sdChantSprite = null;
                _sdDefendSprite = null;
                _sdDamageSprite = null;
                _sdEvadeSprite = null;
                _sdThrustSprite = null;
                _sdSwingSprite = null;
                _sdProjectileSprite = null;
                _sdSkillSprite = null;
                _sdMagicSprite = null;
                _sdItemSprite = null;
                _sdEscapeSprite = null;
                _sdWinSprite = null;
                _sdNearFallenSprite = null;
                _sdStatusAilmentSprite = null;
                _sdSleepSprite = null;
                _sdFallenSprite = null;
                if (_currentSprite != null)
                    _currentSprite.sprite = null;
                if (_image != null)
                    _image.enabled = false;
            }
        }
        
        /// <summary>
        ///     キャラクター用画像の読込
        /// </summary>
        /// <param name="fileName">画像のファイル名</param>
        /// <returns>読み込まれた画像のSprite</returns>
        protected override Sprite LoadCharacterSprite(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return null;

            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                "Assets/RPGMaker/Storage/Images/SV_Actors/" + fileName);
        }

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------
    }
}