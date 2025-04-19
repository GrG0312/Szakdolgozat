using Model.GameModel;
using Model.UnityDependant;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Controllers.Objects.Game
{
    public class ControlPointController : NetworkBehaviour
    {
        [SerializeField] private List<Material> materials; // Last should be neutral skin
        [SerializeField] private GameObject visibleBorder;

        private NetworkVariable<int> materialIndex = new NetworkVariable<int>(-1);

        public ControlPointModel Model { get; private set; }

        private void Awake()
        {
            materialIndex.OnValueChanged += OnMaterialIndexChanged;
            Model = new ControlPointModel();
            Model.OwnerChanged += Model_OwnerChanged;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsHost)
            {
                return;
            }
            Debug.Log($"<color=aqua>Unit entered</color>");
            UnitModel model = other.GetComponent<UnitModel>();
            if (model != null)
            {
                Debug.Log($"<color=aqua>UnitModel is not null</color>");
                Model.ContesterChanged(model, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsHost)
            {
                return;
            }
            Debug.Log($"<color=aqua>Unit left</color>");
            UnitModel model = other.GetComponent<UnitModel>();
            if (model != null)
            {
                Debug.Log($"<color=aqua>UnitModel is not null</color>");
                Model.ContesterChanged(model, false);
            }
        }

        private void Model_OwnerChanged(object sender, int oldvalue)
        {
            materialIndex.Value = Model.Owner;
        }

        private void OnMaterialIndexChanged(int oldvalue, int newvalue)
        {
            Debug.Log($"<color=aqua>Model's owner changed: {newvalue}</color>");
            Renderer r = GetComponentInChildren<Renderer>();
            if (newvalue == -1)
            {
                r.material = materials[materials.Count - 1];
            } else
            {
                r.material = materials[newvalue];
            }
        }
    }
}
