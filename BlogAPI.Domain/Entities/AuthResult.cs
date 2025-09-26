
using BlogAPI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogAPI.Domain.Models;

public record AuthResult
{
        public bool Success { get; set; }
        public string[] Errors {get; set; } = Array.Empty<string>();
        public IUserInfo User { get; set; }
        public string Token { get; set; }
        
}
