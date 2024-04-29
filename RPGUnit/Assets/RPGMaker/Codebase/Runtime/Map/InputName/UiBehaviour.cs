using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.InputName
{
    public class UiBehaviour : MonoBehaviour
    {
        public delegate void OnComplete();

        public GameObject _selecetedItem;

        private void Start() {
        }


        public void ButtonRrocessing(OnComplete callback, GameObject obj, bool flg = true) {
            if (_selecetedItem != null && _selecetedItem != obj)
            {
                _selecetedItem.GetComponent<Animator>().enabled = false;
                _selecetedItem.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }


            if (_selecetedItem == obj)
            {
                callback();
            }
            else
            {
                _selecetedItem = obj;
                _selecetedItem.GetComponent<Animator>().Play("TintButton");
                _selecetedItem.GetComponent<Animator>().enabled = true;
            }

            _selecetedItem = obj;
        }

        public int GetItemOder() {
            return _selecetedItem.transform.GetSiblingIndex();
        }


        private void OnDisable() {
            if (_selecetedItem == null)
                return;
            _selecetedItem.GetComponent<Animator>().enabled = false;
            _selecetedItem.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            _selecetedItem = null;
        }

        public void PlayCancel() {
        }

        public void PlaySave() {
        }

        public void PlayUseItem() {
        }

        public void PlayBuzzer() {
        }

        public void PlayUseSkill() {
        }

        public void PlayEquip() {
        }
    }
}