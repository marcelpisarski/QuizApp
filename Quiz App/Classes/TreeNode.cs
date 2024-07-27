using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_App.Classes
{
    public class TreeNode
    {
        //Defines a node
        public Quiz Data { get; set; }
        public TreeNode Left { get; set; }
        public TreeNode Right { get; set; }

        //Sets up pointers
        public TreeNode(Quiz data)
        {
            Data = data;
            Left = null;
            Right = null;
        }
    }
}
