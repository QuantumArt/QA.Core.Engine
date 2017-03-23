using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Data.Resolvers;
using QA.Core.Engine.Data;
using QA.Core.Engine.QPData;
using QA.Core.Engine.UI;
using QA.Core.Logger;

namespace QA.Core.Engine.Tests
{
	[PartDefinition("FAQ Список ссылок",
	Discriminator = "faq_links_list", NeedLoadM2MRelationsIds=true)]
	public class FaqLinksListWidget : AbstractItem
	{

	}

	//[TestClass]
	public class AbstractItemLoaderTests
	{
		//[TestInitialize]
		public void Initialize()
		{
			var ms = new FileXmlMappingResolver("Mapping\\QPContext_Stage.map", "Mapping\\QPContext_Live.map");
			QPContext.DefaultXmlMappingSource = ms.GetMapping(true);
		}

		//[TestMethod]
		public void AbstractItemLoader_Simple()
		{
			var model = new AbstractItemModel<int, AbstractItem>();

			LoadAll(model);

			Debug.WriteLine(String.Join(", ", model.Items.Keys));

			var item = model.Items[260899];
			Assert.IsNotNull(item);
			Assert.AreEqual(1, item.GetRelationIds("Questions").Count);
			Assert.AreEqual(2, item.GetRelationIds("Questions2").Count);
		}

		private static void LoadAll(AbstractItemModel<int, AbstractItem> model)
		{
			var tf = new FakeTypeFinder();
			var loader = new AbstractItemLoader(
				new AbstractItemActivator(tf, new CombinedDefinitionManager(tf, new NullLogger())), null, new CombinedDefinitionManager(tf, new NullLogger()));

			loader.LoadAll(model);
		}
	}
}
