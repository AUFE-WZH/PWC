//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace PWC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RecordUser
    {
        public int ID { get; set; }
        public int RecordID { get; set; }
        public int UserID { get; set; }
        public System.DateTime LoginTime { get; set; }
    
        public virtual Record Record { get; set; }
        public virtual User User { get; set; }
    }
}
