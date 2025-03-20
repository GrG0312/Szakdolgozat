using System.Collections.ObjectModel;
using System;

namespace Model.Lobby
{
    public class SlotOccupantStatus
    {
        private readonly string toString;
        private SlotOccupantStatus[] requirements;
        public ReadOnlyCollection<SlotOccupantStatus> RequiredStatuses { get; }
        private SlotOccupantStatus(SlotOccupantStatus[] requirements, string toStringForm)
        {
            toString = toStringForm;
            this.requirements = requirements;
            RequiredStatuses = new ReadOnlyCollection<SlotOccupantStatus>(this.requirements);
        }

        public override string ToString()
        {
            return toString;
        }

        public static SlotOccupantStatus ConvertFromString(string name)
        {
            switch (name)
            {
                case "OpenModel":
                    return OpenModel;
                case "ClosedModel":
                    return ClosedModel;
                case "ReservedModel":
                    return ReservedModel;
                case "OccupiedModel":
                    return OccupiedModel;
                default:
                    throw new ArgumentException("Invalid parameter provided!");
            }
        }

        public static SlotOccupantStatus OpenModel = new SlotOccupantStatus(
            new SlotOccupantStatus[]
                {
                    ClosedModel,
                    OccupiedModel
                },
            nameof(OpenModel)
            );
        public static SlotOccupantStatus ClosedModel = new SlotOccupantStatus(
            new SlotOccupantStatus[]
                {
                    OpenModel,
                    OccupiedModel
                },
            nameof(ClosedModel)
            );
        public static SlotOccupantStatus ReservedModel = new SlotOccupantStatus(
            new SlotOccupantStatus[]
                {
                    OpenModel
                },
            nameof(ReservedModel)
            );
        public static SlotOccupantStatus OccupiedModel = new SlotOccupantStatus(
            new SlotOccupantStatus[]
                {
                    ReservedModel
                },
            nameof(OccupiedModel)
            );
    }
}
