﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Hash
    {
        [Key]
        [ForeignKey(nameof(Image))]
        public int imageID { get; set; }
        public int hash { get; set; }
    }
}
