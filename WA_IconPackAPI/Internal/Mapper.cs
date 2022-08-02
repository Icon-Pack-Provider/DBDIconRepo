using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPackAPI.Internal
{
    internal class GenericMapper<T> : ClassMap<IBasic> where T : IBasic
    {
        public GenericMapper()
        {
            Map(m => m.File).Name("File");
            Map(m => m.Name).Name("Name");
        }
    }

    internal class GenericWithFolderMapper<T> : ClassMap<T> where T : IBasic, IFolder
    {
        public GenericWithFolderMapper()
        {
            Map(m => m.Folder).Name("Folder");
            Map(m => m.File).Name("File");
            Map(m => m.Name).Name("Name");
        }
    }

    internal class PowerMapper : ClassMap<Model.Power>
    {
        public PowerMapper()
        {
            Map(m => m.Folder).Name("Folder");
            Map(m => m.File).Name("File");
            Map(m => m.Name).Name("Name");
            Map(m => m.Owner).Name("Owner");
        }
    }

    internal class AddonMapper : ClassMap<Model.Addon>
    {
        public AddonMapper()
        {
            Map(m => m.Folder).Name("Folder");
            Map(m => m.File).Name("File");
            Map(m => m.Name).Name("Name");
            Map(m => m.For).Name("For");
            Map(m => m.Owner).Name("Owner");
        }
    }
    internal class PerkMapper : ClassMap<Model.Perk>
    {
        public PerkMapper()
        {
            Map(m => m.Folder).Name("Folder");
            Map(m => m.File).Name("File");
            Map(m => m.Name).Name("Name");
            Map(m => m.Owner).Name("Owner");
        }
    }

}
