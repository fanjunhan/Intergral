using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Services.WebApi
{
    public class AppParameterDto
    {
        [Display(Name = "请求参数")]
        [Required(ErrorMessage = "{0}是必须的")]
        public string Value { get; set; }
    }
}
