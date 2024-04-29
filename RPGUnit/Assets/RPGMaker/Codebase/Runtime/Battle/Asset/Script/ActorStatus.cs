using RPGMaker.Codebase.Runtime.Battle.Objects;
using System;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Battle.Asset.Script
{
    public class ActorStatus : Selector
    {
        [SerializeField] public GameObject hpGauge;
        [SerializeField] public TextMP     hpText;
        [SerializeField] public GameObject mpGauge;
        [SerializeField] public TextMP     mpText;
        [SerializeField] public Image      StatusIcon1;
        [SerializeField] public Image      StatusIcon2;
        [SerializeField] public GameObject tpGauge;
        [SerializeField] public TextMP     tpText;

        public new void SetUp(int index, string name, Action<int> Select, Action<int> OnClick) {
            hpText.raycastTarget = false;
            mpText.raycastTarget = false;
            tpText.raycastTarget = false;
            var parent = hpText.transform.parent;
            if (parent.transform.Find("HPText") != null)
            {
                parent.transform.Find("HPText").GetComponent<TextMP>().raycastTarget = false;
            }
            parent = mpText.transform.parent;
            if (parent.transform.Find("MPText") != null)
            {
                parent.transform.Find("MPText").GetComponent<TextMP>().raycastTarget = false;
            }
            parent = tpText.transform.parent;
            if (parent.transform.Find("TPText") != null)
            {
                parent.transform.Find("TPText").GetComponent<TextMP>().raycastTarget = false;
            }
            
            base.SetUp(index, name, Select, OnClick);
        }
    }
}