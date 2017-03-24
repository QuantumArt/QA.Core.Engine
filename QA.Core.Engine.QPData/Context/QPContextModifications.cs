
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;


namespace QA.Core.Engine.QPData 
{


public partial class QPContext
{


    
    private Dictionary<string,string> PackQPAbstractItem(QPAbstractItem instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Title != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Title")).Name] = instance.Title; }

        if (instance.Name != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Name")).Name] = instance.Name; }

        if (instance.Parent_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Parent")).Name] = instance.Parent_ID.ToString(); }

        if (instance.IsVisible != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "IsVisible")).Name] = instance.IsVisible.Value ? "1" : "0"; }

        if (instance.IsPage != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "IsPage")).Name] = instance.IsPage.Value ? "1" : "0"; }

        if (instance.Regions != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Regions")).Name] = instance.RegionsString; }

        if (instance.ZoneName != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "ZoneName")).Name] = instance.ZoneName; }

        if (instance.AllowedUrlPatterns != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "AllowedUrlPatterns")).Name] = instance.AllowedUrlPatterns; }

        if (instance.DeniedUrlPatterns != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "DeniedUrlPatterns")).Name] = instance.DeniedUrlPatterns; }

        if (instance.Description != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Description")).Name] = instance.Description; }

        if (instance.Discriminator_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Discriminator")).Name] = instance.Discriminator_ID.ToString(); }

        if (instance.ContentId != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "ContentId")).Name] = instance.ContentId.ToString(); }

        if (instance.VersionOf_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "VersionOf")).Name] = instance.VersionOf_ID.ToString(); }

        if (instance.Culture_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Culture")).Name] = instance.Culture_ID.ToString(); }

        if (instance.TitleFormat_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "TitleFormat")).Name] = instance.TitleFormat_ID.ToString(); }

        if (instance.Keywords != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Keywords")).Name] = instance.Keywords; }

        if (instance.MetaDescription != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "MetaDescription")).Name] = instance.MetaDescription; }

        if (instance.Tags != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Tags")).Name] = instance.Tags; }

        if (instance.IsInSiteMap != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "IsInSiteMap")).Name] = instance.IsInSiteMap.Value ? "1" : "0"; }

        if (instance.IndexOrder != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "IndexOrder")).Name] = instance.IndexOrder.ToString(); }

        if (instance.ExtensionId != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "ExtensionId")).Name] = instance.ExtensionId.ToString(); }

        if (instance.AuthenticationTargeting != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "AuthenticationTargeting")).Name] = instance.AuthenticationTargeting; }

        if (instance.Targeting != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPAbstractItem", "Targeting")).Name] = instance.Targeting; }

        return values;
    }


    partial void InsertQPAbstractItem(QPAbstractItem instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPAbstractItem(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPAbstractItem"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateQPAbstractItem(QPAbstractItem instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPAbstractItem(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPAbstractItem"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteQPAbstractItem(QPAbstractItem instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 
    private Dictionary<string,string> PackQPDiscriminator(QPDiscriminator instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Title != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "Title")).Name] = instance.Title; }

        if (instance.Name != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "Name")).Name] = instance.Name; }

        if (instance.PreferredContentId != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "PreferredContentId")).Name] = instance.PreferredContentId.ToString(); }

        if (instance.CategoryName != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "CategoryName")).Name] = instance.CategoryName; }

        if (instance.Description != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "Description")).Name] = instance.Description; }

        if (instance.IconUrl != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "IconUrl")).Name] = instance.IconUrl; }

        if (instance.IsPage != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "IsPage")).Name] = instance.IsPage.Value ? "1" : "0"; }

        if (instance.AllowedZones != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "AllowedZones")).Name] = instance.AllowedZones; }

        if (instance.AllowedItemDefinitions1 != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "AllowedItemDefinitions1")).Name] = instance.AllowedItemDefinitions1String; }

        if (instance.FilterPartByUrl != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPDiscriminator", "FilterPartByUrl")).Name] = instance.FilterPartByUrl.Value ? "1" : "0"; }

        return values;
    }


    partial void InsertQPDiscriminator(QPDiscriminator instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPDiscriminator(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPDiscriminator"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateQPDiscriminator(QPDiscriminator instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPDiscriminator(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPDiscriminator"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteQPDiscriminator(QPDiscriminator instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 
    private Dictionary<string,string> PackQPCulture(QPCulture instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Enabled != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "Enabled")).Name] = instance.Enabled.Value ? "1" : "0"; }

        if (instance.Title != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "Title")).Name] = instance.Title; }

        if (instance.Name != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "Name")).Name] = instance.Name; }

        if (instance.Icon != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "Icon")).Name] = instance.Icon; }

        if (instance.SortOrder != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "SortOrder")).Name] = instance.SortOrder.ToString(); }

        if (instance.ChangeCultureLabel != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "ChangeCultureLabel")).Name] = instance.ChangeCultureLabel; }

        if (instance.CultureSelectionTitle != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPCulture", "CultureSelectionTitle")).Name] = instance.CultureSelectionTitle; }

        return values;
    }


    partial void InsertQPCulture(QPCulture instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPCulture(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPCulture"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateQPCulture(QPCulture instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPCulture(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPCulture"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteQPCulture(QPCulture instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 
    private Dictionary<string,string> PackItemTitleFormat(ItemTitleFormat instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Value != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "ItemTitleFormat", "Value")).Name] = instance.Value; }

        if (instance.Description != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "ItemTitleFormat", "Description")).Name] = instance.Description; }

        return values;
    }


    partial void InsertItemTitleFormat(ItemTitleFormat instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackItemTitleFormat(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "ItemTitleFormat"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateItemTitleFormat(ItemTitleFormat instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackItemTitleFormat(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "ItemTitleFormat"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteItemTitleFormat(ItemTitleFormat instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 

    partial void InsertQPRegion(QPRegion instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }

    partial void UpdateQPRegion(QPRegion instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }

    partial void DeleteQPRegion(QPRegion instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }
 

    partial void InsertTrailedAbstractItem(TrailedAbstractItem instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }

    partial void UpdateTrailedAbstractItem(TrailedAbstractItem instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }

    partial void DeleteTrailedAbstractItem(TrailedAbstractItem instance)
    {

        throw new InvalidOperationException(@"Virtual Contents cannot be modified");

    }
 
    private Dictionary<string,string> PackQPObsoleteUrl(QPObsoleteUrl instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Url != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPObsoleteUrl", "Url")).Name] = instance.Url; }

        if (instance.AbstractItem_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPObsoleteUrl", "AbstractItem")).Name] = instance.AbstractItem_ID.ToString(); }

        return values;
    }


    partial void InsertQPObsoleteUrl(QPObsoleteUrl instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPObsoleteUrl(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPObsoleteUrl"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateQPObsoleteUrl(QPObsoleteUrl instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPObsoleteUrl(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPObsoleteUrl"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteQPObsoleteUrl(QPObsoleteUrl instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 
    private Dictionary<string,string> PackQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
    {
        var values = GetInitialValues(instance);

        if (instance.Source_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPItemDefinitionConstraint", "Source")).Name] = instance.Source_ID.ToString(); }

        if (instance.Target_ID != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPItemDefinitionConstraint", "Target")).Name] = instance.Target_ID.ToString(); }

        if (instance.Title != null)  { values[Cnn.GetContentAttributeObject(Cnn.GetAttributeIdByNetNames(SiteId, "QPItemDefinitionConstraint", "Title")).Name] = instance.Title; }

        return values;
    }


    partial void InsertQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPItemDefinitionConstraint(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPItemDefinitionConstraint"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Id = Int32.Parse(values[SystemColumnNames.Id]);
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);
      instance.Created = instance.Modified;

    }

    partial void UpdateQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
    {

        Cnn.ExternalTransaction = Transaction;
        var values = PackQPItemDefinitionConstraint(instance);
        Cnn.MassUpdate(Cnn.GetContentIdByNetName(SiteId, "QPItemDefinitionConstraint"), new List<Dictionary<string, string>>() { values }, Cnn.LastModifiedBy
          , new MassUpdateOptions() { ReplaceUrls = true});
      instance.Modified = DateTime.Parse(values[SystemColumnNames.Modified], CultureInfo.InvariantCulture);

    }

    partial void DeleteQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
    {

        Cnn.ExternalTransaction = Transaction;
        Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));

    }
 

    partial void InsertAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
    {
    }

    partial void UpdateAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
    {
    }

    partial void DeleteAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
    {
    }


    partial void InsertItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
    {
    }

    partial void UpdateItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
    {
    }

    partial void DeleteItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
    {
    }


    partial void InsertStatusType(StatusType instance)
    {
    }

    partial void UpdateStatusType(StatusType instance)
    {
    }

    partial void DeleteStatusType(StatusType instance)
    {
    }
}


}
