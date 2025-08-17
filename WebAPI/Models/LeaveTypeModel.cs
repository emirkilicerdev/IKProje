public class LeaveTypeModel
{
    public int Id { get; set; }          // 1, 2, 3 gibi enum değerleri
    public string Name { get; set; }     // "Annual", "Sick", "Unpaid"

    public ICollection<LeaveRequest> LeaveRequests { get; set; }
}
