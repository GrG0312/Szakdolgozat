using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Model.UnityDependant
{
    public class DiceRoller : MonoBehaviour, IDiceRoller
    {
        #region Constants

        private const int THROW_FORCE = 25;
        private const int TORQUE_FORCE = 17;
        private const int DELAY_MILISECONDS = 5000;

        // The values of the faces paired with the directions they are facing on the prefab's model
        private static readonly Dictionary<Vector3, int> faceDirectionValues = new Dictionary<Vector3, int>
        {
            { Vector3.up, 2 },      // Face 2 : ( 0, 1, 0)
            { Vector3.down, 5 },    // Face 5 : ( 0,-1, 0)
            { Vector3.right, 4 },   // Face 4 : ( 1, 0, 0)
            { Vector3.left, 3 },    // Face 3 : (-1, 0, 0)
            { Vector3.forward, 1 }, // Face 1 : ( 0, 0, 1)
            { Vector3.back, 6 }     // Face 6 : ( 0, 0,-1)
        };

        #endregion

        #region Serializations
        [SerializeField] private GameObject dicePrefab;
        [SerializeField] private Transform diceSpawnPoint;
        #endregion

        private List<GameObject> diceList = new List<GameObject>();

        public List<Action<int[]>> Listeneres = new List<Action<int[]>>();

        #region Rolling
        public async Task<int[]> RollDice(int diceCount)
        {
            int[] values = new int[diceCount];
            if (diceCount == 0)
            {
                NotifyListeners(values);
                return values;
            }

            for (int i = 0; i < diceCount; i++)
            {

                GameObject dice = Instantiate(dicePrefab, diceSpawnPoint.position, UnityEngine.Random.rotation);
                dice.GetComponent<NetworkObject>().Spawn(true);
                Rigidbody rb = dice.GetComponent<Rigidbody>();

                // Set the throw direction to be downwards, with an appr. +- 35 degrees possible difference
                Vector3 throwDir = Vector3.down + 
                    new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f), 0, UnityEngine.Random.Range(-1, 1)).normalized;

                rb.AddForce(throwDir * THROW_FORCE, ForceMode.Impulse);
                rb.AddTorque(UnityEngine.Random.onUnitSphere * TORQUE_FORCE, ForceMode.Impulse);

                diceList.Add(dice);
                // Have some delay between spawning D6s, becasue they would go boombayah on eachother and cause some very strange behaviour
                await Task.Delay(100);
            }

            // Wait for the dices to settle
            bool allStopped;
            do
            {
                // Yield so that this calculation only continues on the next frame
                // Important to use Yield instead of Delay because here it would block physics updates
                await Task.Yield();
                allStopped = true;

                foreach (GameObject dice in diceList)
                {
                    Rigidbody rb = dice.GetComponent<Rigidbody>();
                    if (rb.linearVelocity.sqrMagnitude > 0.01f)
                    {
                        allStopped = false;
                        break;
                    }
                }
            }
            while (!allStopped);

            // For some visual delay to process the results
            await Task.Delay(DELAY_MILISECONDS);

            // Calculate results
            for (int i = 0; i < diceCount; i++)
            {
                values[i] = GetDiceValue(diceList[i]);
                diceList[i].GetComponent<NetworkObject>().Despawn(true);
            }
            diceList.Clear();

            NotifyListeners(values);
            return values;
        }

        private int GetDiceValue(GameObject dice)
        {
            // I didnt expect to use this word ever in my life
            float[] upwardsnessValues = new float[faceDirectionValues.Count];

            for (int i = 0; i < faceDirectionValues.Count; i++)
            {
                Vector3 faceDirection = faceDirectionValues.ElementAt(i).Key;

                // A Vector3 that tells which way does the side of the cube face in world space
                Vector3 worldDir = dice.transform.TransformDirection(faceDirection);

                // This is such a neat tool, I love this
                // Creates a float value based on how much does the side's world direction matches the upward direction
                upwardsnessValues[i] = Vector3.Dot(worldDir, Vector3.up);
            }

            // A simple max-search to find the "most upwards value"
            int maxIndex = 0;
            float maxValue = upwardsnessValues[0];

            for (int i = 1; i < upwardsnessValues.Length; i++)
            {
                if (upwardsnessValues[i] > maxValue)
                {
                    maxValue = upwardsnessValues[i];
                    maxIndex = i;
                }
            }

            return faceDirectionValues.ElementAt(maxIndex).Value;
        }

        private void NotifyListeners(int[] results)
        {
            foreach (Action<int[]> listener in Listeneres)
            {
                listener(results);
            }
        }
        #endregion
    }
}
