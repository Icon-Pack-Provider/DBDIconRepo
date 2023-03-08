using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DBDIconRepo.Model.Uploadable;

public partial class UploadableFile : ObservableObject, IUploadableItem
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string displayName = "";

    [ObservableProperty]
    private string filePath = "";

    [ObservableProperty]
    private bool? isSelected = true;

    [ObservableProperty]
    private bool isExpand = true;

    [RelayCommand] private void ToggleIsSelected() => IsSelected = !IsSelected;

    partial void OnIsSelectedChanged(bool? value)
    {
        if (Parent is not null)
        {
            //Notify parent
            Parent.NotifyChildSelectionChanged();
        }
    }

    UploadableFolder? parent = null;
    public UploadableFolder? Parent
    {
        get => parent;
        private set => parent = value;
    }

    public UploadableFile(UploadableFolder? root = null)
    {
        parent = root;
    }
}