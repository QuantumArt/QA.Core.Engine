
using Quantumart.QPublishing.Database;
using System.Collections;
using System.Web;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;


namespace QA.Core.Engine.QPData 
{


public partial class QPContext
{


    
	private Hashtable PackQPAbstractItem(QPAbstractItem instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Title != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Title"), ReplaceUrls(instance.Title)); }

   if (instance.Name != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Name"), ReplaceUrls(instance.Name)); }

   if (instance.Parent_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Parent"), instance.Parent_ID.ToString()); }

   if (instance.IsVisible != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "IsVisible"), ((bool)instance.IsVisible) ? "1" : "0"); }

   if (instance.IsPage != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "IsPage"), ((bool)instance.IsPage) ? "1" : "0"); }

   if (instance.ZoneName != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "ZoneName"), ReplaceUrls(instance.ZoneName)); }

   if (instance.AllowedUrlPatterns != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "AllowedUrlPatterns"), ReplaceUrls(instance.AllowedUrlPatterns)); }

   if (instance.DeniedUrlPatterns != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "DeniedUrlPatterns"), ReplaceUrls(instance.DeniedUrlPatterns)); }

   if (instance.Description != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Description"), ReplaceUrls(instance.Description)); }

   if (instance.Discriminator_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Discriminator"), instance.Discriminator_ID.ToString()); }

   if (instance.ContentId != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "ContentId"), instance.ContentId.ToString()); }

   if (instance.VersionOf_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "VersionOf"), instance.VersionOf_ID.ToString()); }

   if (instance.Culture_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Culture"), instance.Culture_ID.ToString()); }

   if (instance.TitleFormat_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "TitleFormat"), instance.TitleFormat_ID.ToString()); }

   if (instance.Keywords != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Keywords"), ReplaceUrls(instance.Keywords)); }

   if (instance.MetaDescription != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "MetaDescription"), ReplaceUrls(instance.MetaDescription)); }

   if (instance.Tags != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "Tags"), ReplaceUrls(instance.Tags)); }

   if (instance.IsInSiteMap != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "IsInSiteMap"), ((bool)instance.IsInSiteMap) ? "1" : "0"); }

   if (instance.IndexOrder != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "IndexOrder"), instance.IndexOrder.ToString()); }

   if (instance.ExtensionId != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPAbstractItem", "ExtensionId"), instance.ExtensionId.ToString()); }

		return Values;
	}
	

	partial void InsertQPAbstractItem(QPAbstractItem instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPAbstractItem(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPAbstractItem"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateQPAbstractItem(QPAbstractItem instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPAbstractItem(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPAbstractItem"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
	}			

	partial void DeleteQPAbstractItem(QPAbstractItem instance)
	{
		
		Cnn.ExternalTransaction = Transaction;	
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
				
	}
 
	private Hashtable PackQPDiscriminator(QPDiscriminator instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Title != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "Title"), ReplaceUrls(instance.Title)); }

   if (instance.Name != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "Name"), ReplaceUrls(instance.Name)); }

   if (instance.PreferredContentId != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "PreferredContentId"), instance.PreferredContentId.ToString()); }

   if (instance.CategoryName != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "CategoryName"), ReplaceUrls(instance.CategoryName)); }

   if (instance.Description != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "Description"), ReplaceUrls(instance.Description)); }

   if (instance.IconUrl != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "IconUrl"), instance.IconUrl); }

   if (instance.IsPage != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "IsPage"), ((bool)instance.IsPage) ? "1" : "0"); }

   if (instance.AllowedZones != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "AllowedZones"), ReplaceUrls(instance.AllowedZones)); }

   if (instance.FilterPartByUrl != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPDiscriminator", "FilterPartByUrl"), ((bool)instance.FilterPartByUrl) ? "1" : "0"); }

		return Values;
	}
	

	partial void InsertQPDiscriminator(QPDiscriminator instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPDiscriminator(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPDiscriminator"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateQPDiscriminator(QPDiscriminator instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPDiscriminator(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPDiscriminator"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
	}			

	partial void DeleteQPDiscriminator(QPDiscriminator instance)
	{
		
		Cnn.ExternalTransaction = Transaction;	
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
				
	}
 
	private Hashtable PackQPCulture(QPCulture instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Title != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPCulture", "Title"), ReplaceUrls(instance.Title)); }

   if (instance.Name != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPCulture", "Name"), ReplaceUrls(instance.Name)); }

   if (instance.Icon != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPCulture", "Icon"), instance.Icon); }

		return Values;
	}
	

	partial void InsertQPCulture(QPCulture instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPCulture(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPCulture"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateQPCulture(QPCulture instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPCulture(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPCulture"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
	}			

	partial void DeleteQPCulture(QPCulture instance)
	{
		
		Cnn.ExternalTransaction = Transaction;	
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
				
	}
 
	private Hashtable PackItemTitleFormat(ItemTitleFormat instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Value != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "ItemTitleFormat", "Value"), ReplaceUrls(instance.Value)); }

   if (instance.Description != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "ItemTitleFormat", "Description"), ReplaceUrls(instance.Description)); }

		return Values;
	}
	

	partial void InsertItemTitleFormat(ItemTitleFormat instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackItemTitleFormat(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "ItemTitleFormat"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateItemTitleFormat(ItemTitleFormat instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackItemTitleFormat(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "ItemTitleFormat"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
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
 
	private Hashtable PackQPObsoleteUrl(QPObsoleteUrl instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Url != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPObsoleteUrl", "Url"), ReplaceUrls(instance.Url)); }

   if (instance.AbstractItem_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPObsoleteUrl", "AbstractItem"), instance.AbstractItem_ID.ToString()); }

		return Values;
	}
	

	partial void InsertQPObsoleteUrl(QPObsoleteUrl instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPObsoleteUrl(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPObsoleteUrl"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateQPObsoleteUrl(QPObsoleteUrl instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPObsoleteUrl(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPObsoleteUrl"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
	}			

	partial void DeleteQPObsoleteUrl(QPObsoleteUrl instance)
	{
		
		Cnn.ExternalTransaction = Transaction;	
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
				
	}
 
	private Hashtable PackQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
	{
		Hashtable Values = new Hashtable();
		
   if (instance.Source_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPItemDefinitionConstraint", "Source"), instance.Source_ID.ToString()); }

   if (instance.Target_ID != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPItemDefinitionConstraint", "Target"), instance.Target_ID.ToString()); }

   if (instance.Title != null)  { Values.Add(Cnn.GetFormNameByNetNames(SiteId, "QPItemDefinitionConstraint", "Title"), ReplaceUrls(instance.Title)); }

		return Values;
	}
	

	partial void InsertQPItemDefinitionConstraint(QPItemDefinitionConstraint instance) 
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPItemDefinitionConstraint(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime created = DateTime.Now;
		instance.LoadStatusType();
		instance.Id = Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPItemDefinitionConstraint"), instance.StatusType.Name, ref Values, ref cl, 0, true, 0, instance.Visible, instance.Archive, true, ref created);
		instance.Created = created;
		instance.Modified = created;
            
	}

	partial void UpdateQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
	{	
		
		Cnn.ExternalTransaction = Transaction;
		Hashtable Values = PackQPItemDefinitionConstraint(instance);
		HttpFileCollection cl = (HttpFileCollection)null;
		DateTime modified = DateTime.Now;
		Cnn.AddFormToContent(SiteId, Cnn.GetContentIdByNetName(SiteId, "QPItemDefinitionConstraint"), instance.StatusType.Name, ref Values, ref cl, (int)instance.Id, true, 0, instance.Visible, instance.Archive, true, ref modified);
		instance.Modified = modified;
            		
	}			

	partial void DeleteQPItemDefinitionConstraint(QPItemDefinitionConstraint instance)
	{
		
		Cnn.ExternalTransaction = Transaction;	
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'delete from content_item where content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {0}", instance.Id.ToString()));
				
	}
 

	partial void InsertAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
	{
		Cnn.ExternalTransaction = Transaction;
		int linkId = Cnn.GetLinkIdByNetName(SiteId, "AbstractItemAbtractItemRegionArticle");
		
		if (linkId == 0)
			throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "AbstractItemAbtractItemRegionArticle", SiteId));
			
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'if not exists(select * from item_link where link_id = @linkId and item_id = @itemId and linked_item_id = @linkedItemId) insert into item_to_item values(@linkId, @itemId, @linkedItemId)', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", linkId, instance.ITEM_ID, instance.LINKED_ITEM_ID));
	}

	partial void UpdateAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
	{
	
	}

	partial void DeleteAbstractItemAbtractItemRegionArticle(AbstractItemAbtractItemRegionArticle instance)
	{
		Cnn.ExternalTransaction = Transaction;
		Cnn.ProcessData(instance.RemovingInstruction);
	}
    

	partial void InsertItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
	{
		Cnn.ExternalTransaction = Transaction;
		int linkId = Cnn.GetLinkIdByNetName(SiteId, "ItemDefinitionItemDefinitionArticle");
		
		if (linkId == 0)
			throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "ItemDefinitionItemDefinitionArticle", SiteId));
			
		Cnn.ProcessData(String.Format("EXEC sp_executesql N'if not exists(select * from item_link where link_id = @linkId and item_id = @itemId and linked_item_id = @linkedItemId) insert into item_to_item values(@linkId, @itemId, @linkedItemId)', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", linkId, instance.ITEM_ID, instance.LINKED_ITEM_ID));
	}

	partial void UpdateItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
	{
	
	}

	partial void DeleteItemDefinitionItemDefinitionArticle(ItemDefinitionItemDefinitionArticle instance)
	{
		Cnn.ExternalTransaction = Transaction;
		Cnn.ProcessData(instance.RemovingInstruction);
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
