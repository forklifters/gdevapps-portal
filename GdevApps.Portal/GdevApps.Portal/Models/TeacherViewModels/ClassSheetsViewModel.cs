using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class ClassSheetsViewModel
{
    
    public string Id { get; set; }

    [Required]
    [DisplayName("Google sheet name")]
    public string Name { get; set; }

    [Required]
    [DisplayName("Google sheet link")]
    public string Link { get; set; }

    [Required]
    [DisplayName("Class id")]
    public string ClassroomId { get; set; }

    [Required]
    [DisplayName("Google sheet unique id")]
    public string GoogleUniqueId { get; set; }
}