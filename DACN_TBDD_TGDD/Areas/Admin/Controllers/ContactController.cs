using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin,Sales")]
	public class ContactController : Controller
	{
        private readonly DataContext _dataContext;
        public ContactController(DataContext dataContext)
        {
            _dataContext = dataContext;
            
        }
        public async Task<IActionResult> Index(int page = 1)
		{
            const int pageSize = 10; // Define page size
                                     // Fetch contacts asynchronously from the database
            var contacts = await _dataContext.Contacts.ToListAsync();

            // Calculate pagination details
            var paginatedContacts = contacts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Set up pager (use your Pager class or similar implementation)
            var pager = new Paginate(contacts.Count(), page, pageSize);

            // Pass pagination info and paginated data to the view
            ViewBag.Pager = pager;
            return View(paginatedContacts); 
        }
		

        // POST: Admin/Contact/Edit
        public async Task<IActionResult> Edit(string name)
        {
            var contact = await _dataContext.Contacts
       .FirstOrDefaultAsync(c => c.Name == name);

            if (contact == null)
            {
                return NotFound();
            }

            // Pass the updated contact to the view
            return View(contact);
        }

        // POST: Admin/Contact/Edit
        // ... (other controller code)

        // POST: Admin/Contact/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name,Map")] ContactModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingContact = await _dataContext.Contacts
                        .FirstOrDefaultAsync(c => c.Name == contact.Name);

                    if (existingContact == null)
                    {
                        return NotFound();
                    }

                    existingContact.Map = contact.Map;
                    _dataContext.Entry(existingContact).State = EntityState.Modified;
                    await _dataContext.SaveChangesAsync();

                    // Redirect to the Edit action with the updated contact
                    return RedirectToAction("Edit", new { name = contact.Name });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Name))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(contact);
        }


        private bool ContactExists(string name)
        {
            return _dataContext.Contacts.Any(e => e.Name == name);
        }
    }
}
