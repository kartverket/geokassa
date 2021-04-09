using System;

namespace gridfiles
{
    public interface IItem
    {       
        Item.NameType Name { get; set; }

        string Role { get; set; }
        
        string Sample { get; set; }
    }
}
