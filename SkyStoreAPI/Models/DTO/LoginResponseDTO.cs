namespace SkyStoreAPI.Models.DTO
{
    public class LoginResponseDTO
    {
        public UserDTO user { get; set; }
        public string Token { get; set; }
    }
}
