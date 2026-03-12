

using System.ComponentModel.DataAnnotations.Schema;

[Table("Users")]
public class UserCreateStoreDTO
{
   
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Alias { get; set; } = "";
    public UserCreateStoreDTO()
    {
    }
}


[Table("Shop")]
public class ShopCreateStoreDTO
{
   
    // public string Username { get; set; } = "";
    // public string FullName { get; set; } = "";
    // public string PasswordHash { get; set; } = "";
    // public string Email { get; set; } = "";
    // public string Phone { get; set; } = "";
    // public string Alias { get; set; } = "";
    // public UserCreateStoreDTO()
    // {
    // }
}