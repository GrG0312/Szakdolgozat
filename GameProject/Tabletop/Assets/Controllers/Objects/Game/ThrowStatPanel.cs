using Model.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Model.UnityDependant;
using System.Collections.Generic;

namespace Controllers.Objects.Game
{
    public class ThrowStatPanel : NetworkBehaviour
    {
        private const int SIDE_PANEL_WIDTH = 400;
        private const int CONTAINER_HEIGHT = 60;

        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text ballisticsText;
        [SerializeField] private TMP_Text armorPiercingText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text armorSaveText;

        [SerializeField] private TMP_Text rollTextPrefab;
        [SerializeField] private GridLayoutGroup offensiveRollsContainer;
        [SerializeField] private GridLayoutGroup defensiveRollsContainer;

        [SerializeField] private Color goodColor;
        [SerializeField] private Color badColor;

        private NetworkList<int> offensiveResultsNetList = new NetworkList<int>();
        private NetworkList<int> defensiveResultsNetList = new NetworkList<int>();

        private NetworkVariable<int> attackNetVar = new NetworkVariable<int>();
        private NetworkVariable<int> ballisticsNetVar = new NetworkVariable<int>();
        private NetworkVariable<int> armorPiercingNetVar = new NetworkVariable<int>();
        private NetworkVariable<int> damageNetVar = new NetworkVariable<int>();
        private NetworkVariable<int> armorSaveNetVar = new NetworkVariable<int>();
        private NetworkVariable<bool> didRollOffensiveNetVar = new NetworkVariable<bool>();

        private List<TMP_Text> offensiveRollTextObjects = new List<TMP_Text>();
        private List<TMP_Text> defensiveRollTextObjects = new List<TMP_Text>();

        private DiceRoller roller;

        public override void OnNetworkSpawn()
        {
            attackNetVar.OnValueChanged += AttacksStatChanged;
            ballisticsNetVar.OnValueChanged += BallisticsStatChanged;
            armorPiercingNetVar.OnValueChanged += ArmorPiercingStatChanged;
            damageNetVar.OnValueChanged += DamageStatChanged;
            armorSaveNetVar.OnValueChanged += ArmorSaveStatChanged;

            offensiveResultsNetList.OnListChanged += OnOffensiveResultChange;
            defensiveResultsNetList.OnListChanged += OnDefensiveResultChange;
        }

        public void RollFinished(int[] results)
        {
            if (!didRollOffensiveNetVar.Value)
            {
                foreach (int result in results)
                {
                    offensiveResultsNetList.Add(result);
                }

                didRollOffensiveNetVar.Value = true;
            }
            else
            {
                foreach (int result in results)
                {
                    defensiveResultsNetList.Add(result);
                }
            }
        }

        #region Registering variables

        public void RegisterThrower(DiceRoller r)
        {
            roller = r;
            roller.Listeneres.Add(RollFinished);
        }

        public void RegisterAttacker(int a, int bs, int ap, int d)
        {
            attackNetVar.Value = a;
            ballisticsNetVar.Value = bs;
            armorPiercingNetVar.Value = ap;
            damageNetVar.Value = d;


        }

        public void RegisterDefender(int ars)
        {
            offensiveResultsNetList.Clear();
            defensiveResultsNetList.Clear();
            didRollOffensiveNetVar.Value = false;
            armorSaveNetVar.Value = ars;
        }

        #endregion

        #region Network variable event handlers

        private void AttacksStatChanged(int oldvalue, int newvalue)
        {
            attackText.text = newvalue.ToString();
        }

        private void BallisticsStatChanged(int oldvalue, int newvalue)
        {
            ballisticsText.text = newvalue.ToString() + "+";
        }

        private void ArmorPiercingStatChanged(int oldvalue, int newvalue)
        {
            ballisticsText.text = newvalue.ToString();
        }

        private void DamageStatChanged(int oldvalue, int newvalue)
        {
            damageText.text = newvalue.ToString();
        }

        private void ArmorSaveStatChanged(int oldvalue, int newvalue)
        {
            armorSaveText.text = newvalue.ToString() + "+";
        }

        #endregion

        #region Network list change handlers

        private void OnOffensiveResultChange(NetworkListEvent<int> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<int>.EventType.Add:
                    TMP_Text newtext = Instantiate(rollTextPrefab, offensiveRollsContainer.transform);
                    newtext.text = changeEvent.Value.ToString();
                    if (changeEvent.Value > ballisticsNetVar.Value)
                    {
                        newtext.color = goodColor;
                    }
                    else
                    {
                        newtext.color = badColor;
                    }
                    offensiveRollTextObjects.Add(newtext);
                    offensiveRollsContainer.cellSize = new Vector2(SIDE_PANEL_WIDTH / offensiveRollTextObjects.Count, CONTAINER_HEIGHT);
                    break;
                case NetworkListEvent<int>.EventType.Clear:
                    foreach (TMP_Text text in offensiveRollTextObjects)
                    {
                        Destroy(text.gameObject);
                    }
                    offensiveRollTextObjects.Clear();
                    break;
                default:
                    break;
            }
        }

        private void OnDefensiveResultChange(NetworkListEvent<int> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<int>.EventType.Add:
                    TMP_Text newtext = Instantiate(rollTextPrefab, defensiveRollsContainer.transform);
                    newtext.text = changeEvent.Value.ToString();
                    if (changeEvent.Value > armorSaveNetVar.Value)
                    {
                        newtext.color = goodColor;
                    }
                    else
                    {
                        newtext.color = badColor;
                    }
                    defensiveRollTextObjects.Add(newtext);
                    defensiveRollsContainer.cellSize = new Vector2(SIDE_PANEL_WIDTH / defensiveRollTextObjects.Count, CONTAINER_HEIGHT);
                    break;
                case NetworkListEvent<int>.EventType.Clear:
                    foreach (TMP_Text text in defensiveRollTextObjects)
                    {
                        Destroy(text.gameObject);
                    }
                    defensiveRollTextObjects.Clear();
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}