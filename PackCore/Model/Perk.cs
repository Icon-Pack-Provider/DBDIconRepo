using IconPack.Helper;

namespace IconPack.Model
{
    public partial class Perk : Observable, IBasic
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

        string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        string owner;
        public string Owner
        {
            get => owner;
            set => Set(ref owner, value);
        }
    }
}
