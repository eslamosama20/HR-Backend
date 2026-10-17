using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Models.PublicBooking;

public class HotelReviewDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public Guid? BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime StayDate { get; set; }
    public bool IsVerified { get; set; }
    public bool IsApproved { get; set; }
    public bool IsVisible { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime? AdminResponseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHotelReviewDto
{
    [Required(ErrorMessage = "Hotel ID is required")]
    public Guid HotelId { get; set; }

    /// <summary>
    /// Optional - if provided, review will be marked as verified
    /// </summary>
    public Guid? BookingId { get; set; }

    [Required(ErrorMessage = "Guest name is required")]
    [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string GuestEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Comment is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Title must be less than 200 characters")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Stay date is required")]
    public DateTime StayDate { get; set; }
}

public class UpdateReviewResponseDto
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Response must be between 5 and 500 characters")]
    public string AdminResponse { get; set; } = string.Empty;
}

public class ApproveReviewDto
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    public bool IsApproved { get; set; }
}

