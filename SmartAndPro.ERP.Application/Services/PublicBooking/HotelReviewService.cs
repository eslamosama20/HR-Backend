using AutoMapper;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Domains.Models.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.PublicBooking;

public interface IHotelReviewService
{
    Task<OperationResponse<HotelReviewDto>> AddAsync(CreateHotelReviewDto dto);
    Task<OperationResponse<HotelReviewDto>> GetAsync(Guid id);
    Task<OperationResponse<List<HotelReviewDto>>> GetByHotelAsync(Guid hotelId, bool includeNotApproved = false);
    Task<OperationResponse<bool>> UpdateResponseAsync(UpdateReviewResponseDto dto, Guid userId);
    Task<OperationResponse<bool>> ApproveReviewAsync(ApproveReviewDto dto);
    Task<OperationResponse<bool>> DeleteAsync(Guid id);
    Task<OperationResponse<Dictionary<string, object>>> GetHotelRatingStatsAsync(Guid hotelId);
}

public class HotelReviewService : IHotelReviewService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public HotelReviewService(RepositoriesFactory repositoriesFactory, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public async Task<OperationResponse<HotelReviewDto>> AddAsync(CreateHotelReviewDto dto)
    {
        var result = new OperationResponse<HotelReviewDto>();
        var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(true);

        try
        {
            // Validate hotel exists
            var hotelRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicHotel>(false);
            var hotelQuery = await hotelRepo.GetAll();
            var hotel = await hotelQuery.Data.FirstOrDefaultAsync(h => h.Id == dto.HotelId);

            if (hotel == null)
            {
                result.ErrorCode = ErrorCodes.NotFound;
                result.Message = "Hotel not found";
                return result;
            }

            // Check if booking exists and matches guest email (for verification)
            bool isVerified = false;
            if (dto.BookingId.HasValue)
            {
                var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(false);
                var bookingQuery = await bookingRepo.GetAll();
                var booking = await bookingQuery.Data
                    .FirstOrDefaultAsync(b => b.Id == dto.BookingId.Value && 
                                             b.GuestEmail.ToLower() == dto.GuestEmail.ToLower());

                if (booking != null)
                {
                    isVerified = true;
                }
            }

            // Create review entity
            var review = new HotelReview
            {
                HotelId = dto.HotelId,
                BookingId = dto.BookingId,
                GuestName = dto.GuestName,
                GuestEmail = dto.GuestEmail,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Title = dto.Title,
                StayDate = dto.StayDate,
                IsVerified = isVerified,
                IsApproved = false, // Requires admin approval
                IsVisible = true
            };

            await reviewRepo.Add(review);

            // Map to DTO
            var reviewDto = _mapper.Map<HotelReviewDto>(review);
            reviewDto.HotelName = hotel.Name;

            result.ErrorCode = ErrorCodes.Success;
            result.Data = reviewDto;
            result.Message = "Review submitted successfully. It will be visible after admin approval.";
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error adding review: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<HotelReviewDto>> GetAsync(Guid id)
    {
        var result = new OperationResponse<HotelReviewDto>();

        try
        {
            var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(false);
            var query = await reviewRepo.GetAll();
            
            var review = await query.Data
                .Include(r => r.Hotel)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                result.ErrorCode = ErrorCodes.NotFound;
                result.Message = "Review not found";
                return result;
            }

            var reviewDto = _mapper.Map<HotelReviewDto>(review);
            reviewDto.HotelName = review.Hotel?.Name ?? "";

            result.ErrorCode = ErrorCodes.Success;
            result.Data = reviewDto;
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error retrieving review: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<List<HotelReviewDto>>> GetByHotelAsync(Guid hotelId, bool includeNotApproved = false)
    {
        var result = new OperationResponse<List<HotelReviewDto>>();

        try
        {
            var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(false);
            var query = await reviewRepo.GetAll();

            var reviews = await query.Data
                .Include(r => r.Hotel)
                .Include(r => r.Booking)
                .Where(r => r.HotelId == hotelId && r.IsVisible)
                .Where(r => includeNotApproved || r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewDtos = reviews.Select(r =>
            {
                var dto = _mapper.Map<HotelReviewDto>(r);
                dto.HotelName = r.Hotel?.Name ?? "";
                return dto;
            }).ToList();

            result.ErrorCode = ErrorCodes.Success;
            result.Data = reviewDtos;
            result.Message = $"Found {reviewDtos.Count} reviews";
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error retrieving reviews: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<bool>> UpdateResponseAsync(UpdateReviewResponseDto dto, Guid userId)
    {
        var result = new OperationResponse<bool>();
        var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(true);

        try
        {
            var query = await reviewRepo.GetAll();
            var review = await query.Data.FirstOrDefaultAsync(r => r.Id == dto.ReviewId);

            if (review == null)
            {
                result.ErrorCode = ErrorCodes.NotFound;
                result.Message = "Review not found";
                return result;
            }

            review.AdminResponse = dto.AdminResponse;
            review.AdminResponseDate = DateTime.Now;

            await reviewRepo.Update(review);

            result.ErrorCode = ErrorCodes.Success;
            result.Data = true;
            result.Message = "Response added successfully";
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error updating response: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<bool>> ApproveReviewAsync(ApproveReviewDto dto)
    {
        var result = new OperationResponse<bool>();
        var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(true);

        try
        {
            var query = await reviewRepo.GetAll();
            var review = await query.Data.FirstOrDefaultAsync(r => r.Id == dto.ReviewId);

            if (review == null)
            {
                result.ErrorCode = ErrorCodes.NotFound;
                result.Message = "Review not found";
                return result;
            }

            review.IsApproved = dto.IsApproved;

            await reviewRepo.Update(review);

            result.ErrorCode = ErrorCodes.Success;
            result.Data = true;
            result.Message = dto.IsApproved ? "Review approved" : "Review approval revoked";
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error approving review: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<bool>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<bool>();
        var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(true);

        try
        {
            var query = await reviewRepo.GetAll();
            var review = await query.Data.FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                result.ErrorCode = ErrorCodes.NotFound;
                result.Message = "Review not found";
                return result;
            }

            await reviewRepo.Delete(review);

            result.ErrorCode = ErrorCodes.Success;
            result.Data = true;
            result.Message = "Review deleted successfully";
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error deleting review: {ex.Message}";
        }

        return result;
    }

    public async Task<OperationResponse<Dictionary<string, object>>> GetHotelRatingStatsAsync(Guid hotelId)
    {
        var result = new OperationResponse<Dictionary<string, object>>();

        try
        {
            var reviewRepo = _repositoriesFactory.GetRepository<HotelReview>(false);
            var query = await reviewRepo.GetAll();

            var reviews = await query.Data
                .Where(r => r.HotelId == hotelId && r.IsApproved && r.IsVisible)
                .ToListAsync();

            if (!reviews.Any())
            {
                result.ErrorCode = ErrorCodes.Success;
                result.Data = new Dictionary<string, object>
                {
                    { "totalReviews", 0 },
                    { "averageRating", 0.0 },
                    { "ratingDistribution", new Dictionary<int, int>() }
                };
                return result;
            }

            var totalReviews = reviews.Count;
            var averageRating = reviews.Average(r => r.Rating);
            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            result.ErrorCode = ErrorCodes.Success;
            result.Data = new Dictionary<string, object>
            {
                { "totalReviews", totalReviews },
                { "averageRating", Math.Round(averageRating, 1) },
                { "ratingDistribution", ratingDistribution }
            };
        }
        catch (Exception ex)
        {
            result.ErrorCode = ErrorCodes.GeneralFail;
            result.Message = $"Error calculating rating stats: {ex.Message}";
        }

        return result;
    }
}
