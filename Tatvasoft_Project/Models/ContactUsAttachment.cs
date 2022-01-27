using System;
using System.Collections.Generic;

#nullable disable

namespace Tatvasoft_Project.Models
{
    public partial class ContactUsAttachment
    {
        public int ContactUsAttachmentId { get; set; }
        public string Name { get; set; }
        public byte[] FileName { get; set; }
    }
}
