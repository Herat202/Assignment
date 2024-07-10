// using System.Diagnostics;

// namespace Assignment.Models;

// [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
// public record Photo(string Title, string ImageUrl, string Tags)
// {
//     private string GetDebuggerDisplay()
//     {
//         return ToString();
//     }
// }

namespace Assignment.Models;

public record Photo(int PhotoId, string Title, string Description, string ImageUrl, DateTime DateFetched, string Tags);
