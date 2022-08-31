using IconPack.Helper;
using IconPack.Internal;

namespace IconPack.Model.Icon
{
    public partial class Offering : Observable, IBasic, IFolder
    {
#nullable enable
        string? folder;
        public string? Folder
        {
            get => folder;
            set => Set(ref folder, value);
        }
#nullable disable

        string file;
        public string File
        {
            get => file;
            set => Set(ref file, value);
        }

        //Power name
        string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

    }
}
