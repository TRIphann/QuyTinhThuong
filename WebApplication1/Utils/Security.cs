using System.Security.Cryptography; using System.Text;
namespace QLDuLichRBAC.Utils{
 public static class Security{ public static string HashPassword(string p){ using var sha=SHA256.Create(); var b=sha.ComputeHash(Encoding.UTF8.GetBytes(p)); return Convert.ToHexString(b); } } }
