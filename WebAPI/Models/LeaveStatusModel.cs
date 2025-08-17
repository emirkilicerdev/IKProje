public class LeaveStatusModel
{
    public int Id { get; set; }          // 1, 2, 3 gibi enum değerleri
    public string Name { get; set; }     // "Pending", "Approved", "Rejected"

    public ICollection<LeaveRequest> LeaveRequests { get; set; }
}
