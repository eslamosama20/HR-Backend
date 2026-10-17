using SmartAndPro.ERP.Application.Common.Dtos.Projects;

namespace SmartAndPro.ERP.Application.IServices.Projects;

public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid currentUserId);
    Task<ProjectResponse> UpdateProjectAsync(UpdateProjectRequest request, Guid currentUserId);
    Task<bool> DeleteProjectAsync(Guid id, Guid currentUserId);
    Task<ProjectResponse?> GetProjectByIdAsync(Guid id);
    Task<ProjectDetailsResponse?> GetProjectDetailsAsync(Guid id);
    Task<List<ProjectResponse>> GetAllProjectsAsync(bool includeInactive = false);
    Task<List<ProjectResponse>> GetProjectsByStatusAsync(string status);
    Task<bool> ChangeProjectStatusAsync(Guid id, string newStatus, Guid currentUserId);
    
    // Assignment Phase
    Task<object> AddAssignmentPhaseAsync(CreateProjectAssignmentPhaseRequest request, Guid currentUserId);
    Task<object> UpdateAssignmentPhaseAsync(CreateProjectAssignmentPhaseRequest request, Guid currentUserId);
    
    // Execution Phase
    Task<object> AddExecutionPhaseAsync(CreateProjectExecutionPhaseRequest request, Guid currentUserId);
    Task<object> UpdateExecutionPhaseAsync(CreateProjectExecutionPhaseRequest request, Guid currentUserId);
    
    // Documents
    Task<object> AddDocumentAsync(CreateProjectDocumentRequest request, Guid currentUserId);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid currentUserId);
    Task<object> UploadDocumentAsync(Guid projectId, string documentType, string documentName, string fileName, byte[] fileBytes, string contentType, Guid currentUserId);
    Task<bool> UpdateDocumentNameAsync(Guid documentId, string newDocumentName, Guid currentUserId);
    
    // Payments
    Task<object> AddPaymentAsync(CreateProjectPaymentRequest request, Guid currentUserId);
    Task<object> UpdatePaymentAsync(UpdateProjectPaymentRequest request, Guid currentUserId);
    Task<bool> DeletePaymentAsync(Guid paymentId, Guid currentUserId);

    // Price Quotes
    Task<object> AddPriceQuoteAsync(CreateProjectPriceQuoteRequest request, Guid currentUserId);
    Task<object> UpdatePriceQuoteAsync(UpdateProjectPriceQuoteRequest request, Guid currentUserId);
    Task<bool> DeletePriceQuoteAsync(Guid quoteId, Guid currentUserId);
    Task<bool> AcceptPriceQuoteAsync(Guid quoteId, Guid currentUserId);

    // Assessments
    Task<object> AddAssessmentAsync(CreateProjectAssessmentRequest request, Guid currentUserId);
    Task<object> UpdateAssessmentAsync(UpdateProjectAssessmentRequest request, Guid currentUserId);
    Task<bool> DeleteAssessmentAsync(Guid assessmentId, Guid currentUserId);

    // Milestones
    Task<ProjectExecutionMilestoneResponse> ToggleMilestoneCompletionAsync(Guid milestoneId, bool isCompleted, Guid currentUserId);

    // Custom Statuses
    Task<List<string>> GetProjectStatusesAsync();
    Task<List<string>> AddCustomProjectStatusAsync(string status);
}
