using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.Model.Uploadable;

public partial class UploadableFolder : ObservableObject, IUploadableItem
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string displayName = "";

    [ObservableProperty]
    private bool? isSelected = true;

    [ObservableProperty]
    private bool isExpand = true;

    [RelayCommand] private void ToggleIsSelected() => IsSelected = !IsSelected;

    public string SubFolderDisplay
        => $"{(Parent is not null ? "\\" : "")}{(Parent is not null ? Parent.Name : "")}";

    partial void OnIsSelectedChanged(bool? value)
    {
        if (Parent is not null)
            Parent.NotifyChildSelectionChanged();
        //Update child to match
        if (value is null)
            return;
        foreach (var child in SubItems)
        {
            child.IsSelected = value;
        }
    }

    [ObservableProperty]
    private ObservableCollection<IUploadableItem> subItems = new();

    UploadableFolder? parent = null;
    public UploadableFolder? Parent
    {
        get => parent;
        private set => parent = value;
    }

    public UploadableFolder(UploadableFolder? root = null)
    {
        this.parent = root;
    }

    internal void NotifyChildSelectionChanged()
    {
        var bools = SubItems.Select(i => i.IsSelected).Distinct().ToList();
        if (bools.Count <= 1)
            IsSelected = bools[0];
        else
            IsSelected = null;
    }
}