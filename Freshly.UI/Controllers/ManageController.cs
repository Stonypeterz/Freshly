using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Freshly.Identity;
using Freshly.Identity.Models;
using Freshly.UI.Models;

namespace Freshly.UI.Controllers {

	public class ManageController : Controller {

		private readonly UserManager df;
		private readonly ICustomHelpers AppUtil;

		public ManageController(UserManager _df, ICustomHelpers _ch) {
			df = _df;
			AppUtil = _ch;
		}

		public async Task<IActionResult> Index() {
			try {
				return View("Page", await df.GetUsersAsync(1));
			}
			catch (Exception ex) {
				return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex)) });
			}
		}

		public async Task<IActionResult> Page(int pageno, string q) {
			try {
				return View(await df.GetUsersAsync(pageno, AppUtil.GVs.PageSize, string.IsNullOrEmpty(q) ? "0" : q));
			}
			catch (Exception ex) {
				return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex)) });
			}
		}

		public async Task<IActionResult> Details(string UserId) {
			try {
				ApplicationUser obj = await df.GetUserAsync(UserId);
				if(obj != null) return View(obj);
				else return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, "The record was not found.") });
			}
			catch (Exception ex) {
				return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex)) });
			}
		}

		//public IActionResult Create() {
		//	return View(new ApplicationUser());
		//}

		//[HttpPost()]
		//[ValidateAntiForgeryToken()]
		//public async Task<IActionResult> Create(ApplicationUser obj) {
		//	if(ModelState.IsValid) {
		//		try {
		//			await df.InsertAsync(obj);
		//			ViewBag.Msg = AppUtil.GetAlert(Alerts.success, "The record was added successfully!");
		//			return View(new ApplicationUser());
		//	}
		//		catch (Exception ex) {
		//			ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex));
		//		}
		//	}
		//	return View(obj);
		//}

		public async Task<IActionResult> Edit(string UserId) {
			try {
				ApplicationUser obj = await df.GetUserAsync(UserId);
				if(obj != null) return View(obj);
				else return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, "The record was not found.") });
			}
			catch (Exception ex) {
				return View("Resp", new ResponseObject() { Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex)) });
			}
		}

		[HttpPost()]
		[ValidateAntiForgeryToken()]
		public async Task<IActionResult> Edit(ApplicationUser obj) {
			if(ModelState.IsValid) {
				try {
					await df.UpdateAsync(obj);
					ViewBag.Msg = AppUtil.GetAlert(Alerts.success, "The record was updated successfully!");
					return View("Details", obj);
			}
				catch (Exception ex) {
					ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex));
				}
			}
			return View(obj);
		}

		[HttpPost()]
		[ValidateAntiForgeryToken()]
		public async Task<IActionResult> Delete(string UserId) {
			ResponseObject Resp = new ResponseObject();
			if(ModelState.IsValid) {
				try {
					await df.DeleteAsync(UserId);
					Resp.Msg = AppUtil.GetAlert(Alerts.success, "The record was deleted successfully!");
				}
				catch (Exception ex) {
					Resp.Msg = AppUtil.GetAlert(Alerts.danger, AppUtil.LogError(ex));
				}
			}
			ViewBag.ListUrl = Url.Action("Index");
			Resp.Msg = string.Format("{0} <a href=\"{1}\">Click here</a> To go To the list.", Resp.Msg, ViewBag.ListUrl);
			return View("Resp", Resp);
		}

	}

}

