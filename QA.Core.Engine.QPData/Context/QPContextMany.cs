
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
                var result = this.AbstractItemAbtractItemRegionArticles.GetNewBindingList()
                .AsListSelector<AbstractItemAbtractItemRegionArticle, QPRegion>
                (
                    od => od.QPRegion,
                    delegate(IList<AbstractItemAbtractItemRegionArticle> ods, QPRegion p)
                    {
                        var items = ods.Where(od => od.QPRegion_ID == p.Id);
                        if (!items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = new AbstractItemAbtractItemRegionArticle();
                            item.QPAbstractItem_ID = this.Id;
                            item.QPRegion = p;
                            item.InsertWithArticle = true;
                            ods.Add(item);
                        }
                    },
                    delegate(IList<AbstractItemAbtractItemRegionArticle> ods, QPRegion p)
                    {
                        var items = ods.Where(od => od.QPRegion_ID == p.Id);
                        if (items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = items.Single();
                            InternalDataContext.AbstractItemAbtractItemRegionArticles.DeleteOnSubmit(item);
                            item.RemoveWithArticle = true;
                            ods.Remove(item);
                        }
                    }
                );
                if (this.PropertyChanging == null)
                    return result;
                else
                    _Regions = result;
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
                var result = this.ItemDefinitionItemDefinitionArticles.GetNewBindingList()
                .AsListSelector<ItemDefinitionItemDefinitionArticle, QPDiscriminator>
                (
                    od => od.QPDiscriminator2,
                    delegate(IList<ItemDefinitionItemDefinitionArticle> ods, QPDiscriminator p)
                    {
                        var items = ods.Where(od => od.QPDiscriminator_ID2 == p.Id);
                        if (!items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = new ItemDefinitionItemDefinitionArticle();
                            item.QPDiscriminator_ID = this.Id;
                            item.QPDiscriminator2 = p;
                            item.InsertWithArticle = true;
                            ods.Add(item);
                        }
                    },
                    delegate(IList<ItemDefinitionItemDefinitionArticle> ods, QPDiscriminator p)
                    {
                        var items = ods.Where(od => od.QPDiscriminator_ID2 == p.Id);
                        if (items.Any())
                        {
                            this.Modified = System.DateTime.Now;
                            var item = items.Single();
                            InternalDataContext.ItemDefinitionItemDefinitionArticles.DeleteOnSubmit(item);
                            item.RemoveWithArticle = true;
                            ods.Remove(item);
                        }
                    }
                );
                if (this.PropertyChanging == null)
                    return result;
                else
                    _AllowedItemDefinitions1 = result;
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


public partial class AbstractItemAbtractItemRegionArticle : QPLinkBase, IQPLink
{
    public override int Id
    {
        get
        {
            return _ITEM_ID;
        }
    }

    public override int LinkedItemId
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }


    [Obsolete]
    public decimal ITEM_ID
    {
        get
        {
            return _ITEM_ID;
        }
    }

    [Obsolete]
    public decimal LINKED_ITEM_ID
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }

    public override void Detach()
    {
        if (null == PropertyChanging)
            return;

        PropertyChanging = null;
        PropertyChanged = null;

        
        this._QPAbstractItem1 = Detach(this._QPAbstractItem1);

        this._QPRegion1 = Detach(this._QPRegion1);

    }

    public override int LinkId
    {
        get
        {
            int linkId = InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "AbstractItemAbtractItemRegionArticle");

            if (linkId == 0)
                throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "AbstractItemAbtractItemRegionArticle", InternalDataContext.SiteId));

            return linkId;
        }
    }
}


public partial class ItemDefinitionItemDefinitionArticle : QPLinkBase, IQPLink
{
    public override int Id
    {
        get
        {
            return _ITEM_ID;
        }
    }

    public override int LinkedItemId
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }

        
    [Obsolete]
    public decimal ITEM_ID
    {
        get
        {
            return _ITEM_ID;
        }
    }

    [Obsolete]
    public decimal LINKED_ITEM_ID
    {
        get
        {
            return _LINKED_ITEM_ID;
        }
    }

    public override void Detach()
    {
        if (null == PropertyChanging)
            return;

        PropertyChanging = null;
        PropertyChanged = null;


        this._QPDiscriminator1 = Detach(this._QPDiscriminator1);

        this._QPDiscriminator12 = Detach(this._QPDiscriminator12);

    }

    public override int LinkId
    {
        get
        {
            int linkId = InternalDataContext.Cnn.GetLinkIdByNetName(InternalDataContext.SiteId, "ItemDefinitionItemDefinitionArticle");

            if (linkId == 0)
                throw new Exception(String.Format("Junction class '{0}' is not found on the site (ID = {1})", "ItemDefinitionItemDefinitionArticle", InternalDataContext.SiteId));

            return linkId;
        }
    }
}

}
