using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EventBridge.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;

        public AccountController(
            IAuthService authService,
            IRepository<Category> categoryRepo,
            IEmailService emailService,
            IMemoryCache memoryCache)
        {
            _authService = authService;
            _categoryRepo = categoryRepo;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }


        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _authService.LoginAsync(model);

                if (result.Succeeded)
                {
                    return RedirectToAction(
                        "Index",
                        "Dashboard"
                    );
                }

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your account is inactive or your email has not been verified."
                    );
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid login attempt."
                    );
                }
            }

            return View(model);
        }


        // =========================
        // CUSTOMER REGISTRATION
        // =========================

        [HttpGet]
        public IActionResult RegisterCustomer()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCustomer(
            RegisterCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _authService.RegisterCustomerAsync(model);

                if (result.Succeeded)
                {
                    await SendVerificationEmailAsync(model.Email);

                    TempData["VerificationEmail"] = model.Email;

                    return RedirectToAction(
                        "CheckEmail",
                        "Account"
                    );
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }
            }

            return View(model);
        }


        // =========================
        // PLANNER REGISTRATION
        // =========================

        [HttpGet]
        public async Task<IActionResult> RegisterPlanner()
        {
            var model =
                new RegisterPlannerViewModel();

            await LoadCategoriesAsync(model);

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPlanner(
            RegisterPlannerViewModel model)
        {
            if (model.SelectedCategoryIds == null ||
                !model.SelectedCategoryIds.Any())
            {
                ModelState.AddModelError(
                    nameof(model.SelectedCategoryIds),
                    "Please select at least one event type."
                );
            }

            if (ModelState.IsValid)
            {
                var result =
                    await _authService.RegisterPlannerAsync(model);

                if (result.Succeeded)
                {
                    await SendVerificationEmailAsync(model.Email);

                    TempData["VerificationEmail"] = model.Email;

                    return RedirectToAction(
                        "CheckEmail",
                        "Account"
                    );
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }
            }

            await LoadCategoriesAsync(model);

            return View(model);
        }


        private async Task LoadCategoriesAsync(
            RegisterPlannerViewModel model)
        {
            var categories =
                await _categoryRepo.GetAllAsync();

            model.Categories = categories
                .OrderBy(c => c.Name)
                .Select(c =>
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected =
                            model.SelectedCategoryIds.Contains(c.Id)
                    })
                .ToList();
        }


        // =========================
        // SEND VERIFICATION EMAIL
        // =========================

        private async Task SendVerificationEmailAsync(
            string email)
        {
            var token =
                await _authService
                    .GenerateEmailConfirmationTokenAsync(email);

            if (token == null)
            {
                return;
            }

            var verificationUrl =
                Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        email = email,
                        token = token
                    },
                    protocol: Request.Scheme
                );


            await _emailService.SendEmailAsync(
                email,
                "Verify Your EventBridge Email",
                $@"
                    <!DOCTYPE html>
                    <html>
                    <body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;"">

                        <div style=""
                            max-width:600px;
                            margin:40px auto;
                            background:#ffffff;
                            border-radius:12px;
                            padding:40px;
                            text-align:center;
                            box-shadow:0 4px 20px rgba(0,0,0,0.08);
                        "">

                            <h1 style=""
                                color:#52796f;
                                margin-bottom:10px;
                            "">
                                EventBridge
                            </h1>

                            <h2 style=""
                                color:#263238;
                            "">
                                Verify Your Email
                            </h2>

                            <p style=""
                                color:#607d8b;
                                font-size:15px;
                                line-height:1.6;
                            "">
                                Thank you for registering with EventBridge.
                                Please verify your email address to activate
                                your account.
                            </p>

                            <div style=""margin:30px 0;"">

                                <a href=""{verificationUrl}""
                                   style=""
                                       display:inline-block;
                                       background:#52796f;
                                       color:#ffffff;
                                       text-decoration:none;
                                       padding:14px 28px;
                                       border-radius:7px;
                                       font-size:16px;
                                       font-weight:bold;
                                   "">

                                    Verify Email

                                </a>

                            </div>

                            <p style=""
                                color:#7b8581;
                                font-size:13px;
                                line-height:1.5;
                            "">
                                You must verify your email before you can
                                log in to EventBridge.
                            </p>

                            <p style=""
                                color:#9aa5a1;
                                font-size:12px;
                                margin-top:30px;
                            "">
                                This is an automated email from EventBridge.
                            </p>

                        </div>

                    </body>
                    </html>
                "
            );
        }


        // =========================
        // CHECK YOUR EMAIL
        // =========================

        [HttpGet]
        public IActionResult CheckEmail()
        {
            var email =
                TempData["VerificationEmail"]?.ToString();

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(model: email);
        }

        // =========================
        // RESEND VERIFICATION EMAIL
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(CheckEmail));
            }

            var cacheKey =
                $"EmailVerificationCooldown_{email.ToLower()}";

            if (_memoryCache.TryGetValue(cacheKey, out _))
            {
                TempData["ErrorMessage"] =
                    "Please wait 60 seconds before requesting another verification email.";

                TempData["VerificationEmail"] = email;

                return RedirectToAction(nameof(CheckEmail));
            }

            var token =
                await _authService
                    .GenerateEmailConfirmationTokenAsync(email);

            if (token == null)
            {
                return RedirectToAction(nameof(CheckEmail));
            }

            // Start 60-second cooldown
            _memoryCache.Set(
                cacheKey,
                true,
                TimeSpan.FromSeconds(60)
            );

            var verificationUrl =
                Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        email = email,
                        token = token
                    },
                    protocol: Request.Scheme
                );

            await _emailService.SendEmailAsync(
                email,
                "Verify Your EventBridge Email",
                $@"
                    <!DOCTYPE html>
                    <html>
                    <body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;"">

                        <div style=""max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center;"">

                            <h1 style=""color:#52796f;"">
                                EventBridge
                            </h1>

                            <h2 style=""color:#263238;"">
                                Verify Your Email
                            </h2>

                            <p style=""color:#607d8b; font-size:15px; line-height:1.6;"">
                                Please click the button below to verify your
                                EventBridge email address.
                            </p>

                            <div style=""margin:30px 0;"">

                                <a href=""{verificationUrl}""
                                   style=""display:inline-block; background:#52796f; color:#ffffff; text-decoration:none; padding:14px 28px; border-radius:7px; font-size:16px; font-weight:bold;"">

                                    Verify Email

                                </a>

                            </div>

                            <p style=""color:#9aa5a1; font-size:12px;"">
                                This is an automated email from EventBridge.
                            </p>

                        </div>

                    </body>
                    </html>
                "
            );

            TempData["SuccessMessage"] =
                "A new verification email has been sent.";

            TempData["VerificationEmail"] = email;

            return RedirectToAction(nameof(CheckEmail));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
    string email,
    string token)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(token))
            {
                return BadRequest(
                    "A valid email verification link is required."
                );
            }

            // Check if the email is already verified
            if (await _authService.IsEmailAlreadyConfirmedAsync(email))
            {
                return RedirectToAction(
                    "AlreadyVerified",
                    "Account"
                );
            }

            var result =
                await _authService.ConfirmEmailAsync(
                    email,
                    token
                );

            if (result.Succeeded)
            {
                return RedirectToAction(
                    "EmailConfirmed",
                    "Account"
                );
            }

            TempData["VerificationEmail"] = email;

            return RedirectToAction(
                "EmailConfirmationFailed",
                "Account"
            );
        }

        // =========================
        // EMAIL CONFIRMED
        // =========================

        [HttpGet]
        public IActionResult EmailConfirmed()
        {
            return View();
        }

        // =========================
        // EMAIL ALREADY VERIFIED
        // =========================

        [HttpGet]
        public IActionResult AlreadyVerified()
        {
            return View();
        }

        // =========================
        // EMAIL CONFIRMATION FAILED
        // =========================

        [HttpGet]
        public IActionResult EmailConfirmationFailed()
        {
            var email =
                TempData["VerificationEmail"]?.ToString();

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Login));
            }

            TempData["VerificationEmail"] = email;

            return View(model: email);
        }

        // =========================
        // ACCESS DENIED
        // =========================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =========================
        // FORGOT PASSWORD
        // =========================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var code =
                await _authService
                    .GeneratePasswordResetTokenAsync(
                        model.Email
                    );

            if (code == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please enter a registered email address."
                );

                return View(model);
            }


            var resetUrl =
                Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        code = code,
                        email = model.Email
                    },
                    protocol: Request.Scheme
                );


            await _emailService.SendEmailAsync(
                model.Email,
                "Reset Your EventBridge Password",
                $@"
                    <!DOCTYPE html>
                    <html>
                    <body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial, sans-serif;"">

                        <div style=""max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center; box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

                            <h1 style=""color:#12615c; margin-bottom:10px;"">
                                EventBridge
                            </h1>

                            <h2 style=""color:#263238;"">
                                Reset Your Password
                            </h2>

                            <p style=""color:#607d8b; font-size:15px; line-height:1.6;"">
                                We received a request to reset your EventBridge password.
                                Click the button below to create a new password.
                            </p>

                            <div style=""margin:30px 0;"">
                                <a href=""{resetUrl}""
                                   style=""display:inline-block; background:#12615c; color:#ffffff; text-decoration:none; padding:14px 28px; border-radius:7px; font-size:16px; font-weight:bold;"">
                                    Reset Password
                                </a>
                            </div>

                            <p style=""color:#7b8581; font-size:13px; line-height:1.5;"">
                                If you did not request a password reset,
                                you can safely ignore this email.
                            </p>

                            <p style=""color:#9aa5a1; font-size:12px; margin-top:30px;"">
                                © {DateTime.Now.Year} EventBridge
                            </p>

                        </div>

                    </body>
                    </html>
                "
            );


            return RedirectToAction(
                "ForgotPasswordConfirmation",
                "Account"
            );
        }


        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // =========================
        // RESET PASSWORD
        // =========================

        [HttpGet]
        public async Task<IActionResult> ResetPassword(
            string code = null,
            string email = null)
        {
            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(email))
            {
                return View("InvalidResetPasswordLink");
            }

            var tokenValid =
                await _authService.ValidatePasswordResetTokenAsync(
                    email,
                    code
                );

            if (!tokenValid)
            {
                return View("InvalidResetPasswordLink");
            }

            var model =
                new ResetPasswordViewModel
                {
                    Code = code,
                    Email = email
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await _authService
                    .ResetPasswordAsync(model);

            if (result.Succeeded)
            {
                return RedirectToAction(
                    "ResetPasswordConfirmation",
                    "Account"
                );
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}