using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EventBridge.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IRepository<PlannerEventType> _plannerEventTypeRepo;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IRepository<Customer> customerRepo,
            IRepository<EventPlanner> plannerRepo,
            IRepository<PlannerEventType> plannerEventTypeRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;

            _customerRepo = customerRepo;
            _plannerRepo = plannerRepo;
            _plannerEventTypeRepo = plannerEventTypeRepo;
        }


        // =========================
        // LOGIN
        // =========================

        public async Task<SignInResult> LoginAsync(
            LoginViewModel model)
        {
            var user =
                await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return SignInResult.Failed;
            }

            if (user.Status != "Active" || !user.IsActive)
            {
                return SignInResult.NotAllowed;
            }

            return await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );
        }


        // =========================
        // LOGOUT
        // =========================

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        // =========================
        // CUSTOMER REGISTRATION
        // =========================

        public async Task<IdentityResult> RegisterCustomerAsync(
            RegisterCustomerViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.CountryCode + model.Phone,
                Role = "Customer",
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password
                );

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    user,
                    "Customer"
                );

                var customer = new Customer
                {
                    CustomerId = user.Id,
                    Address = model.Address
                };

                await _customerRepo.AddAsync(customer);

                await _unitOfWork.SaveChangesAsync();

                // Welcome email removed.
                // Verification email is now sent
                // from AccountController.
            }

            return result;
        }


        // =========================
        // PLANNER REGISTRATION
        // =========================

        public async Task<IdentityResult> RegisterPlannerAsync(
            RegisterPlannerViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.CountryCode + model.Phone,
                Role = "EventPlanner",
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password
                );

            if (!result.Succeeded)
            {
                return result;
            }


            await _userManager.AddToRoleAsync(
                user,
                "EventPlanner"
            );


            // Create planner
            var planner = new EventPlanner
            {
                PlannerId = user.Id,
                CompanyName = model.CompanyName,
                Description = model.Description,
                Experience = model.Experience
            };

            await _plannerRepo.AddAsync(planner);


            // Save selected event types
            if (model.SelectedCategoryIds != null)
            {
                foreach (var categoryId in model.SelectedCategoryIds.Distinct())
                {
                    var plannerEventType = new PlannerEventType
                    {
                        PlannerId = user.Id,
                        CategoryId = categoryId
                    };

                    await _plannerEventTypeRepo.AddAsync(
                        plannerEventType
                    );
                }
            }


            await _unitOfWork.SaveChangesAsync();


            // Welcome email removed.
            // Verification email is now sent
            // from AccountController.

            return result;
        }


        // =========================
        // EMAIL CONFIRMATION TOKEN
        // =========================

        public async Task<string> GenerateEmailConfirmationTokenAsync(
            string email)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            return await _userManager
                .GenerateEmailConfirmationTokenAsync(user);
        }

        // =========================
        // CHECK EMAIL CONFIRMATION STATUS
        // =========================

        public async Task<bool> IsEmailAlreadyConfirmedAsync(
            string email)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return false;
            }

            return await _userManager.IsEmailConfirmedAsync(user);
        }

        // =========================
        // CONFIRM EMAIL
        // =========================

        public async Task<IdentityResult> ConfirmEmailAsync(
            string email,
            string token)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    }
                );
            }

            return await _userManager.ConfirmEmailAsync(
                user,
                token
            );
        }


        // =========================
        // PASSWORD RESET TOKEN
        // =========================

        public async Task<string> GeneratePasswordResetTokenAsync(
            string email)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            return await _userManager
                .GeneratePasswordResetTokenAsync(user);
        }

        // =========================
        // VALIDATE PASSWORD RESET TOKEN
        // =========================

        public async Task<bool> ValidatePasswordResetTokenAsync(
            string email,
            string token)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return false;
            }

            return await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<User>.ResetPasswordTokenPurpose,
                token
            );
        }

        // =========================
        // RESET PASSWORD
        // =========================

        public async Task<IdentityResult> ResetPasswordAsync(
            ResetPasswordViewModel model)
        {
            var user =
                await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    }
                );
            }

            return await _userManager.ResetPasswordAsync(
                user,
                model.Code,
                model.Password
            );
        }
    }
}