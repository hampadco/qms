 public class Category: Parent
{
    public string CatName { get; set; }

    public int ParentId { get; set; }
    public int CatCode { get; set; }
    public bool Status { get; set; }

    public ICollection<Users> Subs { get; set; }
}