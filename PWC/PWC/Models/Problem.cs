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
    
    public partial class Problem
    {
        public Problem()
        {
            this.OCProblem = new HashSet<OCProblem>();
            this.UserProblem = new HashSet<UserProblem>();
            this.TestData = new HashSet<TestData>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public short Difficulty { get; set; }
        public string Describe { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string SampleInput { get; set; }
        public string SampleOutput { get; set; }
        public string Source { get; set; }
        public short Evaluate { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int CreatorID { get; set; }
        public int Answers { get; set; }
        public int TypeID { get; set; }
        public int TimeLimit { get; set; }
        public int MemLimit { get; set; }
    
        public virtual Admin Admin { get; set; }
        public virtual ICollection<OCProblem> OCProblem { get; set; }
        public virtual ProblemType ProblemType { get; set; }
        public virtual ICollection<UserProblem> UserProblem { get; set; }
        public virtual ICollection<TestData> TestData { get; set; }
    }
}
