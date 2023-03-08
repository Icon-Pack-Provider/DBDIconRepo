using DBDIconRepo.Service;
using System;
using System.Text.Json.Serialization;

namespace DBDIconRepo.Model.History;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "HistoryType")]
[JsonDerivedType(typeof(HistoryInstallPack), nameof(HistoryInstallPack))]
[JsonDerivedType(typeof(HistoryViewPack), nameof(HistoryViewPack))]
public interface IHistoryItem
{
    DateTime Time { get; set; }
    /// <summary>
    /// Repository ID
    /// </summary>
    long Victim { get; set; }
    HistoryType Action { get; set; }
}