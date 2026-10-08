using EventBridge.Interfaces;
using EventBridge.Models;

namespace EventBridge.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IRepository<PlannerPortfolio> _portfolioRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public PortfolioService(IRepository<PlannerPortfolio> portfolioRepo, IUnitOfWork unitOfWork, IFileService fileService)
        {
            _portfolioRepo = portfolioRepo;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<bool> AddImageAsync(string plannerId, string imagePath, string caption)
        {
            var count = await GetPortfolioCountAsync(plannerId);
            if (count >= 10) return false; // Max 10 images

            var portfolio = new PlannerPortfolio
            {
                PlannerId = plannerId,
                ImagePath = imagePath,
                Caption = caption
            };

            await _portfolioRepo.AddAsync(portfolio);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImageAsync(int portfolioId, string plannerId)
        {
            var images = await _portfolioRepo.FindAsync(p => p.PortfolioId == portfolioId && p.PlannerId == plannerId);
            var image = images.FirstOrDefault();
            if (image == null) return false;

            _fileService.DeleteFile(image.ImagePath);
            _portfolioRepo.Remove(image);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PlannerPortfolio>> GetPortfolioAsync(string plannerId)
        {
            return await _portfolioRepo.FindAsync(p => p.PlannerId == plannerId);
        }

        public async Task<int> GetPortfolioCountAsync(string plannerId)
        {
            var images = await _portfolioRepo.FindAsync(p => p.PlannerId == plannerId);
            return images.Count();
        }
    }
}
