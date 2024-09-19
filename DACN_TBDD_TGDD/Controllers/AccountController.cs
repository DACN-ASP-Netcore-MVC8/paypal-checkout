using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DACN_TBDD_TGDD.Areas.Admin.Repository;


public class AccountController : Controller
{
    private readonly UserManager<AppUserModel> _userManager;
    private readonly SignInManager<AppUserModel> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailSender _emailSender;

	public AccountController(SignInManager<AppUserModel> signInManager,
							UserManager<AppUserModel> userManager,
							RoleManager<IdentityRole> roleManager,
							IEmailSender emailSender)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_roleManager = roleManager;
		_emailSender = emailSender; // Inject the email sender
	}
    private string GenerateRandomPassword()
    {
        var length = 12; // Length of the new password
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var password = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return password;
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Generate a new password
                var newPassword = GenerateRandomPassword();

                // Reset the user's password
                var resetResult = await _userManager.RemovePasswordAsync(user);
                if (resetResult.Succeeded)
                {
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
                    if (addPasswordResult.Succeeded)
                    {
						// Send the new password via email
						var subject = "Mật khẩu mới của bạn";
						var message = $@"
Xin chào,

Chúng tôi đã thiết lập lại mật khẩu của bạn thành công. Mật khẩu mới của bạn là:

{newPassword}

Vui lòng sử dụng mật khẩu này để đăng nhập vào tài khoản của bạn. Không chia sẻ mật khẩu của bạn";


						try
						{
                            await _emailSender.SendEmailAsync(model.Email, subject, message);
                            return RedirectToAction("ForgotPasswordConfirmation");
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError(string.Empty, "Failed to send the new password email.");
                            TempData["errolSearchEmail"] = "Failed to send the new password email.";
                        }
                    }
                    else
                    {
                        foreach (var error in addPasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            TempData["errolSearchEmail"] = "Failed to reset the password.";
                        }
                    }
                }
                else
                {
                    foreach (var error in resetResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        TempData["errolSearchEmail"] = "Failed to reset the password.";
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email address.");
                TempData["errolSearchEmail"] = "Email address not found.";
            }
        }

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid request.");
        }

        return View(model);
    }

    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }


    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginVM)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Lấy thông tin người dùng
                var user = await _userManager.FindByNameAsync(loginVM.UserName);

                if (user == null)
                {
                    TempData["ErrorMessageUser"] = "User not found or password incorrect.";
                    return View(loginVM);
                }

                // Kiểm tra vai trò
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin") || roles.Contains("Sales"))
                {
                    return Redirect("/Admin");
                }
                else
                {
                    TempData["success"] = "Đăng nhập thành công";

                    // Gửi email thông báo đăng nhập thành công
                    var subject = "Đăng nhập thành công";
					var message = $@"
                Kính gửi {user.UserName},
                Chúng tôi xin gửi lời chúc mừng bạn đã đăng nhập thành công vào hệ thống của chúng tôi. 
                Chúng tôi rất vui mừng khi bạn trở lại và hy vọng bạn sẽ có một trải nghiệm tuyệt vời với các dịch vụ và sản phẩm của chúng tôi. Nếu bạn có bất kỳ câu hỏi nào hoặc cần hỗ trợ, đừng ngần ngại liên hệ với chúng tôi qua email hoặc số điện thoại hỗ trợ.
                Cảm ơn bạn đã lựa chọn chúng tôi. Chúng tôi luôn sẵn sàng hỗ trợ bạn!
                Chúc bạn một ngày tốt lành!

                    Trân trọng";

                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email, subject, message);
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi khi gửi email (ghi log hoặc thông báo)
                        ModelState.AddModelError(string.Empty, "Gửi email thông báo thất bại.");
                    }

                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
            }
            TempData["ErrorMessageUser"] = "User not found or password incorrect.";
        }
        return View(loginVM);
    }



    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserModel userModel)
    {
        if (ModelState.IsValid)
        {
            var newUser = new AppUserModel
            {
                UserName = userModel.UserName,
                Email = userModel.Email
            };

            var result = await _userManager.CreateAsync(newUser, userModel.Password);

            if (result.Succeeded)
            {
                TempData["successCreated"] = "Account created successfully.";

                // Gửi email thông báo tạo tài khoản thành công
                var subject = "Chúc mừng! Tạo tài khoản thành công";
				var message = $@"
Kính gửi {userModel.UserName},

Chúng tôi vui mừng thông báo rằng tài khoản của bạn đã được tạo thành công trên hệ thống của chúng tôi.

Giờ đây, bạn có thể đăng nhập vào tài khoản của mình và bắt đầu khám phá tất cả các dịch vụ và tính năng mà chúng tôi cung cấp. Nếu bạn cần hỗ trợ thêm hoặc có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với đội ngũ hỗ trợ của chúng tôi. Chúng tôi luôn sẵn sàng để giúp đỡ bạn.

Chúng tôi hy vọng bạn sẽ có một trải nghiệm tuyệt vời với chúng tôi và tận hưởng mọi lợi ích mà tài khoản của bạn mang lại.

Cảm ơn bạn đã chọn chúng tôi và chào mừng bạn gia nhập cộng đồng của chúng tôi!

Trân trọng";

				try
                {
                    await _emailSender.SendEmailAsync(newUser.Email, subject, message);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi gửi email (ghi log hoặc thông báo)
                    ModelState.AddModelError(string.Empty, "Gửi email thông báo thất bại.");
                }

                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToAction("Index","Home");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(userModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {

        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name"); // Display role names
        return View(user);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppUserModel model, string selectedRole)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains(selectedRole))
                {
                    currentRoles.Remove(selectedRole);
                }
                else
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, selectedRole);
                }

                TempData["successEditAccount"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name", selectedRole); // Update selected role
        return View(model);
    }
	[HttpGet]
	public IActionResult ChangePassword()
	{
		return View();
	}
	[HttpPost]
	public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return RedirectToAction("Login");
		}

		var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

		if (result.Succeeded)
		{
			await _signInManager.RefreshSignInAsync(user); // Đảm bảo người dùng vẫn được đăng nhập sau khi đổi mật khẩu
			TempData["successChangePassword"] = "Your password has been changed successfully.";
			return RedirectToAction("Index", "Home");
		}

		foreach (var error in result.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		return View(model);
	}
	[HttpGet]
	public async Task<IActionResult> EditProfile()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return RedirectToAction("Login");
		}

		var model = new EditProfileViewModel
		{
			UserName = user.UserName,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
			Address = user.Address, // Assuming `AppUserModel` has Address property
			BirthDate = user.BirthDate // Assuming `AppUserModel` has BirthDate property
		};

		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditProfile(EditProfileViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return RedirectToAction("Login");
		}

		user.UserName = model.UserName;
		user.Email = model.Email;
		user.PhoneNumber = model.PhoneNumber;
		user.Address = model.Address; // Update Address
		user.BirthDate = model.BirthDate; // Update BirthDate

		var result = await _userManager.UpdateAsync(user);
		if (result.Succeeded)
		{
			TempData["successEdit"] = "Your profile has been updated successfully.";
			return RedirectToAction("Index", "Home");
		}

		foreach (var error in result.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		return View(model);
	}



}
