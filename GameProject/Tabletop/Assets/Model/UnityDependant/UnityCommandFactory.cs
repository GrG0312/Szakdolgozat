using Model.GameModel.Commands;
using Model.Interfaces;
using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.UnityDependant
{
    public class UnityCommandFactory : ICommandFactory
    {
        private Dictionary<Type, Func<object[], IUnitCommand>> creationCommands;

        public UnityCommandFactory()
        {
            creationCommands = new Dictionary<Type, Func<object[], IUnitCommand>>();

            creationCommands[typeof(MoveCommand<Vector3>)]
                // MoveCommand: selectedUnit, {targetLocation}
                = args => new MoveCommand<Vector3>((IMovable<Vector3>)args[0], (Vector3)args[1]);

            creationCommands[typeof(AttackCommand<ulong>)]
                // AttackCommand : selectedUnit, {targetUnit}, roller
                = (args) => new AttackCommand<ulong>((IWeaponUser<ulong>)args[0], (IDamagable<ulong>)args[1], (IDiceRoller)args[2]);
        }

        public IUnitCommand Produce<T>(params object[] args) where T : IUnitCommand
        {
            if (creationCommands.TryGetValue(typeof(T), out Func<object[], IUnitCommand> func))
            {
                object[] arr = FlattenArray(args).ToArray(); // flatten the array becasue in the model we also get a params object[] args
                return func(arr);
            }
            throw new ArgumentException("Not supported command type found.");
        }

        private static IEnumerable<object> FlattenArray(IEnumerable<object> array)
        {
            foreach (var item in array)
            {
                if (item is IEnumerable<object> enumerable && !(item is string))
                {
                    foreach (var subItem in FlattenArray(enumerable))
                        yield return subItem;
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
