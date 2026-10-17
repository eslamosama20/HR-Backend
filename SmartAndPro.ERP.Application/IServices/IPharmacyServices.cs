using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using Microsoft.AspNetCore.Http;

namespace SmartAndPro.ERP.Application.IServices
{
    public interface IPharmacyPrescriptionService
    {
        /// <summary>
        /// Get paginated list of prescriptions with filtering
        /// </summary>
        Task<PrescriptionListResponseDto> GetPrescriptionsAsync(PrescriptionListQueryDto query);

        /// <summary>
        /// Get prescription details by ID
        /// </summary>
        Task<PrescriptionDetailsDto?> GetPrescriptionByIdAsync(int id);

        /// <summary>
        /// Create a new prescription
        /// </summary>
        Task<CreatePrescriptionResponse> CreatePrescriptionAsync(CreatePrescriptionRequest request, string userId);

        /// <summary>
        /// Update an existing prescription
        /// </summary>
        Task<UpdatePrescriptionResponse> UpdatePrescriptionAsync(int id, UpdatePrescriptionRequest request, string userId);

        /// <summary>
        /// Update prescription status
        /// </summary>
        Task<UpdatePrescriptionStatusResponse> UpdatePrescriptionStatusAsync(int id, UpdatePrescriptionStatusRequest request, string userId);

        /// <summary>
        /// Delete/Cancel a prescription
        /// </summary>
        Task<bool> DeletePrescriptionAsync(int id, string userId);

        /// <summary>
        /// Check for drug interactions
        /// </summary>
        Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request);

        /// <summary>
        /// Update insurance information for a prescription
        /// </summary>
        Task<UpdateInsuranceResponse> UpdateInsuranceAsync(int id, UpdateInsuranceRequest request, string userId);

        /// <summary>
        /// Mark prescription as ready
        /// </summary>
        Task<MarkReadyResponse> MarkAsReadyAsync(int id, MarkReadyRequest request, string userId);

        /// <summary>
        /// Send prescription to POS
        /// </summary>
        Task<SendToPosResponse> SendToPosAsync(int id, SendToPosRequest request, string userId);

        /// <summary>
        /// Cancel a prescription
        /// </summary>
        Task<CancelPrescriptionResponse> CancelPrescriptionAsync(int id, CancelPrescriptionRequest request, string userId);

        /// <summary>
        /// Upload attachment to prescription
        /// </summary>
        Task<UploadAttachmentResponse> UploadAttachmentAsync(int id, IFormFile file, string? notes, string userId);

        /// <summary>
        /// Get attachments for a prescription
        /// </summary>
        Task<List<AttachmentDto>> GetAttachmentsAsync(int id);

        /// <summary>
        /// Delete an attachment
        /// </summary>
        Task<bool> DeleteAttachmentAsync(int prescriptionId, int attachmentId, string userId);

        /// <summary>
        /// Get ready prescriptions for cashier
        /// </summary>
        Task<ReadyPrescriptionsResponseDto> GetReadyPrescriptionsAsync(ReadyPrescriptionsQueryDto query);
    }

    public interface IPharmacyItemService
    {
        /// <summary>
        /// Search for drugs/items
        /// </summary>
        Task<ItemLookupResponseDto> SearchItemsAsync(ItemLookupQueryDto query);

        /// <summary>
        /// Get available batches for an item
        /// </summary>
        Task<BatchLookupResponseDto> GetItemBatchesAsync(BatchLookupQueryDto query);

        /// <summary>
        /// Check stock availability
        /// </summary>
        Task<StockCheckResponse> CheckStockAsync(StockCheckRequest request);
    }

    public interface IPharmacyInsuranceService
    {
        /// <summary>
        /// Verify insurance coverage
        /// </summary>
        Task<InsuranceCheckResponse> CheckInsuranceCoverageAsync(InsuranceCheckRequest request);

        /// <summary>
        /// Submit insurance approval
        /// </summary>
        Task<InsuranceApprovalResponse> SubmitInsuranceApprovalAsync(InsuranceApprovalRequest request);
    }

    public interface IPharmacyPatientService
    {
        /// <summary>
        /// Search for patients
        /// </summary>
        Task<PatientListResponseDto> SearchPatientsAsync(PatientSearchQueryDto query);

        /// <summary>
        /// Get patient details
        /// </summary>
        Task<PatientDetailsDto?> GetPatientByIdAsync(int id);

        /// <summary>
        /// Create new patient
        /// </summary>
        Task<CreatePatientResponse> CreatePatientAsync(CreatePatientRequest request, string userId);
    }

    public interface IPharmacyDoctorService
    {
        /// <summary>
        /// Search for doctors
        /// </summary>
        Task<DoctorListResponseDto> SearchDoctorsAsync(DoctorSearchQueryDto query);
    }

    public interface IPharmacyLookupService
    {
        /// <summary>
        /// Get stores by branch
        /// </summary>
        Task<List<StoreLookupDto>> GetStoresAsync(StoresQueryDto query);

        /// <summary>
        /// Search insurance companies
        /// </summary>
        Task<List<InsuranceCompanyLookupDto>> SearchInsuranceCompaniesAsync(InsuranceCompaniesQueryDto query);
    }
}
