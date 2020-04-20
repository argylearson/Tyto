﻿using System.ComponentModel.DataAnnotations;

namespace SodalisCore.DataTransferObjects {
    public class LoginDto {
        [Required]
        [StringLength(128)]
        public string EmailAddress;
        [Required]
        [StringLength(128)]
        public string Password;
    }
}