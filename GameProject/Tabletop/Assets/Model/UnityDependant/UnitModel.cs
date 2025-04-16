using Model.GameModel;
using Model.Interfaces;
using Model.Units;
using Model.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Model.UnityDependant
{
    public class UnitModel : NetworkBehaviour, 
        IUnit, ISelectable<ulong>, IMovable<Vector3>, IDamageable<Vector3>, IArmored, IUsable, IWeaponUser<Vector3>
    {
        #region Serializations
        [SerializeField] private LayerMask navigableLayer;
        [SerializeField] private LayerMask ignore;
        #endregion

        #region IWeaponUser

        public IReadOnlyList<UsableWeapon> UsableWeapons { get; private set; }

        public bool CanTarget(IDamageable<Vector3> d)
        {
            if (d.Side == this.Side)
            {
                return false;
            }
            if (d is IOwned<ulong> o && o.Owner == this.Owner)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region IUsable
        public bool IsUsable(Phase where)
        {
            if (where == Phase.Movement)
            {
                return CanMove;
            } else if(where == Phase.Fighting)
            {
                return UsableWeapons.Any(w => w.IsUsable(where));
            }
            return false;
        }
        #endregion

        #region IDamagable

        public event EventHandler? UnitDestroyed;
        public event EventHandler<int>? DamageTaken;

        public bool Alive { get; protected set; }
        public int CurrentHP { get; protected set; }

        public void TakeDamage(int amount)
        {
            CurrentHP = Math.Max(CurrentHP - amount, 0);
            DamageTaken?.Invoke(this, amount);
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Alive = false;
            // Don't destroy the corpse right away, they will be destroyed at the end of the turn
            UnitDestroyed?.Invoke(this, EventArgs.Empty);
            this.gameObject.layer = ignore;
        }

        public void Delete()
        {
            Destroy(this.gameObject);
        }

        #endregion

        #region IArmored

        public async Task<int> ArmorSave(int amount, int armorPiercing, IDiceRoller roller)
        {
            int[] results = await roller.RollDice(amount);

            int threshold = Constants.ArmorSave + armorPiercing;
            int evaded = results.Count(roll => roll > threshold);
            return evaded;
        }

        #endregion

        #region IMapObject
        public Vector3 Position
        {
            get { return transform.position; }
            set
            {
                transform.position = value;
            }
        }

        public float DistanceTo(IMapObject<Vector3> other)
        {
            return Vector3.Distance(this.Position, other.Position);
        }
        #endregion

        #region IMovable
        public NavMeshAgent NavAgent { get; private set; }

        public event EventHandler<bool>? Moving;

        public bool CanMove { get; set; }

        public void MoveTo(Vector3 moveDestination)
        {
            if (!Alive)
            {
                return;
            }
            // Store the full calculated path
            NavMeshPath fullPath = new NavMeshPath();
            if (!NavAgent.CalculatePath(moveDestination, fullPath))
            {
                return;
            }

            // Maximum distance is specified in Movement stat, adjusted by a specified number so it matches the map's scale 
            float remainingDistance = Constants.Movement * Defines.RANGE_ADJUSTMENT;
            List<Vector3> partialWaypoints = new List<Vector3> { fullPath.corners[0] };
            
            // Calculate how far we can go along the path
            for (int i = 0; i < fullPath.corners.Length - 1; i++)
            {
                Vector3 currentWaypoint = fullPath.corners[i];
                Vector3 nextWaypoint = fullPath.corners[i + 1];
                float segmentDistance = Vector3.Distance(currentWaypoint, nextWaypoint);

                if (segmentDistance <= remainingDistance)
                {
                    partialWaypoints.Add(nextWaypoint);
                    remainingDistance -= segmentDistance;
                }
                else
                {
                    // Calculate how far can we go along this part
                    Vector3 direction = (nextWaypoint - currentWaypoint).normalized;
                    Vector3 stopPoint = currentWaypoint + direction * remainingDistance;
                    partialWaypoints.Add(stopPoint);
                    break;
                }
            }

            // Create new path with the partial waypoints
            NavMeshPath partialPath = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, partialWaypoints[partialWaypoints.Count - 1], NavMesh.AllAreas, partialPath))
            {
                NavAgent.SetPath(partialPath); // why dont you have an event for stoppingg!!! man thats lame
                CanMove = false;
                Moving?.Invoke(this, true);
                StartCoroutine(DetectAgentStop());
            }
        }

        private IEnumerator DetectAgentStop()
        {
            yield return new WaitForSeconds(0.5f);
            while (NavAgent.velocity.magnitude >= 0.15f)
            {
                yield return null;
            }
            Moving?.Invoke(this, false);
        }
        #endregion

        #region ISelectable

        public event EventHandler<bool>? Selected;
        public void SetSelected(bool status)
        {
            Selected?.Invoke(this, status);
        }

        #endregion

        #region IOwned
        public ulong Owner
        {
            get { return OwnerClientId; }
            protected set
            {
                this.NetworkObject.ChangeOwnership(value);
            }
        }
        #endregion

        #region ISidedObject

        public Side Side { get => Constants.Side; }

        #endregion

        #region IUnit
        public UnitIdentifier Identity { get; protected set; }
        public UnitConstants Constants { get; protected set; }

        public void SetStartValues()
        {
            CanMove = true;
            foreach (UsableWeapon w in UsableWeapons)
            {
                w.CanDamage = true;
            }
        }
        #endregion

        #region Setup

        public event EventHandler? SetupFinished;

        public void SetupData(ulong owner, UnitIdentifier identity, UnitConstants basestat, Vector3 pos)
        {
            CanMove = true;
            Owner = owner;
            Identity = identity;
            Constants = basestat;
            Position = pos;
            CurrentHP = Constants.Wound;
            Alive = true;
            
            NavAgent = this.gameObject.AddComponent<NavMeshAgent>();
            NavAgent.speed = 60;
            NavAgent.acceleration = 600;
            NavAgent.angularSpeed = 480;
            // Set stopping distance so that the agents wont bash into eachother
            // This might reduce the actual path that they travel a little,
            //  but it will ensure that the agents avoid some unwanted behaviour
            NavAgent.stoppingDistance = 2;

            SetupFinished?.Invoke(this, EventArgs.Empty);
            List<UsableWeapon> ws = new();
            foreach (UnitWeapon w in Constants.Weapons)
            {
                ws.Add(new UsableWeapon(w));
            }
            UsableWeapons = ws;
        }
        #endregion
    }
}