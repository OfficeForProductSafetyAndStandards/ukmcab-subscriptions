﻿using MoreLinq;
using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Core.Domain;

/// <summary>
/// The frequency of email notifications on a given subscription
/// </summary>
public enum SubscriptionType { Search, Cab }


public class SearchResultChange
{
    public Guid CabId => Old.CabId;
    public SubscriptionsCoreCabSearchResultModel Old { get; set; }
    public SubscriptionsCoreCabSearchResultModel New { get; set; }

    public SearchResultChange(SubscriptionsCoreCabSearchResultModel old, SubscriptionsCoreCabSearchResultModel @new)
    {
        Old = old;
        New = @new;
    }
}

public class SearchResultsChangesModel
{
    public List<SubscriptionsCoreCabSearchResultModel> Added { get; set; } = new();
    public List<SubscriptionsCoreCabSearchResultModel> Removed { get; set; } = new();
    public List<SearchResultChange> Modified { get; set; } = new();

    public SearchResultsChangesModel(List<SubscriptionsCoreCabSearchResultModel> old, List<SubscriptionsCoreCabSearchResultModel> @new)
    {
        Added = @new.ExceptBy(old, x => x.CabId).ToList();
        Removed = @old.ExceptBy(@new, x => x.CabId).ToList();
        Modified = @new.Join(old, x => x.CabId, x => x.CabId, (n, o) => new { ncab = n, ocab = o }).Where(x => x.ocab.Name != x.ncab.Name).Select(x => new SearchResultChange(x.ocab, x.ncab)).ToList();
    }

    public SearchResultsChangesModel() { }

    public override string ToString() => $"{Added.Count} added, {Removed.Count} removed, {Modified.Count} modified.";
}