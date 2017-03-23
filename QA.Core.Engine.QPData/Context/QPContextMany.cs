
using System.Data.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections;
using System.ComponentModel;

    
namespace QA.Core.Engine.QPData 
{

public partial class QPAbstractItem
{
	

	private ListSelector<AbstractItemAbtractItemRegionArticle, QPRegion> _Regions = null;

	public ListSelector<AbstractItemAbtractItemRegionArticle, QPRegion> Regions
	{
		get
		{
			if (_Regions == null)
			{
				_Regions = this.AbstractItemAbtractItemRegionArticles.GetNewBindingList()
				.AsListSelector<AbstractItemAbtractItemRegionArticle, QPRegion>
				(
					od => od.QPRegion,
					delegate(IList<AbstractItemAbtractItemRegionArticle> ods, QPRegion p)
					{
						var items = ods.Where(od => od.QPRegion_ID == p.Id);
						if (items.Count() == 0)
						{
							this.Modified = System.DateTime.Now;
							ods.Add(
								new AbstractItemAbtractItemRegionArticle 
								{
									QPAbstractItem_ID = this.Id,
									QPRegion = p
								}
							);
						}
					},
					delegate(IList<AbstractItemAbtractItemRegionArticle> ods, QPRegion p)
					{
						var items = ods.Where(od => od.QPRegion_ID == p.Id);
						if (items.Count() > 0)
						{
							this.Modified = System.DateTime.Now;
							var item = items.Single();
							item.InternalDataContext = InternalDataContext;
							InternalDataContext.AbstractItemAbtractItemRegionArticles.DeleteOnSubmit(item);
							item.SaveRemovingInstruction();
							ods.Remove(item);
						}
					}
				);
			}
			return _Regions;
		}
	}
			
	public string[] RegionsIDs
	{
		get
		{
			return Regions.Select(n => n.Id.ToString()).ToArray();
		}
	}
	
	public string RegionsString
	{
		get
		{
			return String.Join(",", RegionsIDs.ToArray());
		}
	}
	
}

public partial class QPDiscriminator
{
	

	private ListSelector<ItemDefinitionItemDefinitionArticle, QPDiscriminator> _AllowedItemDefinitions1 = null;

	public ListSelector<ItemDefinitionItemDefinitionArticle, QPDiscriminator> AllowedItemDefinitions1
	{
		get
		{
			if (_AllowedItemDefinitions1 == null)
			{
				_AllowedItemDefinitions1 = this.ItemDefinitionItemDefinitionArticles.GetNewBindingList()
				.AsListSelector<ItemDefinitionItemDefinitionArticle, QPDiscriminator>
				(
					od => od.QPDiscriminator2,
					delegate(IList<ItemDefinitionItemDefinitionArticle> ods, QPDiscriminator p)
					{
						var items = ods.Where(od => od.QPDiscriminator_ID2 == p.Id);
						if (items.Count() == 0)
						{
							this.Modified = System.DateTime.Now;
							ods.Add(
								new ItemDefinitionItemDefinitionArticle 
								{
									QPDiscriminator_ID = this.Id,
									QPDiscriminator2 = p
								}
							);
						}
					},
					delegate(IList<ItemDefinitionItemDefinitionArticle> ods, QPDiscriminator p)
					{
						var items = ods.Where(od => od.QPDiscriminator_ID2 == p.Id);
						if (items.Count() > 0)
						{
							this.Modified = System.DateTime.Now;
							var item = items.Single();
							item.InternalDataContext = InternalDataContext;
							InternalDataContext.ItemDefinitionItemDefinitionArticles.DeleteOnSubmit(item);
							item.SaveRemovingInstruction();
							ods.Remove(item);
						}
					}
				);
			}
			return _AllowedItemDefinitions1;
		}
	}
			
	public string[] AllowedItemDefinitions1IDs
	{
		get
		{
			return AllowedItemDefinitions1.Select(n => n.Id.ToString()).ToArray();
		}
	}
	
	public string AllowedItemDefinitions1String
	{
		get
		{
			return String.Join(",", AllowedItemDefinitions1IDs.ToArray());
		}
	}
	
}

	
public partial class AbstractItemAbtractItemRegionArticle
{
	private QPContext _InternalDataContext = LinqHelper.Context;
	
	public QPContext InternalDataContext
	{
		get { return _InternalDataContext; }
		set { _InternalDataContext = value; }
	}
        
	public decimal ITEM_ID
	{
		get
		{
			return _ITEM_ID;
		}
	}
	
	public decimal LINKED_ITEM_ID
	{
		get
		{
			return _LINKED_ITEM_ID;
		}
	}
	
	private string _removingInstruction;
	public string RemovingInstruction
	{
		get
		{
			if (String.IsNullOrEmpty(_removingInstruction))
				SaveRemovingInstruction();
			return _removingInstruction;
		}
	}
	
	public void SaveRemovingInstruction()
	{
		int linkId = InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "AbstractItemAbtractItemRegionArticle");
		
		if (linkId == 0)
			throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "AbstractItemAbtractItemRegionArticle", InternalDataContext.SiteId));
			
		_removingInstruction = String.Format("EXEC sp_executesql N'delete from item_to_item where link_id = @linkId and ((l_item_id = @itemId  and r_item_id = @linkedItemId) or (l_item_id = @linkedItemId  and r_item_id = @itemId))', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", linkId, this.ITEM_ID, this.LINKED_ITEM_ID);
	}
}

	
public partial class ItemDefinitionItemDefinitionArticle
{
	private QPContext _InternalDataContext = LinqHelper.Context;
	
	public QPContext InternalDataContext
	{
		get { return _InternalDataContext; }
		set { _InternalDataContext = value; }
	}
        
	public decimal ITEM_ID
	{
		get
		{
			return _ITEM_ID;
		}
	}
	
	public decimal LINKED_ITEM_ID
	{
		get
		{
			return _LINKED_ITEM_ID;
		}
	}
	
	private string _removingInstruction;
	public string RemovingInstruction
	{
		get
		{
			if (String.IsNullOrEmpty(_removingInstruction))
				SaveRemovingInstruction();
			return _removingInstruction;
		}
	}
	
	public void SaveRemovingInstruction()
	{
		int linkId = InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "ItemDefinitionItemDefinitionArticle");
		
		if (linkId == 0)
			throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "ItemDefinitionItemDefinitionArticle", InternalDataContext.SiteId));
			
		_removingInstruction = String.Format("EXEC sp_executesql N'delete from item_to_item where link_id = @linkId and ((l_item_id = @itemId  and r_item_id = @linkedItemId) or (l_item_id = @linkedItemId  and r_item_id = @itemId))', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", linkId, this.ITEM_ID, this.LINKED_ITEM_ID);
	}
}

}
