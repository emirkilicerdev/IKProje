public class LeaveRequestResponseDto
{
    public int Id { get; set; }
    public int LeaveTypeId { get; set; }       // EKLENDİ
    public string LeaveTypeName { get; set; }
    public int LeaveStatusId { get; set; }     // EKLENDİ
    public string LeaveStatusName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }
    public int UserId { get; set; }            // EKLENDİ
    public string UserName { get; set; }
    public int? ApprovedById { get; set; }     // EKLENDİ
    public string ApprovedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
