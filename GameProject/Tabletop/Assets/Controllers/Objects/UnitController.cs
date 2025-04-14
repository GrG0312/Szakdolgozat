using Model.UnityDependant;
using UnityEngine;
using System;
using Controllers.Objects.Game.Billboard;
using Model;
using Unity.Netcode;
using Unity.Collections;
using Model.Units;
using System.Collections.Generic;
using TMPro;

namespace Controllers.Objects
{
    public class UnitController : NetworkBehaviour
    {
        [SerializeField] private UnitModel model;
        [SerializeField] private UnitBillboard billboard;
        [SerializeField] private SpriteRenderer userIndicator;
        [SerializeField] private SpriteRenderer selectedIndicator;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject waypointMarkerPrefab;
        [SerializeField] private float LineWidth = 1;
        [SerializeField] private float pointSize = 2;
        [SerializeField] private Color destroyedColor;
        [SerializeField] private GameObject popupPrefab;

        private NetworkVariable<FixedString32Bytes> ColorNetworkVar = new NetworkVariable<FixedString32Bytes>();
        private NetworkVariable<int> IdentityNetworkVar = new NetworkVariable<int>(-1);
        private NetworkVariable<bool> SelectedNetworkVar = new NetworkVariable<bool>(false);
        private NetworkList<Vector3> WaypointsNetworkVar = new NetworkList<Vector3>();
        private NetworkVariable<bool> IsDestroyedNetVar = new NetworkVariable<bool>(false);

        private List<GameObject> waypointMarkers = new List<GameObject>();
        private Color usedColor;

        #region Unity messages

        private void Awake()
        {
            selectedIndicator.gameObject.SetActive(false);

            model.SetupFinished += Model_SetupFinished;
            model.Selected += Model_Selection;
            model.Moving += Model_Moving;
            model.UnitDestroyed += Model_UnitDestroyed;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ColorNetworkVar.OnValueChanged += OnColorNetworkValueChanged;
            IdentityNetworkVar.OnValueChanged += OnIdentityNetworkValueChanged;
            SelectedNetworkVar.OnValueChanged += OnSelectedNetworkValueChanged;
            WaypointsNetworkVar.OnListChanged += WaypointsNetworkValueChanged;
            IsDestroyedNetVar.OnValueChanged += IsDestroyedValueChanged;
        }

        #endregion

        #region Network Variable event handlers
        private void OnIdentityNetworkValueChanged(int oldvalue, int newvalue)
        {
            billboard.SetupVisuals(Defines.UnitVisuals[(UnitIdentifier)newvalue]);
        }
        private void OnColorNetworkValueChanged(FixedString32Bytes oldvalue, FixedString32Bytes newvalue)
        {
            ColorUtility.TryParseHtmlString(newvalue.ToString(), out Color c);
            usedColor = c;
            userIndicator.color = usedColor;
            selectedIndicator.color = usedColor;

            lineRenderer.startColor = usedColor;
            lineRenderer.endColor = usedColor;
        }
        private void OnSelectedNetworkValueChanged(bool oldvalue, bool newvalue)
        {
            selectedIndicator.gameObject.SetActive(newvalue);
        }

        private void IsDestroyedValueChanged(bool oldvalue, bool newvalue)
        {
            if (newvalue)
            {
                SpriteRenderer renderer = billboard.GetComponent<SpriteRenderer>();
                renderer.color = destroyedColor;
            }
        }
        #endregion

        #region Model event handlers

        private void Model_SetupFinished(object sender, EventArgs e)
        {
            ColorNetworkVar.Value = GameController.Instance.UserColors[model.Owner];
            IdentityNetworkVar.Value = (int)model.Identity;

            lineRenderer.startWidth = LineWidth;
            lineRenderer.endWidth = LineWidth;
            lineRenderer.positionCount = 0;
        }

        private void Model_Selection(object sender, bool e)
        {
            SelectedNetworkVar.Value = e;
        }

        private void Model_Moving(object sender, bool isMoving)
        {
            if (isMoving)
            {
                foreach (Vector3 wp in model.NavAgent.path.corners)
                {
                    WaypointsNetworkVar.Add(wp);
                }
            } else
            {
                WaypointsNetworkVar.Clear();
            }
        }

        private void Model_UnitDestroyed(object sender, EventArgs e)
        {
            IsDestroyedNetVar.Value = true;
        }

        #endregion

        #region Path Visualization
        private void WaypointsNetworkValueChanged(NetworkListEvent<Vector3> changeEvent)
        {
            VisualizePath();
        }
        private void VisualizePath()
        {
            lineRenderer.positionCount = WaypointsNetworkVar.Count;
            for (int i = 0; i < WaypointsNetworkVar.Count; i++)
            {
                lineRenderer.SetPosition(i, WaypointsNetworkVar[i]);
            }

            ClearWaypointMarkers();
            CreatePointMarkers();

            for (int i = 0; i < WaypointsNetworkVar.Count; i++)
            {
                waypointMarkers[i].transform.position = WaypointsNetworkVar[i];
            }
        }

        private void ClearWaypointMarkers()
        {
            foreach (GameObject wp in waypointMarkers)
            {
                Destroy(wp);
            }
            waypointMarkers.Clear();
        }

        private void CreatePointMarkers()
        {
            for (int i = 0; i < WaypointsNetworkVar.Count; i++)
            {
                GameObject marker = Instantiate(waypointMarkerPrefab);
                marker.transform.position = WaypointsNetworkVar[i];
                marker.transform.localScale = Vector3.one * pointSize;
                marker.GetComponent<Renderer>().material.color = usedColor;

                waypointMarkers.Add(marker);
            }
        }

        private void ClearPath()
        {
            lineRenderer.positionCount = 0;
            ClearWaypointMarkers();
        }
        #endregion
    }
}
