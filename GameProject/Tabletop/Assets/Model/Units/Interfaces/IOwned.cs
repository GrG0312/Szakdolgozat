using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Units.Interfaces
{
    public interface IOwned<IdType>
    {
        public IdType Owner { get; }
    }
}
