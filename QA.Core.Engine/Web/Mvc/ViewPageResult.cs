using System.Web.Mvc;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
	/// <summary>
	/// ��������������� <see cref="ActionResult"/>, ������� ��������� �������� ��������� ���������� ��� ������������ ��������.
	/// </summary>
	public class ViewPageResult : ActionResult
	{
		private readonly AbstractItem _thePage;
		private readonly IControllerMapper _controllerMapper;
		private readonly IActionInvoker _actionInvoker;

        /// <summary>
        /// �������� ����������
        /// </summary>
        /// <param name="thePage">��������, ������� ���� ������������</param>
        /// <param name="controllerMapper"></param>
        /// <param name="actionInvoker"></param>
		public ViewPageResult(AbstractItem thePage, IControllerMapper controllerMapper, IActionInvoker actionInvoker)
		{
			_thePage = thePage;
			_controllerMapper = controllerMapper;
			_actionInvoker = actionInvoker;
		}

		public AbstractItem Page
		{
			get { return _thePage; }
		}

		/// <summary>
		/// ��������� Result
		/// </summary>
		/// <param name="context"/>
		public override void ExecuteResult(ControllerContext context)
		{
			context = BuildPageControllerContext(context);

			_actionInvoker.InvokeAction(context, "Index");
		}


		private ControllerContext BuildPageControllerContext(ControllerContext context)
		{
			string controllerName = _controllerMapper.GetControllerName(_thePage.GetContentType());
			
			var routeData = context.RouteData;
			RouteExtensions.ApplyCurrentItem(routeData, controllerName, "Index", _thePage, null);
			if (context.RouteData.DataTokens.ContainsKey(ContentRoute.ContentPartKey))
			{
				routeData.ApplyAbstractItem(ContentRoute.ContentPartKey, context.RouteData.DataTokens[ContentRoute.ContentPartKey] as AbstractItem);
			}

			var requestContext = new RequestContext(context.HttpContext, routeData);

			var controller = (ControllerBase)ControllerBuilder.Current.GetControllerFactory()
			                                 	.CreateController(requestContext, controllerName);

			controller.ControllerContext = new ControllerContext(requestContext, controller);
			controller.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);

			return controller.ControllerContext;
		}
	}
}