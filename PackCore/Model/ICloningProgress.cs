using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPack.Model
{
    public interface ICloningProgress
    {
        double Percent { get; }
        string RepositoryName { get; }
    }
}
