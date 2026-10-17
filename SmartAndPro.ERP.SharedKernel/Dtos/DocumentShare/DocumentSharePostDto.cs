using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare
{
    public class DocumentSharePostDto
    {
        public string? Method { get; set; }   
        public string Receiver { get; set; } 
    }
}
