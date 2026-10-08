using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace EventBridge.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IFileService _fileService;

        public ProfileService(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            IRepository<Customer> customerRepo,
            IRepository<EventPlanner> plannerRepo,
            IFileService fileService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _customerRepo = customerRepo;
            _plannerRepo = plannerRepo;
            _fileService = fileService;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        }

        public async Task<ProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email!,
                Phone = user.PhoneNumber!,
                Role = user.Role,
                ProfilePicturePath = user.ProfilePicturePath
            };

            if (user.Role == "Customer")
            {
                var customers = await _customerRepo.FindAsync(c => c.CustomerId == userId);
                var customer = customers.FirstOrDefault();
                if (customer != null)
                {
                    model.Address = customer.Address;
                }
            }
            else if (user.Role == "EventPlanner")
            {
                var planners = await _plannerRepo.FindAsync(p => p.PlannerId == userId);
                var planner = planners.FirstOrDefault();
                if (planner != null)
                {
                    model.CompanyName = planner.CompanyName;
                    model.Description = planner.Description;
                    model.Experience = planner.Experience;
                }
            }

            return model;
        }

        public async Task<IdentityResult> UpdateProfileAsync(string userId, ProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.FullName = model.FullName;
            user.PhoneNumber = model.Phone;

            if(model.ProfilePicture != null)
{
                try
                {
                    // Save the new picture first so the existing
                    // picture is not deleted if validation fails.
                    var newProfilePicturePath =
                        await _fileService.SaveFileAsync(
                            model.ProfilePicture,
                            "profile");

                    // Delete the old picture only after the new
                    // picture has been successfully saved.
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                    {
                        _fileService.DeleteFile(user.ProfilePicturePath);
                    }

                    user.ProfilePicturePath = newProfilePicturePath;
                }
                catch (InvalidOperationException ex)
                {
                    return IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = ex.Message
                        });
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return updateResult;

            if (user.Role == "Customer")
            {
                var customers = await _customerRepo.FindAsync(c => c.CustomerId == userId);
                var customer = customers.FirstOrDefault();
                if (customer != null)
                {
                    customer.Address = model.Address ?? string.Empty;
                    _customerRepo.Update(customer);
                }
            }
            else if (user.Role == "EventPlanner")
            {
                var planners = await _plannerRepo.FindAsync(p => p.PlannerId == userId);
                var planner = planners.FirstOrDefault();
                if (planner != null)
                {
                    planner.CompanyName = model.CompanyName ?? string.Empty;
                    planner.Description = model.Description ?? string.Empty;
                    planner.Experience = model.Experience ?? 0;
                    _plannerRepo.Update(planner);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
