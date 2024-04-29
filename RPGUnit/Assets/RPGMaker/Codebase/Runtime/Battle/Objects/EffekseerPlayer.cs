using Effekseer;
using UnityEngine;

/// <summary>
/// Effekseerの再生を行う
/// </summary>
public class EffekseerPlayer : MonoBehaviour
{
    /// <summary>
    /// エフェクトデータ
    /// </summary>
    private EffekseerEffectAsset _effect;
    /// <summary>
    /// エフェクシアの制御クラス
    /// </summary>
    private EffekseerHandle _handle;
    /// <summary>
    /// アニメーションをループするかどうか
    /// </summary>
    private bool _loop;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start() {
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update() {
        // ループ再生が設定されている場合は再生完了時に再度再生
        if (_loop && _handle.exists == false)
            _handle = EffekseerSystem.PlayEffect(_effect, transform.position);
    }

    /// <summary>
    /// エフェクト再生
    /// </summary>
    /// <param name="loop"></param>
    public void Play(bool loop = false) {
        // transformの位置でエフェクトを再生する
        _handle = EffekseerSystem.PlayEffect(_effect, transform.position);

        // transformの回転を設定する。
        _handle.SetRotation(transform.rotation);

        // ループ設定
        _loop = loop;
    }

    /// <summary>
    /// エフェクト停止
    /// </summary>
    public void Stop() {
        EffekseerSystem.StopAllEffects();
    }

    /// <summary>
    /// エフェクトデータ設定
    /// </summary>
    /// <param name="asset"></param>
    public void SetEffectData(EffekseerEffectAsset asset) {
        _effect = asset;
    }

    /// <summary>
    /// 再生中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying() {
        return _handle.exists;
    }
}