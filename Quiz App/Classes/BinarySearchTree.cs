using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quiz_App.QuizSelectorEdit;

namespace Quiz_App.Classes
{
    public class BinarySearchTree
    {
        private TreeNode root;

        //If a quiz doesn't exist at root node, then create a new root node
        public void InsertQuiz(Quiz data)
        {
            if (root == null)
            {
                root = new TreeNode(data);
            }
            else
            {
                InsertRecursively(data, root);
            }
        }

        //Recursively insert new quizzes into the tree
        public void InsertRecursively(Quiz data, TreeNode root)
        {
            if (data.Id < root.Data.Id)
            {
                if (root.Left == null)
                {
                    root.Left = new TreeNode(data);
                }
                else
                {
                    InsertRecursively(data, root.Left);
                }
            }
            else
            {
                if (root.Right == null)
                {
                    root.Right = new TreeNode(data);
                }
                else
                {
                    InsertRecursively(data, root.Right);
                }
            }
        }

        public Quiz SearchQuiz(int id)
        {
            return SearchRecursively(root, id);
        }

        public Quiz SearchRecursively(TreeNode root, int id)
        {
            //Checks if the root node is null or id matches up
            if (root == null || root.Data.Id == id)
            {
                //returns null if root is null, otherwise it returns the quiz id
                return root?.Data;
            }

            if (id < root.Data?.Id)
            {
                return SearchRecursively(root.Left, id);
            }
            else
            {
                return SearchRecursively(root.Right, id);
            }
        }
    }
}
