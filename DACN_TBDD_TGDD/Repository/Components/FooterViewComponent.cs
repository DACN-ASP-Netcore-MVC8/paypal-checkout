using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN_TBDD_TGDD.Repository.Components
{
	public class FooterViewComponent : ViewComponent
	{
		private readonly DataContext _dataContext;
		public FooterViewComponent(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Contacts.FirstOrDefaultAsync	());
	}
}
